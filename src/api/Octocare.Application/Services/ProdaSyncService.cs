using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class ProdaSyncService
{
    private readonly IProdaPaceClient _prodaClient;
    private readonly IParticipantRepository _participantRepo;
    private readonly IPlanRepository _planRepo;

    public ProdaSyncService(IProdaPaceClient prodaClient,
        IParticipantRepository participantRepo,
        IPlanRepository planRepo)
    {
        _prodaClient = prodaClient;
        _participantRepo = participantRepo;
        _planRepo = planRepo;
    }

    /// <summary>
    /// Fetches plan info from PRODA for the given participant, compares with local plan data,
    /// and returns a sync result with any discrepancies found.
    /// </summary>
    public async Task<SyncResult> SyncParticipantPlanAsync(Guid participantId, CancellationToken ct = default)
    {
        var participant = await _participantRepo.GetByIdAsync(participantId, ct)
            ?? throw new KeyNotFoundException("Participant not found.");

        var prodaPlan = await _prodaClient.GetPlanInfoAsync(participant.NdisNumber, ct);
        if (prodaPlan is null)
        {
            return new SyncResult(false, new List<SyncDiscrepancy>
            {
                new("ProdaLookup", "N/A", "Not found", "warning")
            });
        }

        var localPlans = await _planRepo.GetByParticipantIdAsync(participantId, ct);
        var activePlan = localPlans.FirstOrDefault(p => p.Status == "active")
                      ?? localPlans.FirstOrDefault();

        if (activePlan is null)
        {
            return new SyncResult(false, new List<SyncDiscrepancy>
            {
                new("LocalPlan", "No local plan", prodaPlan.PlanNumber, "error")
            });
        }

        var discrepancies = new List<SyncDiscrepancy>();

        if (activePlan.PlanNumber != prodaPlan.PlanNumber)
        {
            discrepancies.Add(new SyncDiscrepancy("PlanNumber",
                activePlan.PlanNumber, prodaPlan.PlanNumber, "error"));
        }

        if (activePlan.Status != prodaPlan.Status)
        {
            discrepancies.Add(new SyncDiscrepancy("Status",
                activePlan.Status, prodaPlan.Status, "warning"));
        }

        if (activePlan.StartDate != prodaPlan.StartDate)
        {
            discrepancies.Add(new SyncDiscrepancy("StartDate",
                activePlan.StartDate.ToString("yyyy-MM-dd"),
                prodaPlan.StartDate.ToString("yyyy-MM-dd"),
                "warning"));
        }

        if (activePlan.EndDate != prodaPlan.EndDate)
        {
            discrepancies.Add(new SyncDiscrepancy("EndDate",
                activePlan.EndDate.ToString("yyyy-MM-dd"),
                prodaPlan.EndDate.ToString("yyyy-MM-dd"),
                "warning"));
        }

        var localTotalBudget = activePlan.BudgetCategories
            .Sum(bc => new Money(bc.AllocatedAmount).ToDollars());

        if (localTotalBudget != prodaPlan.TotalBudget)
        {
            discrepancies.Add(new SyncDiscrepancy("TotalBudget",
                localTotalBudget.ToString("F2"),
                prodaPlan.TotalBudget.ToString("F2"),
                "error"));
        }

        return new SyncResult(discrepancies.Count == 0, discrepancies);
    }

    /// <summary>
    /// Compares local budget categories with PRODA budget data for a given plan.
    /// </summary>
    public async Task<SyncResult> VerifyBudgetAsync(Guid planId, CancellationToken ct = default)
    {
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(planId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        var participant = await _participantRepo.GetByIdAsync(plan.ParticipantId, ct)
            ?? throw new KeyNotFoundException("Participant not found.");

        var prodaBudget = await _prodaClient.GetBudgetInfoAsync(
            participant.NdisNumber, plan.PlanNumber, ct);

        if (prodaBudget is null)
        {
            return new SyncResult(false, new List<SyncDiscrepancy>
            {
                new("ProdaBudgetLookup", "N/A", "Not found", "warning")
            });
        }

        var discrepancies = new List<SyncDiscrepancy>();

        foreach (var prodaLine in prodaBudget.Categories)
        {
            var localCategory = plan.BudgetCategories
                .FirstOrDefault(bc =>
                    bc.SupportCategory.ToString() == prodaLine.Category &&
                    bc.SupportPurpose.ToString() == prodaLine.Purpose);

            if (localCategory is null)
            {
                discrepancies.Add(new SyncDiscrepancy(
                    $"Budget:{prodaLine.Category}/{prodaLine.Purpose}",
                    "Missing locally",
                    $"${prodaLine.Allocated:F2}",
                    "error"));
                continue;
            }

            var localAllocated = new Money(localCategory.AllocatedAmount).ToDollars();
            if (localAllocated != prodaLine.Allocated)
            {
                discrepancies.Add(new SyncDiscrepancy(
                    $"Budget:{prodaLine.Category}/{prodaLine.Purpose}:Allocated",
                    $"${localAllocated:F2}",
                    $"${prodaLine.Allocated:F2}",
                    "warning"));
            }
        }

        // Check for local categories not in PRODA
        foreach (var localCat in plan.BudgetCategories)
        {
            var existsInProda = prodaBudget.Categories.Any(p =>
                p.Category == localCat.SupportCategory.ToString() &&
                p.Purpose == localCat.SupportPurpose.ToString());

            if (!existsInProda)
            {
                var localAllocated = new Money(localCat.AllocatedAmount).ToDollars();
                discrepancies.Add(new SyncDiscrepancy(
                    $"Budget:{localCat.SupportCategory}/{localCat.SupportPurpose}",
                    $"${localAllocated:F2}",
                    "Not in PRODA",
                    "warning"));
            }
        }

        return new SyncResult(discrepancies.Count == 0, discrepancies);
    }
}
