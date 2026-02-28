using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly OctocareDbContext _db;

    public PaymentRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<PaymentBatch?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.PaymentBatches
            .Include(b => b.Items)
                .ThenInclude(i => i.Provider)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<(IReadOnlyList<PaymentBatch> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? status = null,
        CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);

        var query = _db.PaymentBatches
            .Include(b => b.Items)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(b => b.Status == status);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<PaymentBatch> AddAsync(PaymentBatch batch, CancellationToken ct = default)
    {
        _db.PaymentBatches.Add(batch);
        await _db.SaveChangesAsync(ct);
        return batch;
    }

    public async Task UpdateAsync(PaymentBatch batch, CancellationToken ct = default)
    {
        _db.PaymentBatches.Update(batch);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
