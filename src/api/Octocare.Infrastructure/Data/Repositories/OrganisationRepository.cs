using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class OrganisationRepository : IOrganisationRepository
{
    private readonly OctocareDbContext _db;

    public OrganisationRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<Organisation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Organisations.FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Organisation> AddAsync(Organisation organisation, CancellationToken ct = default)
    {
        _db.Organisations.Add(organisation);
        await _db.SaveChangesAsync(ct);
        return organisation;
    }

    public async Task UpdateAsync(Organisation organisation, CancellationToken ct = default)
    {
        _db.Organisations.Update(organisation);
        await _db.SaveChangesAsync(ct);
    }
}
