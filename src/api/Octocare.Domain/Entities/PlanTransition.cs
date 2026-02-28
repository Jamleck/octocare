using Octocare.Domain.Enums;

namespace Octocare.Domain.Entities;

public class PlanTransition
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OldPlanId { get; private set; }
    public Guid? NewPlanId { get; private set; }
    public PlanTransitionStatus Status { get; private set; }
    public string ChecklistItems { get; private set; } = "[]"; // JSON array
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public Plan OldPlan { get; private set; } = null!;
    public Plan? NewPlan { get; private set; }

    private PlanTransition() { }

    public static PlanTransition Create(Guid tenantId, Guid oldPlanId, string checklistItemsJson)
    {
        return new PlanTransition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OldPlanId = oldPlanId,
            Status = PlanTransitionStatus.Pending,
            ChecklistItems = checklistItemsJson,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateChecklist(string checklistItemsJson)
    {
        ChecklistItems = checklistItemsJson;
        if (Status == PlanTransitionStatus.Pending)
            Status = PlanTransitionStatus.InProgress;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }

    public void SetNewPlan(Guid newPlanId)
    {
        NewPlanId = newPlanId;
    }

    public void Complete()
    {
        if (Status == PlanTransitionStatus.Completed)
            throw new InvalidOperationException("Transition is already completed.");

        Status = PlanTransitionStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
