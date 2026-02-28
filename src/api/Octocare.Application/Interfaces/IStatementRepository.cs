using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IStatementRepository
{
    Task<ParticipantStatement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ParticipantStatement>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default);
    Task AddAsync(ParticipantStatement statement, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
