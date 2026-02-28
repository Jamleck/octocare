using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class ServiceAgreementRepository : IServiceAgreementRepository
{
    private readonly OctocareDbContext _db;

    public ServiceAgreementRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceAgreement?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.ServiceAgreements
            .Include(sa => sa.Participant)
            .Include(sa => sa.Provider)
            .Include(sa => sa.Plan)
            .Include(sa => sa.Items)
            .Include(sa => sa.Bookings)
                .ThenInclude(b => b.BudgetCategory)
            .FirstOrDefaultAsync(sa => sa.Id == id, ct);
    }

    public async Task<IReadOnlyList<ServiceAgreement>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.ServiceAgreements
            .Include(sa => sa.Participant)
            .Include(sa => sa.Provider)
            .Include(sa => sa.Plan)
            .Include(sa => sa.Items)
            .Include(sa => sa.Bookings)
                .ThenInclude(b => b.BudgetCategory)
            .Where(sa => sa.ParticipantId == participantId)
            .OrderByDescending(sa => sa.StartDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ServiceAgreement>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.ServiceAgreements
            .Include(sa => sa.Participant)
            .Include(sa => sa.Provider)
            .Include(sa => sa.Plan)
            .Include(sa => sa.Items)
            .Include(sa => sa.Bookings)
                .ThenInclude(b => b.BudgetCategory)
            .Where(sa => sa.PlanId == planId)
            .OrderByDescending(sa => sa.StartDate)
            .ToListAsync(ct);
    }

    public async Task<ServiceAgreement> AddAsync(ServiceAgreement agreement, CancellationToken ct = default)
    {
        _db.ServiceAgreements.Add(agreement);
        await _db.SaveChangesAsync(ct);
        return agreement;
    }

    public async Task UpdateAsync(ServiceAgreement agreement, CancellationToken ct = default)
    {
        _db.ServiceAgreements.Update(agreement);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddItemAsync(ServiceAgreementItem item, CancellationToken ct = default)
    {
        _db.ServiceAgreementItems.Add(item);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddBookingAsync(ServiceBooking booking, CancellationToken ct = default)
    {
        _db.ServiceBookings.Add(booking);
        await _db.SaveChangesAsync(ct);
    }
}
