using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly OctocareDbContext _db;

    public EmailTemplateRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.EmailTemplates.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.EmailTemplates
            .FirstOrDefaultAsync(e => e.Name == name && e.IsActive, ct);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.EmailTemplates
            .OrderBy(e => e.Name)
            .ToListAsync(ct);
    }

    public async Task<EmailTemplate> AddAsync(EmailTemplate template, CancellationToken ct = default)
    {
        _db.EmailTemplates.Add(template);
        await _db.SaveChangesAsync(ct);
        return template;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.EmailTemplates
            .AnyAsync(e => e.Name == name && (!excludeId.HasValue || e.Id != excludeId.Value), ct);
    }
}
