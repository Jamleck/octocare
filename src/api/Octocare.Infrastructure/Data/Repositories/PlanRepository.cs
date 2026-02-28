using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly OctocareDbContext _db;

    public PlanRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Plans
            .Include(p => p.Participant)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Plan?> GetByIdWithBudgetCategoriesAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Plans
            .Include(p => p.Participant)
            .Include(p => p.BudgetCategories)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Plan>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Plans
            .Include(p => p.Participant)
            .Include(p => p.BudgetCategories)
            .Where(p => p.ParticipantId == participantId)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(ct);
    }

    public async Task<Plan?> GetActivePlanForParticipantAsync(Guid participantId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Plans
            .Include(p => p.BudgetCategories)
            .FirstOrDefaultAsync(p => p.ParticipantId == participantId &&
                (p.Status == PlanStatus.Active || p.Status == PlanStatus.Expiring), ct);
    }

    public async Task<bool> ExistsByPlanNumberAsync(string planNumber, Guid? excludeId = null, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var query = _db.Plans.Where(p => p.PlanNumber == planNumber);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<Plan> AddAsync(Plan plan, CancellationToken ct = default)
    {
        _db.Plans.Add(plan);
        await _db.SaveChangesAsync(ct);
        return plan;
    }

    public async Task UpdateAsync(Plan plan, CancellationToken ct = default)
    {
        _db.Plans.Update(plan);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddBudgetCategoryAsync(BudgetCategory category, CancellationToken ct = default)
    {
        _db.BudgetCategories.Add(category);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Plan>> GetActivePlansAsync(CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Plans
            .Include(p => p.Participant)
            .Include(p => p.BudgetCategories)
            .Where(p => p.Status == PlanStatus.Active || p.Status == PlanStatus.Expiring)
            .ToListAsync(ct);
    }
}
