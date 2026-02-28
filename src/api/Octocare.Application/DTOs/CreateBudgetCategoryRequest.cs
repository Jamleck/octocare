using Octocare.Domain.Enums;

namespace Octocare.Application.DTOs;

public record CreateBudgetCategoryRequest(
    SupportCategory SupportCategory,
    SupportPurpose SupportPurpose,
    decimal AllocatedAmount);  // dollars input, will be converted to cents
