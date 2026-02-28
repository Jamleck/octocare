namespace Octocare.Application.DTOs;

public record PlanDto(
    Guid Id,
    Guid ParticipantId,
    string ParticipantName,
    string PlanNumber,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    IReadOnlyList<BudgetCategoryDto> BudgetCategories,
    DateTimeOffset CreatedAt);
