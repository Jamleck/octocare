using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class PlanTransitionRepository : IPlanTransitionRepository
{
    private readonly OctocareDbContext _db;

    public PlanTransitionRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<PlanTransition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.PlanTransitions
            .Include(t => t.OldPlan)
            .Include(t => t.NewPlan)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<PlanTransition>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.PlanTransitions
            .Include(t => t.OldPlan)
            .Include(t => t.NewPlan)
            .Where(t => t.OldPlanId == planId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlanTransition>> GetAllAsync(CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.PlanTransitions
            .Include(t => t.OldPlan)
            .Include(t => t.NewPlan)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(PlanTransition transition, CancellationToken ct = default)
    {
        _db.PlanTransitions.Add(transition);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
