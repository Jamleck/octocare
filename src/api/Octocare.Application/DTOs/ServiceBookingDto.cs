namespace Octocare.Application.DTOs;

public record ServiceBookingDto(
    Guid Id,
    Guid BudgetCategoryId,
    string SupportCategory,
    decimal AllocatedAmount,
    decimal UsedAmount,
    string Status);
