using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public record LineItemValidationResult(
    int Index,
    string Status,  // "valid", "warning", "error"
    string? Message);

public class InvoiceValidationService
{
    private readonly IPlanRepository _planRepo;
    private readonly IPriceGuideRepository _priceGuideRepo;

    public InvoiceValidationService(IPlanRepository planRepo, IPriceGuideRepository priceGuideRepo)
    {
        _planRepo = planRepo;
        _priceGuideRepo = priceGuideRepo;
    }

    public async Task<List<LineItemValidationResult>> ValidateLineItemsAsync(
        Guid planId,
        IEnumerable<CreateInvoiceLineItemRequest> lineItems,
        CancellationToken ct)
    {
        var results = new List<LineItemValidationResult>();
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(planId, ct);

        // Get current price guide version
        var currentVersion = await _priceGuideRepo.GetCurrentVersionAsync(ct);

        var index = 0;
        foreach (var item in lineItems)
        {
            var rateCents = Money.FromDollars(item.Rate).Cents;

            // 1. Check support item exists in current price guide
            if (currentVersion is not null)
            {
                var supportItem = await _priceGuideRepo.GetItemByNumberAsync(
                    currentVersion.Id, item.SupportItemNumber, ct);

                if (supportItem is null)
                {
                    results.Add(new LineItemValidationResult(index, "warning",
                        $"Support item '{item.SupportItemNumber}' not found in current price guide."));
                    index++;
                    continue;
                }

                // 2. Check rate <= price limit (use national price limit)
                if (rateCents > supportItem.PriceLimitNational)
                {
                    var limitDollars = new Money(supportItem.PriceLimitNational).ToDollars();
                    results.Add(new LineItemValidationResult(index, "warning",
                        $"Rate ${item.Rate:N2} exceeds price limit ${limitDollars:N2} for {item.SupportItemNumber}."));
                    index++;
                    continue;
                }
            }

            // 3. Check service date within plan period
            if (plan is not null)
            {
                if (item.ServiceDate < plan.StartDate || item.ServiceDate > plan.EndDate)
                {
                    results.Add(new LineItemValidationResult(index, "warning",
                        $"Service date {item.ServiceDate} is outside the plan period ({plan.StartDate} - {plan.EndDate})."));
                    index++;
                    continue;
                }
            }

            // 4. Budget category funds check (warning only)
            if (item.BudgetCategoryId.HasValue && plan is not null)
            {
                var category = plan.BudgetCategories.FirstOrDefault(bc => bc.Id == item.BudgetCategoryId.Value);
                if (category is null)
                {
                    results.Add(new LineItemValidationResult(index, "warning",
                        "Specified budget category not found in plan."));
                    index++;
                    continue;
                }
            }

            results.Add(new LineItemValidationResult(index, "valid", null));
            index++;
        }

        return results;
    }
}
