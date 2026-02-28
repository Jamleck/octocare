namespace Octocare.Application.DTOs;

public record CreateServiceAgreementRequest(
    Guid ProviderId,
    Guid PlanId,
    DateOnly StartDate,
    DateOnly EndDate,
    List<CreateServiceAgreementItemRequest> Items);

public record CreateServiceAgreementItemRequest(
    string SupportItemNumber,
    decimal AgreedRate,
    string? Frequency);

public record CreateServiceBookingRequest(
    Guid BudgetCategoryId,
    decimal AllocatedAmount);
