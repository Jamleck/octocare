using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Application.Services;

public class MemberService
{
    private readonly IUserRepository _userRepo;
    private readonly IMembershipRepository _memberRepo;
    private readonly ITenantContext _tenantContext;

    public MemberService(IUserRepository userRepo, IMembershipRepository memberRepo, ITenantContext tenantContext)
    {
        _userRepo = userRepo;
        _memberRepo = memberRepo;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var memberships = await _memberRepo.GetByOrganisationAsync(tenantId, ct);
        return memberships.Select(MapToDto).ToList();
    }

    public async Task<MemberDto> InviteMemberAsync(InviteMemberRequest request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        // Find or create user
        var user = await _userRepo.GetByEmailAsync(request.Email, ct);
        if (user is null)
        {
            user = User.Create($"pending|{Guid.NewGuid()}", request.Email, request.FirstName, request.LastName);
            await _userRepo.AddAsync(user, ct);
        }

        // Check for existing membership
        var existing = await _memberRepo.GetAsync(user.Id, tenantId, ct);
        if (existing is not null)
            throw new InvalidOperationException("User is already a member of this organisation.");

        var membership = UserOrgMembership.Create(user.Id, tenantId, request.Role);
        await _memberRepo.AddAsync(membership, ct);

        return MapToDto(membership, user);
    }

    public async Task<MemberDto> UpdateMemberRoleAsync(Guid userId, UpdateMemberRoleRequest request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var membership = await _memberRepo.GetAsync(userId, tenantId, ct)
            ?? throw new KeyNotFoundException("Member not found.");

        membership.UpdateRole(request.Role);
        await _memberRepo.UpdateAsync(membership, ct);

        return MapToDto(membership);
    }

    public async Task DeactivateMemberAsync(Guid userId, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var membership = await _memberRepo.GetAsync(userId, tenantId, ct)
            ?? throw new KeyNotFoundException("Member not found.");

        membership.Deactivate();
        await _memberRepo.UpdateAsync(membership, ct);
    }

    private static MemberDto MapToDto(UserOrgMembership membership)
    {
        return new MemberDto(
            membership.UserId, membership.User.Email,
            membership.User.FirstName, membership.User.LastName,
            membership.Role, membership.IsActive, membership.CreatedAt);
    }

    private static MemberDto MapToDto(UserOrgMembership membership, User user)
    {
        return new MemberDto(
            user.Id, user.Email,
            user.FirstName, user.LastName,
            membership.Role, membership.IsActive, membership.CreatedAt);
    }
}
