using Octocare.Domain.Enums;

namespace Octocare.Application.DTOs;

public record BudgetCategoryDto(
    Guid Id,
    SupportCategory SupportCategory,
    SupportPurpose SupportPurpose,
    decimal AllocatedAmount);  // dollars for display
