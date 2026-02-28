using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly OctocareDbContext _db;

    public InvoiceRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Invoices
            .Include(i => i.Provider)
            .Include(i => i.Participant)
            .Include(i => i.Plan)
            .Include(i => i.LineItems)
                .ThenInclude(li => li.BudgetCategory)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? status = null, Guid? participantId = null, Guid? providerId = null,
        CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);

        var query = _db.Invoices
            .Include(i => i.Provider)
            .Include(i => i.Participant)
            .Include(i => i.Plan)
            .Include(i => i.LineItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);

        if (participantId.HasValue)
            query = query.Where(i => i.ParticipantId == participantId.Value);

        if (providerId.HasValue)
            query = query.Where(i => i.ProviderId == providerId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(i => i.ServicePeriodEnd)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber, Guid? excludeId = null, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var query = _db.Invoices.Where(i => i.InvoiceNumber == invoiceNumber);
        if (excludeId.HasValue)
            query = query.Where(i => i.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<Invoice> AddAsync(Invoice invoice, CancellationToken ct = default)
    {
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);
        return invoice;
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        _db.Invoices.Update(invoice);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
