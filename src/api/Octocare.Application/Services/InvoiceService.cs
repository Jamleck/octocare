using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class InvoiceService
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IEventStore _eventStore;
    private readonly InvoiceValidationService _validationService;

    public InvoiceService(
        IInvoiceRepository invoiceRepo,
        ITenantContext tenantContext,
        IEventStore eventStore,
        InvoiceValidationService validationService)
    {
        _invoiceRepo = invoiceRepo;
        _tenantContext = tenantContext;
        _eventStore = eventStore;
        _validationService = validationService;
    }

    public async Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(id, ct);
        return invoice is null ? null : MapToDto(invoice);
    }

    public async Task<InvoicePagedResult> GetPagedAsync(int page, int pageSize,
        string? status = null, Guid? participantId = null, Guid? providerId = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _invoiceRepo.GetPagedAsync(page, pageSize, status, participantId, providerId, ct);
        return new InvoicePagedResult(
            items.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        if (await _invoiceRepo.ExistsByInvoiceNumberAsync(request.InvoiceNumber, ct: ct))
            throw new InvalidOperationException("An invoice with this invoice number already exists.");

        var invoice = Invoice.Create(tenantId, request.ProviderId, request.ParticipantId,
            request.PlanId, request.InvoiceNumber, request.ServicePeriodStart, request.ServicePeriodEnd,
            request.Notes);

        // Validate and create line items before saving
        var validationResults = await _validationService.ValidateLineItemsAsync(
            request.PlanId, request.LineItems, ct);

        foreach (var (lineItemReq, index) in request.LineItems.Select((li, i) => (li, i)))
        {
            var rateCents = Money.FromDollars(lineItemReq.Rate).Cents;
            var lineItem = InvoiceLineItem.Create(
                invoice.Id,
                lineItemReq.SupportItemNumber,
                lineItemReq.Description,
                lineItemReq.ServiceDate,
                lineItemReq.Quantity,
                rateCents,
                lineItemReq.BudgetCategoryId);

            // Apply validation result
            var validation = validationResults.FirstOrDefault(v => v.Index == index);
            if (validation is not null)
            {
                lineItem.UpdateValidation(validation.Status, validation.Message);
            }

            invoice.LineItems.Add(lineItem);
        }

        invoice.RecalculateTotal();

        // Save invoice and line items in a single operation
        await _invoiceRepo.AddAsync(invoice, ct);

        await _eventStore.AppendAsync(
            invoice.Id,
            "Invoice",
            "InvoiceSubmitted",
            new
            {
                invoice.InvoiceNumber,
                invoice.ProviderId,
                invoice.ParticipantId,
                invoice.PlanId,
                TotalAmountCents = invoice.TotalAmount,
                LineItemCount = invoice.LineItems.Count
            },
            0,
            null,
            ct);

        // Re-fetch with navigation properties
        var saved = await _invoiceRepo.GetByIdAsync(invoice.Id, ct);
        return MapToDto(saved!);
    }

    public async Task<InvoiceDto> ApproveAsync(Guid id, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Invoice not found.");

        invoice.Approve();
        await _invoiceRepo.UpdateAsync(invoice, ct);

        var events = await _eventStore.GetStreamAsync(invoice.Id, ct);
        await _eventStore.AppendAsync(
            invoice.Id,
            "Invoice",
            "InvoiceApproved",
            new { invoice.InvoiceNumber, invoice.Status },
            events.Count,
            null,
            ct);

        return MapToDto(invoice);
    }

    public async Task<InvoiceDto> RejectAsync(Guid id, string reason, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Invoice not found.");

        invoice.Reject(reason);
        await _invoiceRepo.UpdateAsync(invoice, ct);

        var events = await _eventStore.GetStreamAsync(invoice.Id, ct);
        await _eventStore.AppendAsync(
            invoice.Id,
            "Invoice",
            "InvoiceRejected",
            new { invoice.InvoiceNumber, invoice.Status, Reason = reason },
            events.Count,
            null,
            ct);

        return MapToDto(invoice);
    }

    public async Task<InvoiceDto> DisputeAsync(Guid id, string reason, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Invoice not found.");

        invoice.Dispute(reason);
        await _invoiceRepo.UpdateAsync(invoice, ct);

        var events = await _eventStore.GetStreamAsync(invoice.Id, ct);
        await _eventStore.AppendAsync(
            invoice.Id,
            "Invoice",
            "InvoiceDisputed",
            new { invoice.InvoiceNumber, invoice.Status, Reason = reason },
            events.Count,
            null,
            ct);

        return MapToDto(invoice);
    }

    public async Task<InvoiceDto> MarkPaidAsync(Guid id, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Invoice not found.");

        invoice.MarkPaid();
        await _invoiceRepo.UpdateAsync(invoice, ct);

        var events = await _eventStore.GetStreamAsync(invoice.Id, ct);
        await _eventStore.AppendAsync(
            invoice.Id,
            "Invoice",
            "InvoiceMarkedPaid",
            new { invoice.InvoiceNumber, invoice.Status },
            events.Count,
            null,
            ct);

        return MapToDto(invoice);
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.ProviderId,
            invoice.Provider?.Name ?? string.Empty,
            invoice.ParticipantId,
            invoice.Participant?.FullName ?? string.Empty,
            invoice.PlanId,
            invoice.Plan?.PlanNumber ?? string.Empty,
            invoice.InvoiceNumber,
            invoice.ServicePeriodStart,
            invoice.ServicePeriodEnd,
            new Money(invoice.TotalAmount).ToDollars(),
            invoice.Status,
            invoice.Source,
            invoice.Notes,
            invoice.LineItems.Select(MapLineItemToDto).ToList(),
            invoice.CreatedAt);
    }

    private static InvoiceLineItemDto MapLineItemToDto(InvoiceLineItem lineItem)
    {
        return new InvoiceLineItemDto(
            lineItem.Id,
            lineItem.SupportItemNumber,
            lineItem.Description,
            lineItem.ServiceDate,
            lineItem.Quantity,
            new Money(lineItem.Rate).ToDollars(),
            new Money(lineItem.Amount).ToDollars(),
            lineItem.BudgetCategoryId,
            lineItem.BudgetCategory?.SupportCategory.ToString(),
            lineItem.ValidationStatus,
            lineItem.ValidationMessage);
    }
}
