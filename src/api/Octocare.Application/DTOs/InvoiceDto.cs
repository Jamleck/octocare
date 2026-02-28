namespace Octocare.Application.DTOs;

public record InvoiceDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    Guid ParticipantId,
    string ParticipantName,
    Guid PlanId,
    string PlanNumber,
    string InvoiceNumber,
    DateOnly ServicePeriodStart,
    DateOnly ServicePeriodEnd,
    decimal TotalAmount,
    string Status,
    string Source,
    string? Notes,
    IReadOnlyList<InvoiceLineItemDto> LineItems,
    DateTimeOffset CreatedAt);

public record InvoiceLineItemDto(
    Guid Id,
    string SupportItemNumber,
    string Description,
    DateOnly ServiceDate,
    decimal Quantity,
    decimal Rate,
    decimal Amount,
    Guid? BudgetCategoryId,
    string? SupportCategory,
    string ValidationStatus,
    string? ValidationMessage);

public record CreateInvoiceRequest(
    Guid ProviderId,
    Guid ParticipantId,
    Guid PlanId,
    string InvoiceNumber,
    DateOnly ServicePeriodStart,
    DateOnly ServicePeriodEnd,
    string? Notes,
    List<CreateInvoiceLineItemRequest> LineItems);

public record CreateInvoiceLineItemRequest(
    string SupportItemNumber,
    string Description,
    DateOnly ServiceDate,
    decimal Quantity,
    decimal Rate,
    Guid? BudgetCategoryId);

public record RejectInvoiceRequest(string Reason);

public record DisputeInvoiceRequest(string Reason);

public record InvoicePagedResult(
    IReadOnlyList<InvoiceDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
