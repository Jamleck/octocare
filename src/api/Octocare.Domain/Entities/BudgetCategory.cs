using Octocare.Domain.Enums;

namespace Octocare.Domain.Entities;

public class BudgetCategory
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public SupportCategory SupportCategory { get; private set; }
    public SupportPurpose SupportPurpose { get; private set; }
    public long AllocatedAmount { get; private set; } // stored as cents
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Plan Plan { get; private set; } = null!;

    private BudgetCategory() { }

    public static BudgetCategory Create(Guid planId, SupportCategory supportCategory,
        SupportPurpose supportPurpose, long allocatedAmountCents)
    {
        return new BudgetCategory
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            SupportCategory = supportCategory,
            SupportPurpose = supportPurpose,
            AllocatedAmount = allocatedAmountCents,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateAllocation(long newAmountCents)
    {
        AllocatedAmount = newAmountCents;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
