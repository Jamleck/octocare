using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly OctocareDbContext _db;

    public ParticipantRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<Participant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Participants.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Participant> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var query = _db.Participants.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.NdisNumber.Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<bool> ExistsByNdisNumberAsync(string ndisNumber, Guid? excludeId = null, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var query = _db.Participants.Where(p => p.NdisNumber == ndisNumber);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<Participant> AddAsync(Participant participant, CancellationToken ct = default)
    {
        _db.Participants.Add(participant);
        await _db.SaveChangesAsync(ct);
        return participant;
    }

    public async Task UpdateAsync(Participant participant, CancellationToken ct = default)
    {
        _db.Participants.Update(participant);
        await _db.SaveChangesAsync(ct);
    }
}
