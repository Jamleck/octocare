using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface ICommunicationLogRepository
{
    Task<(IReadOnlyList<CommunicationLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? recipientEmail = null,
        string? templateName = null, CancellationToken ct = default);
    Task<CommunicationLog> AddAsync(CommunicationLog log, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
