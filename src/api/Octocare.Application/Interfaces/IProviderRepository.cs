using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Provider> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<bool> ExistsByAbnAsync(string abn, Guid? excludeId = null, CancellationToken ct = default);
    Task<Provider> AddAsync(Provider provider, CancellationToken ct = default);
    Task UpdateAsync(Provider provider, CancellationToken ct = default);
}
