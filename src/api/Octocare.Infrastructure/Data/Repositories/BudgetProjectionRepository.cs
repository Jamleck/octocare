using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class BudgetProjectionRepository : IBudgetProjectionRepository
{
    private readonly OctocareDbContext _db;

    public BudgetProjectionRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BudgetProjection>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);

        // Get budget category IDs for this plan
        var categoryIds = await _db.BudgetCategories
            .Where(bc => bc.PlanId == planId)
            .Select(bc => bc.Id)
            .ToListAsync(ct);

        return await _db.BudgetProjections
            .Include(bp => bp.BudgetCategory)
            .Where(bp => categoryIds.Contains(bp.BudgetCategoryId))
            .ToListAsync(ct);
    }

    public async Task<BudgetProjection?> GetByCategoryIdAsync(Guid budgetCategoryId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.BudgetProjections
            .Include(bp => bp.BudgetCategory)
            .FirstOrDefaultAsync(bp => bp.BudgetCategoryId == budgetCategoryId, ct);
    }

    public async Task<BudgetProjection> AddAsync(BudgetProjection projection, CancellationToken ct = default)
    {
        _db.BudgetProjections.Add(projection);
        // Don't save here — let the caller batch saves
        return projection;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Dictionary<Guid, long>> GetCommittedAmountsByPlanAsync(Guid planId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);

        // Get budget category IDs for this plan
        var categoryIds = await _db.BudgetCategories
            .Where(bc => bc.PlanId == planId)
            .Select(bc => bc.Id)
            .ToListAsync(ct);

        // Sum allocated amounts from active service bookings per budget category
        return await _db.ServiceBookings
            .Where(sb => categoryIds.Contains(sb.BudgetCategoryId)
                && sb.Status == ServiceBookingStatus.Active)
            .GroupBy(sb => sb.BudgetCategoryId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Sum(sb => sb.AllocatedAmount),
                ct);
    }

    public async Task<Dictionary<Guid, long>> GetSpentAmountsByPlanAsync(Guid planId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);

        // Sum line item amounts from approved or paid invoices for this plan
        return await _db.InvoiceLineItems
            .Where(li => li.BudgetCategoryId != null
                && li.Invoice.PlanId == planId
                && (li.Invoice.Status == InvoiceStatus.Approved || li.Invoice.Status == InvoiceStatus.Paid))
            .GroupBy(li => li.BudgetCategoryId!.Value)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Sum(li => li.Amount),
                ct);
    }

    public async Task<Dictionary<Guid, long>> GetPendingAmountsByPlanAsync(Guid planId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);

        // Sum line item amounts from submitted or under_review invoices for this plan
        return await _db.InvoiceLineItems
            .Where(li => li.BudgetCategoryId != null
                && li.Invoice.PlanId == planId
                && (li.Invoice.Status == InvoiceStatus.Submitted || li.Invoice.Status == InvoiceStatus.UnderReview))
            .GroupBy(li => li.BudgetCategoryId!.Value)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Sum(li => li.Amount),
                ct);
    }
}
