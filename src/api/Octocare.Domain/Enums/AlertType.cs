namespace Octocare.Domain.Enums;

public enum AlertType
{
    BudgetThreshold75,
    BudgetThreshold90,
    ProjectedOverspend,
    ProjectedUnderspend,
    PlanExpiry90Days,
    PlanExpiry60Days,
    PlanExpiry30Days,
    ServiceGap
}
