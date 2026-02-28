using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Claim> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? status = null,
        CancellationToken ct = default);
    Task<Claim> AddAsync(Claim claim, CancellationToken ct = default);
    Task UpdateAsync(Claim claim, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
