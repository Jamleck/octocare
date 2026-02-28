namespace Octocare.Application.DTOs;

public record PaymentBatchDto(
    Guid Id,
    string BatchNumber,
    string Status,
    decimal TotalAmount,
    int ItemCount,
    string? AbaFileUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? ConfirmedAt);

public record PaymentBatchDetailDto(
    Guid Id,
    string BatchNumber,
    string Status,
    decimal TotalAmount,
    int ItemCount,
    string? AbaFileUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? ConfirmedAt,
    IReadOnlyList<PaymentItemDto> Items);

public record PaymentItemDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    decimal Amount,
    int InvoiceCount,
    string InvoiceIds);

public record PaymentBatchPagedResult(
    IReadOnlyList<PaymentBatchDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
