using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface ICurrentUserService
{
    string? ExternalUserId { get; }
    Guid? TenantId { get; }
    Task<User?> GetUserAsync(CancellationToken ct = default);
    Task<string?> GetRoleAsync(CancellationToken ct = default);
    Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default);
}
