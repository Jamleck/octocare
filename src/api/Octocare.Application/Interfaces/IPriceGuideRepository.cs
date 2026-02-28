using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Application.Interfaces;

public interface IPriceGuideRepository
{
    Task<PriceGuideVersion?> GetVersionByIdAsync(Guid id, CancellationToken ct = default);
    Task<PriceGuideVersion?> GetCurrentVersionAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PriceGuideVersion>> GetAllVersionsAsync(CancellationToken ct = default);
    Task<PriceGuideVersion> AddVersionAsync(PriceGuideVersion version, CancellationToken ct = default);
    Task<(IReadOnlyList<SupportItem> Items, int TotalCount)> GetItemsAsync(
        Guid versionId, int page, int pageSize, string? search = null,
        SupportCategory? category = null, CancellationToken ct = default);
    Task<SupportItem?> GetItemByNumberAsync(Guid versionId, string itemNumber, CancellationToken ct = default);
    Task AddItemsAsync(IEnumerable<SupportItem> items, CancellationToken ct = default);
}
