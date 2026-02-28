namespace Octocare.Application.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    string? Link,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);

public record NotificationPagedResult(
    IReadOnlyList<NotificationDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record UnreadCountDto(int Count);
