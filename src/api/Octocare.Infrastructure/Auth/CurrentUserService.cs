using Microsoft.AspNetCore.Http;
using Octocare.Application.Interfaces;
using Octocare.Domain.Authorization;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepo;
    private readonly IMembershipRepository _memberRepo;
    private readonly ITenantContext _tenantContext;

    private User? _cachedUser;
    private string? _cachedRole;
    private bool _userLoaded;
    private bool _roleLoaded;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepo,
        IMembershipRepository memberRepo,
        ITenantContext tenantContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepo = userRepo;
        _memberRepo = memberRepo;
        _tenantContext = tenantContext;
    }

    public string? ExternalUserId =>
        _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

    public Guid? TenantId => _tenantContext.TenantId;

    public async Task<User?> GetUserAsync(CancellationToken ct = default)
    {
        if (_userLoaded) return _cachedUser;

        if (ExternalUserId is { } externalId)
            _cachedUser = await _userRepo.GetByExternalIdAsync(externalId, ct);

        _userLoaded = true;
        return _cachedUser;
    }

    public async Task<string?> GetRoleAsync(CancellationToken ct = default)
    {
        if (_roleLoaded) return _cachedRole;

        var user = await GetUserAsync(ct);
        if (user is not null && TenantId is { } tenantId)
        {
            var membership = await _memberRepo.GetAsync(user.Id, tenantId, ct);
            _cachedRole = membership?.IsActive == true ? membership.Role : null;
        }

        _roleLoaded = true;
        return _cachedRole;
    }

    public async Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default)
    {
        var role = await GetRoleAsync(ct);
        return role is not null && RolePermissions.HasPermission(role, permission);
    }
}
