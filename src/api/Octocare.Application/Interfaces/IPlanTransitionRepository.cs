using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IPlanTransitionRepository
{
    Task<PlanTransition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PlanTransition>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task<IReadOnlyList<PlanTransition>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(PlanTransition transition, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
