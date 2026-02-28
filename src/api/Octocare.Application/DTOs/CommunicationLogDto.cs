namespace Octocare.Application.DTOs;

public record CommunicationLogDto(
    Guid Id,
    string RecipientEmail,
    string Subject,
    string? TemplateName,
    DateTimeOffset SentAt,
    string Status,
    string? ErrorMessage,
    string? RelatedEntityType,
    string? RelatedEntityId);

public record CommunicationLogPagedResult(
    IReadOnlyList<CommunicationLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
