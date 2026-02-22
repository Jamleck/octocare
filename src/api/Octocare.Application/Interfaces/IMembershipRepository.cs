using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IMembershipRepository
{
    Task<UserOrgMembership?> GetAsync(Guid userId, Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<UserOrgMembership>> GetByOrganisationAsync(Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<UserOrgMembership>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<UserOrgMembership> AddAsync(UserOrgMembership membership, CancellationToken ct = default);
    Task UpdateAsync(UserOrgMembership membership, CancellationToken ct = default);
}
