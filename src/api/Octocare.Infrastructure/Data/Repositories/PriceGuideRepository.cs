using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Infrastructure.Data.Repositories;

public class PriceGuideRepository : IPriceGuideRepository
{
    private readonly OctocareDbContext _db;

    public PriceGuideRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<PriceGuideVersion?> GetVersionByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.PriceGuideVersions.FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<PriceGuideVersion?> GetCurrentVersionAsync(CancellationToken ct = default)
    {
        return await _db.PriceGuideVersions.FirstOrDefaultAsync(v => v.IsCurrent, ct);
    }

    public async Task<IReadOnlyList<PriceGuideVersion>> GetAllVersionsAsync(CancellationToken ct = default)
    {
        return await _db.PriceGuideVersions
            .OrderByDescending(v => v.EffectiveFrom)
            .ToListAsync(ct);
    }

    public async Task<PriceGuideVersion> AddVersionAsync(PriceGuideVersion version, CancellationToken ct = default)
    {
        _db.PriceGuideVersions.Add(version);
        await _db.SaveChangesAsync(ct);
        return version;
    }

    public async Task<(IReadOnlyList<SupportItem> Items, int TotalCount)> GetItemsAsync(
        Guid versionId, int page, int pageSize, string? search = null,
        SupportCategory? category = null, CancellationToken ct = default)
    {
        var query = _db.SupportItems.Where(i => i.VersionId == versionId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(term) ||
                i.ItemNumber.ToLower().Contains(term));
        }

        if (category.HasValue)
        {
            query = query.Where(i => i.SupportCategory == category.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(i => i.ItemNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<SupportItem?> GetItemByNumberAsync(Guid versionId, string itemNumber, CancellationToken ct = default)
    {
        return await _db.SupportItems
            .FirstOrDefaultAsync(i => i.VersionId == versionId && i.ItemNumber == itemNumber, ct);
    }

    public async Task AddItemsAsync(IEnumerable<SupportItem> items, CancellationToken ct = default)
    {
        _db.SupportItems.AddRange(items);
        await _db.SaveChangesAsync(ct);
    }
}
