using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IParticipantRepository
{
    Task<Participant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Participant> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<bool> ExistsByNdisNumberAsync(string ndisNumber, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Participant>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Participant> AddAsync(Participant participant, CancellationToken ct = default);
    Task UpdateAsync(Participant participant, CancellationToken ct = default);
}
