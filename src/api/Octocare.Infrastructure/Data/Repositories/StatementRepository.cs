using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class StatementRepository : IStatementRepository
{
    private readonly OctocareDbContext _db;

    public StatementRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<ParticipantStatement?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.ParticipantStatements
            .Include(s => s.Participant)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<ParticipantStatement>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.ParticipantStatements
            .Include(s => s.Plan)
            .Where(s => s.ParticipantId == participantId)
            .OrderByDescending(s => s.GeneratedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ParticipantStatement statement, CancellationToken ct = default)
    {
        _db.ParticipantStatements.Add(statement);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
