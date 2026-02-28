using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class BudgetProjectionService
{
    private readonly IPlanRepository _planRepo;
    private readonly IBudgetProjectionRepository _projectionRepo;

    public BudgetProjectionService(
        IPlanRepository planRepo,
        IBudgetProjectionRepository projectionRepo)
    {
        _planRepo = planRepo;
        _projectionRepo = projectionRepo;
    }

    public async Task<BudgetOverviewDto?> GetProjectionsForPlanAsync(Guid planId, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(planId, ct);
        if (plan is null) return null;

        // Recalculate projections from actual data before returning
        await RecalculateProjectionsAsync(planId, ct);

        var projections = await _projectionRepo.GetByPlanIdAsync(planId, ct);

        var categoryDtos = new List<BudgetCategoryProjectionDto>();

        foreach (var bc in plan.BudgetCategories)
        {
            var projection = projections.FirstOrDefault(p => p.BudgetCategoryId == bc.Id);

            var allocated = projection?.AllocatedAmount ?? bc.AllocatedAmount;
            var committed = projection?.CommittedAmount ?? 0L;
            var spent = projection?.SpentAmount ?? 0L;
            var pending = projection?.PendingAmount ?? 0L;
            var available = allocated - committed - spent;

            var utilisation = allocated > 0
                ? Math.Round((decimal)(committed + spent) / allocated * 100, 1)
                : 0m;

            categoryDtos.Add(new BudgetCategoryProjectionDto(
                CategoryId: bc.Id,
                SupportCategory: bc.SupportCategory.ToString(),
                SupportPurpose: bc.SupportPurpose.ToString(),
                Allocated: new Money(allocated).ToDollars(),
                Committed: new Money(committed).ToDollars(),
                Spent: new Money(spent).ToDollars(),
                Pending: new Money(pending).ToDollars(),
                Available: new Money(available).ToDollars(),
                UtilisationPercentage: utilisation));
        }

        var totalAllocated = categoryDtos.Sum(c => c.Allocated);
        var totalCommitted = categoryDtos.Sum(c => c.Committed);
        var totalSpent = categoryDtos.Sum(c => c.Spent);
        var totalPending = categoryDtos.Sum(c => c.Pending);
        var totalAvailable = categoryDtos.Sum(c => c.Available);

        var totalUtilisation = totalAllocated > 0
            ? Math.Round((totalCommitted + totalSpent) / totalAllocated * 100, 1)
            : 0m;

        return new BudgetOverviewDto(
            PlanId: plan.Id,
            PlanNumber: plan.PlanNumber,
            TotalAllocated: totalAllocated,
            TotalCommitted: totalCommitted,
            TotalSpent: totalSpent,
            TotalPending: totalPending,
            TotalAvailable: totalAvailable,
            UtilisationPercentage: totalUtilisation,
            Categories: categoryDtos);
    }

    public async Task RecalculateProjectionsAsync(Guid planId, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(planId, ct);
        if (plan is null) return;

        // Get existing projections
        var existingProjections = await _projectionRepo.GetByPlanIdAsync(planId, ct);

        // Get committed amounts from active service bookings per category
        var committedByCategory = await _projectionRepo.GetCommittedAmountsByPlanAsync(planId, ct);

        // Get spent amounts from approved/paid invoices per category
        var spentByCategory = await _projectionRepo.GetSpentAmountsByPlanAsync(planId, ct);

        // Get pending amounts from submitted/under_review invoices per category
        var pendingByCategory = await _projectionRepo.GetPendingAmountsByPlanAsync(planId, ct);

        foreach (var bc in plan.BudgetCategories)
        {
            var committed = committedByCategory.GetValueOrDefault(bc.Id, 0L);
            var spent = spentByCategory.GetValueOrDefault(bc.Id, 0L);
            var pending = pendingByCategory.GetValueOrDefault(bc.Id, 0L);

            var existing = existingProjections.FirstOrDefault(p => p.BudgetCategoryId == bc.Id);

            if (existing is not null)
            {
                existing.UpdateFromEvent(bc.AllocatedAmount, committed, spent, pending);
            }
            else
            {
                var projection = BudgetProjection.Create(bc.Id, bc.AllocatedAmount);
                projection.UpdateFromEvent(bc.AllocatedAmount, committed, spent, pending);
                await _projectionRepo.AddAsync(projection, ct);
            }
        }

        await _projectionRepo.SaveAsync(ct);
    }
}
