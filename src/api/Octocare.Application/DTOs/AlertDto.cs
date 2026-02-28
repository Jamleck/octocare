namespace Octocare.Application.DTOs;

public record AlertDto(
    Guid Id,
    Guid PlanId,
    Guid? BudgetCategoryId,
    string AlertType,
    string Severity,
    string Message,
    bool IsRead,
    bool IsDismissed,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt,
    string? Data);

public record AlertSummaryDto(
    int Total,
    int UnreadInfo,
    int UnreadWarning,
    int UnreadCritical);
