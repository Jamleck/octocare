namespace Octocare.Application.DTOs;

public record BudgetOverviewDto(
    Guid PlanId,
    string PlanNumber,
    decimal TotalAllocated,
    decimal TotalCommitted,
    decimal TotalSpent,
    decimal TotalPending,
    decimal TotalAvailable,
    decimal UtilisationPercentage,
    IReadOnlyList<BudgetCategoryProjectionDto> Categories);

public record BudgetCategoryProjectionDto(
    Guid CategoryId,
    string SupportCategory,
    string SupportPurpose,
    decimal Allocated,
    decimal Committed,
    decimal Spent,
    decimal Pending,
    decimal Available,
    decimal UtilisationPercentage);
