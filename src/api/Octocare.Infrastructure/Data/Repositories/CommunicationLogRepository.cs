using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class CommunicationLogRepository : ICommunicationLogRepository
{
    private readonly OctocareDbContext _db;

    public CommunicationLogRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<CommunicationLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? recipientEmail = null,
        string? templateName = null, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var query = _db.CommunicationLogs.AsQueryable();

        if (!string.IsNullOrEmpty(recipientEmail))
            query = query.Where(c => c.RecipientEmail.Contains(recipientEmail));

        if (!string.IsNullOrEmpty(templateName))
            query = query.Where(c => c.TemplateName == templateName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<CommunicationLog> AddAsync(CommunicationLog log, CancellationToken ct = default)
    {
        _db.CommunicationLogs.Add(log);
        await _db.SaveChangesAsync(ct);
        return log;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
