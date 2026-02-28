namespace Octocare.Application.DTOs;

public record ServiceAgreementDto(
    Guid Id,
    Guid ParticipantId,
    string ParticipantName,
    Guid ProviderId,
    string ProviderName,
    Guid PlanId,
    string PlanNumber,
    string Status,
    DateOnly StartDate,
    DateOnly EndDate,
    string? SignedDocumentUrl,
    IReadOnlyList<ServiceAgreementItemDto> Items,
    IReadOnlyList<ServiceBookingDto> Bookings,
    DateTimeOffset CreatedAt);
