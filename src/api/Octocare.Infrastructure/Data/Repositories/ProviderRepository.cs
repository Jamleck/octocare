using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class ProviderRepository : IProviderRepository
{
    private readonly OctocareDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ProviderRepository(OctocareDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Provider?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Providers are shared — no tenant filter needed for single lookup
        return await _db.Providers.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Provider> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);

        // Get provider IDs linked to the current tenant via TenantProviderRelationship
        var tenantProviderIds = _db.TenantProviderRelationships
            .Where(r => r.Status == "active")
            .Select(r => r.ProviderId);

        var query = _db.Providers
            .Where(p => p.IsActive && tenantProviderIds.Contains(p.Id));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Abn != null && p.Abn.Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<bool> ExistsByAbnAsync(string abn, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Providers.Where(p => p.Abn == abn);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<Provider> AddAsync(Provider provider, CancellationToken ct = default)
    {
        _db.Providers.Add(provider);

        // Also create TenantProviderRelationship for current tenant
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var relationship = TenantProviderRelationship.Create(tenantId, provider.Id);
        _db.TenantProviderRelationships.Add(relationship);

        await _db.SaveChangesAsync(ct);
        return provider;
    }

    public async Task UpdateAsync(Provider provider, CancellationToken ct = default)
    {
        _db.Providers.Update(provider);
        await _db.SaveChangesAsync(ct);
    }
}
