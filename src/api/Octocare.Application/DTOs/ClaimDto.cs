namespace Octocare.Application.DTOs;

public record ClaimDto(
    Guid Id,
    string BatchNumber,
    string Status,
    decimal TotalAmount,
    string? NdiaReference,
    DateOnly? SubmissionDate,
    DateOnly? ResponseDate,
    IReadOnlyList<ClaimLineItemDto> LineItems,
    DateTimeOffset CreatedAt);

public record ClaimLineItemDto(
    Guid Id,
    Guid InvoiceLineItemId,
    string SupportItemNumber,
    string Description,
    DateOnly ServiceDate,
    decimal Quantity,
    decimal Rate,
    decimal Amount,
    string InvoiceNumber,
    string ProviderName,
    string ParticipantName,
    string Status,
    string? RejectionReason);

public record CreateClaimRequest(
    List<Guid> InvoiceLineItemIds);

public record RecordClaimOutcomeRequest(
    string? NdiaReference,
    List<ClaimLineItemOutcome> LineItems);

public record ClaimLineItemOutcome(
    Guid LineItemId,
    string Status,
    string? RejectionReason);

public record ClaimPagedResult(
    IReadOnlyList<ClaimDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
