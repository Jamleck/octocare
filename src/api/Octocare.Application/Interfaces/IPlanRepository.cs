using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Plan?> GetByIdWithBudgetCategoriesAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Plan>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default);
    Task<Plan?> GetActivePlanForParticipantAsync(Guid participantId, CancellationToken ct = default);
    Task<bool> ExistsByPlanNumberAsync(string planNumber, Guid? excludeId = null, CancellationToken ct = default);
    Task<Plan> AddAsync(Plan plan, CancellationToken ct = default);
    Task UpdateAsync(Plan plan, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
    Task AddBudgetCategoryAsync(BudgetCategory category, CancellationToken ct = default);
    Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken ct = default);
}
