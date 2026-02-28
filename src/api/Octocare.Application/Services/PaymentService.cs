using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IOrganisationRepository _orgRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IEventStore _eventStore;
    private readonly AbaFileGenerator _abaGenerator;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IInvoiceRepository invoiceRepo,
        IOrganisationRepository orgRepo,
        ITenantContext tenantContext,
        IEventStore eventStore,
        AbaFileGenerator abaGenerator)
    {
        _paymentRepo = paymentRepo;
        _invoiceRepo = invoiceRepo;
        _orgRepo = orgRepo;
        _tenantContext = tenantContext;
        _eventStore = eventStore;
        _abaGenerator = abaGenerator;
    }

    public async Task<PaymentBatchDetailDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var batch = await _paymentRepo.GetByIdAsync(id, ct);
        return batch is null ? null : MapToDetailDto(batch);
    }

    public async Task<PaymentBatchPagedResult> GetPagedAsync(int page, int pageSize,
        string? status = null, CancellationToken ct = default)
    {
        var (items, totalCount) = await _paymentRepo.GetPagedAsync(page, pageSize, status, ct);
        return new PaymentBatchPagedResult(
            items.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize);
    }

    public async Task<PaymentBatchDetailDto> CreateBatchAsync(CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        // Fetch all approved invoices (status = approved or paid) grouped by provider
        var (approvedInvoices, _) = await _invoiceRepo.GetPagedAsync(1, 1000, InvoiceStatus.Approved, ct: ct);
        var (paidInvoices, _) = await _invoiceRepo.GetPagedAsync(1, 1000, InvoiceStatus.Paid, ct: ct);

        var allInvoices = approvedInvoices.Concat(paidInvoices).ToList();

        if (allInvoices.Count == 0)
            throw new InvalidOperationException("No approved or paid invoices available for payment.");

        // Group by provider
        var grouped = allInvoices.GroupBy(i => new { i.ProviderId, i.Provider.Name });

        var batchNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";
        var batch = PaymentBatch.Create(tenantId, batchNumber);

        foreach (var group in grouped)
        {
            var totalAmount = group.Sum(i => i.TotalAmount);
            var invoiceIds = string.Join(",", group.Select(i => i.Id));

            var item = PaymentItem.Create(
                batch.Id,
                group.Key.ProviderId,
                group.Key.Name,
                totalAmount,
                invoiceIds);

            batch.AddItem(item);
        }

        await _paymentRepo.AddAsync(batch, ct);

        await _eventStore.AppendAsync(
            batch.Id,
            "PaymentBatch",
            "PaymentBatchCreated",
            new
            {
                batch.BatchNumber,
                batch.TotalAmount,
                ItemCount = batch.Items.Count,
                InvoiceCount = allInvoices.Count
            },
            0,
            null,
            ct);

        // Re-fetch with navigations
        var saved = await _paymentRepo.GetByIdAsync(batch.Id, ct);
        return MapToDetailDto(saved!);
    }

    public async Task<string> GenerateAbaAsync(Guid id, CancellationToken ct)
    {
        var batch = await _paymentRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Payment batch not found.");

        if (batch.Status != Domain.Enums.PaymentBatchStatus.Draft &&
            batch.Status != Domain.Enums.PaymentBatchStatus.Generated)
            throw new InvalidOperationException(
                $"Cannot generate ABA for a batch with status '{batch.Status}'.");

        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var org = await _orgRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Organisation not found.");

        // Use organisation details for the sender info
        var companyName = org.Name;

        // Generate ABA content
        var abaContent = _abaGenerator.Generate(
            batch,
            bankBsb: "032-000",           // Default/placeholder BSB
            bankAccountNumber: "000000000", // Placeholder
            bankAccountName: companyName,
            companyName: companyName);

        // Mark as generated
        if (batch.Status == Domain.Enums.PaymentBatchStatus.Draft)
        {
            batch.MarkGenerated();
            await _paymentRepo.UpdateAsync(batch, ct);
        }

        return abaContent;
    }

    public async Task<PaymentBatchDetailDto> MarkSentAsync(Guid id, CancellationToken ct)
    {
        var batch = await _paymentRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Payment batch not found.");

        batch.MarkSent();
        await _paymentRepo.UpdateAsync(batch, ct);

        var events = await _eventStore.GetStreamAsync(batch.Id, ct);
        await _eventStore.AppendAsync(
            batch.Id,
            "PaymentBatch",
            "PaymentBatchSent",
            new { batch.BatchNumber, batch.Status, batch.SentAt },
            events.Count,
            null,
            ct);

        return MapToDetailDto(batch);
    }

    public async Task<PaymentBatchDetailDto> MarkConfirmedAsync(Guid id, CancellationToken ct)
    {
        var batch = await _paymentRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Payment batch not found.");

        batch.MarkConfirmed();
        await _paymentRepo.UpdateAsync(batch, ct);

        var events = await _eventStore.GetStreamAsync(batch.Id, ct);
        await _eventStore.AppendAsync(
            batch.Id,
            "PaymentBatch",
            "PaymentBatchConfirmed",
            new { batch.BatchNumber, batch.Status, batch.ConfirmedAt },
            events.Count,
            null,
            ct);

        return MapToDetailDto(batch);
    }

    private static PaymentBatchDto MapToDto(PaymentBatch batch)
    {
        return new PaymentBatchDto(
            batch.Id,
            batch.BatchNumber,
            batch.Status,
            new Money(batch.TotalAmount).ToDollars(),
            batch.Items.Count,
            batch.AbaFileUrl,
            batch.CreatedAt,
            batch.SentAt,
            batch.ConfirmedAt);
    }

    private static PaymentBatchDetailDto MapToDetailDto(PaymentBatch batch)
    {
        return new PaymentBatchDetailDto(
            batch.Id,
            batch.BatchNumber,
            batch.Status,
            new Money(batch.TotalAmount).ToDollars(),
            batch.Items.Count,
            batch.AbaFileUrl,
            batch.CreatedAt,
            batch.SentAt,
            batch.ConfirmedAt,
            batch.Items.Select(MapItemToDto).ToList());
    }

    private static PaymentItemDto MapItemToDto(PaymentItem item)
    {
        var invoiceIds = item.InvoiceIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return new PaymentItemDto(
            item.Id,
            item.ProviderId,
            item.ProviderName,
            new Money(item.Amount).ToDollars(),
            invoiceIds.Length,
            item.InvoiceIds);
    }
}
