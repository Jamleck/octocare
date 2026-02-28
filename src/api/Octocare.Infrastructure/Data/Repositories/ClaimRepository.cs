using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly OctocareDbContext _db;

    public ClaimRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Claims
            .Include(c => c.LineItems)
                .ThenInclude(li => li.InvoiceLineItem)
                    .ThenInclude(ili => ili.Invoice)
                        .ThenInclude(i => i.Provider)
            .Include(c => c.LineItems)
                .ThenInclude(li => li.InvoiceLineItem)
                    .ThenInclude(ili => ili.Invoice)
                        .ThenInclude(i => i.Participant)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Claim> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? status = null,
        CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);

        var query = _db.Claims
            .Include(c => c.LineItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status == status);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.BatchNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Claim> AddAsync(Claim claim, CancellationToken ct = default)
    {
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync(ct);
        return claim;
    }

    public async Task UpdateAsync(Claim claim, CancellationToken ct = default)
    {
        _db.Claims.Update(claim);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
