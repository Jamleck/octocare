using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IServiceAgreementRepository
{
    Task<ServiceAgreement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceAgreement>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceAgreement>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task<ServiceAgreement> AddAsync(ServiceAgreement agreement, CancellationToken ct = default);
    Task UpdateAsync(ServiceAgreement agreement, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
    Task AddItemAsync(ServiceAgreementItem item, CancellationToken ct = default);
    Task AddBookingAsync(ServiceBooking booking, CancellationToken ct = default);
}
