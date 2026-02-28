using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class ClaimService
{
    private readonly IClaimRepository _claimRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IOrganisationRepository _orgRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IEventStore _eventStore;
    private readonly NdiaCsvExporter _csvExporter;

    public ClaimService(
        IClaimRepository claimRepo,
        IInvoiceRepository invoiceRepo,
        IOrganisationRepository orgRepo,
        ITenantContext tenantContext,
        IEventStore eventStore,
        NdiaCsvExporter csvExporter)
    {
        _claimRepo = claimRepo;
        _invoiceRepo = invoiceRepo;
        _orgRepo = orgRepo;
        _tenantContext = tenantContext;
        _eventStore = eventStore;
        _csvExporter = csvExporter;
    }

    public async Task<ClaimDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var claim = await _claimRepo.GetByIdAsync(id, ct);
        return claim is null ? null : MapToDto(claim);
    }

    public async Task<ClaimPagedResult> GetPagedAsync(int page, int pageSize,
        string? status = null, CancellationToken ct = default)
    {
        var (items, totalCount) = await _claimRepo.GetPagedAsync(page, pageSize, status, ct);
        return new ClaimPagedResult(
            items.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize);
    }

    public async Task<ClaimDto> CreateBatchAsync(CreateClaimRequest request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        if (request.InvoiceLineItemIds is null || request.InvoiceLineItemIds.Count == 0)
            throw new InvalidOperationException("At least one invoice line item ID is required.");

        // Generate batch number
        var batchNumber = $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        var claim = Claim.Create(tenantId, batchNumber);

        // Validate all line items come from approved invoices
        foreach (var lineItemId in request.InvoiceLineItemIds)
        {
            var lineItem = ClaimLineItem.Create(claim.Id, lineItemId);
            claim.LineItems.Add(lineItem);
        }

        await _claimRepo.AddAsync(claim, ct);

        // Re-fetch with navigation properties to calculate total
        var saved = await _claimRepo.GetByIdAsync(claim.Id, ct);
        if (saved is not null)
        {
            saved.RecalculateTotal();
            await _claimRepo.UpdateAsync(saved, ct);
        }

        await _eventStore.AppendAsync(
            claim.Id,
            "Claim",
            "ClaimBatchCreated",
            new
            {
                claim.BatchNumber,
                LineItemCount = request.InvoiceLineItemIds.Count
            },
            0,
            null,
            ct);

        // Re-fetch again for the final DTO
        saved = await _claimRepo.GetByIdAsync(claim.Id, ct);
        return MapToDto(saved!);
    }

    public async Task<ClaimDto> SubmitAsync(Guid id, CancellationToken ct)
    {
        var claim = await _claimRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Claim not found.");

        claim.Submit();
        await _claimRepo.UpdateAsync(claim, ct);

        var events = await _eventStore.GetStreamAsync(claim.Id, ct);
        await _eventStore.AppendAsync(
            claim.Id,
            "Claim",
            "ClaimSubmitted",
            new { claim.BatchNumber, claim.Status },
            events.Count,
            null,
            ct);

        return MapToDto(claim);
    }

    public async Task<ClaimDto> RecordOutcomeAsync(Guid id, RecordClaimOutcomeRequest request, CancellationToken ct)
    {
        var claim = await _claimRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Claim not found.");

        if (claim.Status != ClaimStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot record outcome for a claim with status '{claim.Status}'. Claim must be in Submitted status.");

        if (!string.IsNullOrEmpty(request.NdiaReference))
            claim.SetNdiaReference(request.NdiaReference);

        // Process each line item outcome
        foreach (var outcome in request.LineItems)
        {
            var lineItem = claim.LineItems.FirstOrDefault(li => li.Id == outcome.LineItemId)
                ?? throw new KeyNotFoundException($"Claim line item '{outcome.LineItemId}' not found.");

            if (outcome.Status == ClaimLineItemStatus.Accepted)
                lineItem.Accept();
            else if (outcome.Status == ClaimLineItemStatus.Rejected)
                lineItem.Reject(outcome.RejectionReason ?? "No reason provided");
            else
                throw new InvalidOperationException($"Invalid line item status '{outcome.Status}'. Must be 'accepted' or 'rejected'.");
        }

        // Determine overall claim status based on line item outcomes
        var allProcessed = claim.LineItems.All(li =>
            li.Status == ClaimLineItemStatus.Accepted || li.Status == ClaimLineItemStatus.Rejected);

        if (allProcessed)
        {
            var allAccepted = claim.LineItems.All(li => li.Status == ClaimLineItemStatus.Accepted);
            var allRejected = claim.LineItems.All(li => li.Status == ClaimLineItemStatus.Rejected);

            if (allAccepted)
                claim.Accept();
            else if (allRejected)
                claim.Reject();
            else
                claim.PartiallyReject();
        }

        await _claimRepo.UpdateAsync(claim, ct);

        var events = await _eventStore.GetStreamAsync(claim.Id, ct);
        await _eventStore.AppendAsync(
            claim.Id,
            "Claim",
            "ClaimOutcomeRecorded",
            new
            {
                claim.BatchNumber,
                claim.Status,
                claim.NdiaReference,
                ProcessedLineItems = request.LineItems.Count
            },
            events.Count,
            null,
            ct);

        return MapToDto(claim);
    }

    public async Task<byte[]> GenerateCsvAsync(Guid id, CancellationToken ct)
    {
        var claim = await _claimRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Claim not found.");

        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        // org.Id == org.TenantId in this system
        var org = await _orgRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Organisation not found.");

        return _csvExporter.Generate(claim, org.Abn ?? string.Empty);
    }

    private static ClaimDto MapToDto(Claim claim)
    {
        return new ClaimDto(
            claim.Id,
            claim.BatchNumber,
            claim.Status,
            new Money(claim.TotalAmount).ToDollars(),
            claim.NdiaReference,
            claim.SubmissionDate,
            claim.ResponseDate,
            claim.LineItems.Select(MapLineItemToDto).ToList(),
            claim.CreatedAt);
    }

    private static ClaimLineItemDto MapLineItemToDto(ClaimLineItem lineItem)
    {
        var invoiceLineItem = lineItem.InvoiceLineItem;
        var invoice = invoiceLineItem?.Invoice;

        return new ClaimLineItemDto(
            lineItem.Id,
            lineItem.InvoiceLineItemId,
            invoiceLineItem?.SupportItemNumber ?? string.Empty,
            invoiceLineItem?.Description ?? string.Empty,
            invoiceLineItem?.ServiceDate ?? default,
            invoiceLineItem?.Quantity ?? 0,
            new Money(invoiceLineItem?.Rate ?? 0).ToDollars(),
            new Money(invoiceLineItem?.Amount ?? 0).ToDollars(),
            invoice?.InvoiceNumber ?? string.Empty,
            invoice?.Provider?.Name ?? string.Empty,
            invoice?.Participant?.FullName ?? string.Empty,
            lineItem.Status,
            lineItem.RejectionReason);
    }
}
