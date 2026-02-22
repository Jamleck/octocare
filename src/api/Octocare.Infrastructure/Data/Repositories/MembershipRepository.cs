using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly OctocareDbContext _db;

    public MembershipRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<UserOrgMembership?> GetAsync(Guid userId, Guid organisationId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.UserOrgMemberships
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.OrganisationId == organisationId, ct);
    }

    public async Task<IReadOnlyList<UserOrgMembership>> GetByOrganisationAsync(Guid organisationId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.UserOrgMemberships
            .Include(m => m.User)
            .Where(m => m.OrganisationId == organisationId)
            .OrderBy(m => m.User.LastName)
            .ThenBy(m => m.User.FirstName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserOrgMembership>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        // Cross-tenant query — no tenant filter needed
        return await _db.UserOrgMemberships
            .IgnoreQueryFilters()
            .Include(m => m.Organisation)
            .Where(m => m.UserId == userId && m.IsActive)
            .ToListAsync(ct);
    }

    public async Task<UserOrgMembership> AddAsync(UserOrgMembership membership, CancellationToken ct = default)
    {
        _db.UserOrgMemberships.Add(membership);
        await _db.SaveChangesAsync(ct);
        return membership;
    }

    public async Task UpdateAsync(UserOrgMembership membership, CancellationToken ct = default)
    {
        _db.UserOrgMemberships.Update(membership);
        await _db.SaveChangesAsync(ct);
    }
}
