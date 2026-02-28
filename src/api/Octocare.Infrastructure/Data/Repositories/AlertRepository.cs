using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Infrastructure.Data.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly OctocareDbContext _db;

    public AlertRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BudgetAlert>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.BudgetAlerts
            .Where(a => a.PlanId == planId && !a.IsDismissed)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BudgetAlert>> GetAllAsync(
        bool includeRead = false, bool includeDismissed = false, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var query = _db.BudgetAlerts.AsQueryable();

        if (!includeDismissed)
            query = query.Where(a => !a.IsDismissed);
        if (!includeRead)
            query = query.Where(a => !a.IsRead);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<BudgetAlert?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.BudgetAlerts.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task AddAsync(BudgetAlert alert, CancellationToken ct = default)
    {
        _db.BudgetAlerts.Add(alert);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<BudgetAlert> alerts, CancellationToken ct = default)
    {
        _db.BudgetAlerts.AddRange(alerts);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var alerts = await _db.BudgetAlerts.Where(a => a.PlanId == planId).ToListAsync(ct);
        _db.BudgetAlerts.RemoveRange(alerts);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(int Info, int Warning, int Critical)> GetUnreadCountsBySeverityAsync(CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var counts = await _db.BudgetAlerts
            .Where(a => !a.IsRead && !a.IsDismissed)
            .GroupBy(a => a.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return (
            counts.FirstOrDefault(c => c.Severity == AlertSeverity.Info)?.Count ?? 0,
            counts.FirstOrDefault(c => c.Severity == AlertSeverity.Warning)?.Count ?? 0,
            counts.FirstOrDefault(c => c.Severity == AlertSeverity.Critical)?.Count ?? 0
        );
    }
}
