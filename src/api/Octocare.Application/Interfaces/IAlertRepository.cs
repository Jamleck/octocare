using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IAlertRepository
{
    Task<IReadOnlyList<BudgetAlert>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task<IReadOnlyList<BudgetAlert>> GetAllAsync(bool includeRead = false, bool includeDismissed = false, CancellationToken ct = default);
    Task<BudgetAlert?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(BudgetAlert alert, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<BudgetAlert> alerts, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
    Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default);

    /// <summary>
    /// Gets the count of unread, non-dismissed alerts grouped by severity.
    /// </summary>
    Task<(int Info, int Warning, int Critical)> GetUnreadCountsBySeverityAsync(CancellationToken ct = default);
}
