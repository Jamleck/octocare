using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IOrganisationRepository
{
    Task<Organisation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Organisation> AddAsync(Organisation organisation, CancellationToken ct = default);
    Task UpdateAsync(Organisation organisation, CancellationToken ct = default);
}
