using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IBudgetProjectionRepository
{
    Task<IReadOnlyList<BudgetProjection>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task<BudgetProjection?> GetByCategoryIdAsync(Guid budgetCategoryId, CancellationToken ct = default);
    Task<BudgetProjection> AddAsync(BudgetProjection projection, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets committed amounts per budget category from active service bookings for a plan.
    /// </summary>
    Task<Dictionary<Guid, long>> GetCommittedAmountsByPlanAsync(Guid planId, CancellationToken ct = default);

    /// <summary>
    /// Gets spent amounts per budget category from approved/paid invoice line items for a plan.
    /// </summary>
    Task<Dictionary<Guid, long>> GetSpentAmountsByPlanAsync(Guid planId, CancellationToken ct = default);

    /// <summary>
    /// Gets pending amounts per budget category from submitted/under_review invoice line items for a plan.
    /// </summary>
    Task<Dictionary<Guid, long>> GetPendingAmountsByPlanAsync(Guid planId, CancellationToken ct = default);
}
