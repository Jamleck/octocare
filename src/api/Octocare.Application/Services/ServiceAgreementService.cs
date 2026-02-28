using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class ServiceAgreementService
{
    private readonly IServiceAgreementRepository _agreementRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IEventStore _eventStore;

    public ServiceAgreementService(
        IServiceAgreementRepository agreementRepo,
        IParticipantRepository participantRepo,
        ITenantContext tenantContext,
        IEventStore eventStore)
    {
        _agreementRepo = agreementRepo;
        _participantRepo = participantRepo;
        _tenantContext = tenantContext;
        _eventStore = eventStore;
    }

    public async Task<ServiceAgreementDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var agreement = await _agreementRepo.GetByIdAsync(id, ct);
        return agreement is null ? null : MapToDto(agreement);
    }

    public async Task<IReadOnlyList<ServiceAgreementDto>> GetByParticipantIdAsync(Guid participantId, CancellationToken ct)
    {
        var agreements = await _agreementRepo.GetByParticipantIdAsync(participantId, ct);
        return agreements.Select(MapToDto).ToList();
    }

    public async Task<ServiceAgreementDto> CreateAsync(Guid participantId, CreateServiceAgreementRequest request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var participant = await _participantRepo.GetByIdAsync(participantId, ct)
            ?? throw new KeyNotFoundException("Participant not found.");

        var agreement = ServiceAgreement.Create(tenantId, participantId, request.ProviderId,
            request.PlanId, request.StartDate, request.EndDate);

        await _agreementRepo.AddAsync(agreement, ct);

        // Add items
        foreach (var itemRequest in request.Items)
        {
            var rateCents = Money.FromDollars(itemRequest.AgreedRate).Cents;
            var item = ServiceAgreementItem.Create(agreement.Id, itemRequest.SupportItemNumber,
                rateCents, itemRequest.Frequency);
            await _agreementRepo.AddItemAsync(item, ct);
        }

        await _eventStore.AppendAsync(
            agreement.Id,
            "ServiceAgreement",
            "ServiceAgreementCreated",
            new { agreement.ParticipantId, agreement.ProviderId, agreement.PlanId, agreement.StartDate, agreement.EndDate, ItemCount = request.Items.Count },
            0,
            null,
            ct);

        // Re-fetch with includes to populate navigation properties
        var saved = await _agreementRepo.GetByIdAsync(agreement.Id, ct);
        return MapToDto(saved!);
    }

    public async Task<ServiceAgreementDto> ActivateAsync(Guid id, CancellationToken ct)
    {
        var agreement = await _agreementRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Service agreement not found.");

        agreement.Activate();
        await _agreementRepo.UpdateAsync(agreement, ct);

        var events = await _eventStore.GetStreamAsync(agreement.Id, ct);
        await _eventStore.AppendAsync(
            agreement.Id,
            "ServiceAgreement",
            "ServiceAgreementActivated",
            new { agreement.Status },
            events.Count,
            null,
            ct);

        return MapToDto(agreement);
    }

    public async Task<ServiceBookingDto> AddBookingAsync(Guid agreementId, CreateServiceBookingRequest request, CancellationToken ct)
    {
        var agreement = await _agreementRepo.GetByIdAsync(agreementId, ct)
            ?? throw new KeyNotFoundException("Service agreement not found.");

        var amountCents = Money.FromDollars(request.AllocatedAmount).Cents;
        var booking = ServiceBooking.Create(agreementId, request.BudgetCategoryId, amountCents);

        await _agreementRepo.AddBookingAsync(booking, ct);

        var events = await _eventStore.GetStreamAsync(agreement.Id, ct);
        await _eventStore.AppendAsync(
            agreement.Id,
            "ServiceAgreement",
            "ServiceBookingCreated",
            new { booking.BudgetCategoryId, AllocatedAmountCents = amountCents },
            events.Count,
            null,
            ct);

        // Re-fetch the booking to get BudgetCategory navigation
        var refreshed = await _agreementRepo.GetByIdAsync(agreementId, ct);
        var savedBooking = refreshed!.Bookings.First(b => b.Id == booking.Id);
        return MapBookingToDto(savedBooking);
    }

    public async Task<ServiceBookingDto> CancelBookingAsync(Guid agreementId, Guid bookingId, CancellationToken ct)
    {
        var agreement = await _agreementRepo.GetByIdAsync(agreementId, ct)
            ?? throw new KeyNotFoundException("Service agreement not found.");

        var booking = agreement.Bookings.FirstOrDefault(b => b.Id == bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        booking.Cancel();
        await _agreementRepo.SaveAsync(ct);

        var events = await _eventStore.GetStreamAsync(agreement.Id, ct);
        await _eventStore.AppendAsync(
            agreement.Id,
            "ServiceAgreement",
            "ServiceBookingCancelled",
            new { bookingId, booking.Status },
            events.Count,
            null,
            ct);

        return MapBookingToDto(booking);
    }

    private static ServiceAgreementDto MapToDto(ServiceAgreement agreement)
    {
        return new ServiceAgreementDto(
            agreement.Id,
            agreement.ParticipantId,
            agreement.Participant?.FullName ?? string.Empty,
            agreement.ProviderId,
            agreement.Provider?.Name ?? string.Empty,
            agreement.PlanId,
            agreement.Plan?.PlanNumber ?? string.Empty,
            agreement.Status,
            agreement.StartDate,
            agreement.EndDate,
            agreement.SignedDocumentUrl,
            agreement.Items.Select(MapItemToDto).ToList(),
            agreement.Bookings.Select(MapBookingToDto).ToList(),
            agreement.CreatedAt);
    }

    private static ServiceAgreementItemDto MapItemToDto(ServiceAgreementItem item)
    {
        return new ServiceAgreementItemDto(
            item.Id,
            item.SupportItemNumber,
            new Money(item.AgreedRate).ToDollars(),
            item.Frequency);
    }

    private static ServiceBookingDto MapBookingToDto(ServiceBooking booking)
    {
        return new ServiceBookingDto(
            booking.Id,
            booking.BudgetCategoryId,
            booking.BudgetCategory?.SupportCategory.ToString() ?? string.Empty,
            new Money(booking.AllocatedAmount).ToDollars(),
            new Money(booking.UsedAmount).ToDollars(),
            booking.Status);
    }
}
