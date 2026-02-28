namespace Octocare.Application.DTOs;

public record PlanTransitionDto(
    Guid Id,
    Guid OldPlanId,
    string OldPlanNumber,
    Guid? NewPlanId,
    string? NewPlanNumber,
    string Status,
    IReadOnlyList<TransitionChecklistItemDto> ChecklistItems,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public record TransitionChecklistItemDto(
    string Label,
    bool Completed);

public record InitiateTransitionRequest(
    Guid OldPlanId);

public record UpdateTransitionRequest(
    IReadOnlyList<TransitionChecklistItemDto> ChecklistItems,
    string? Notes);
