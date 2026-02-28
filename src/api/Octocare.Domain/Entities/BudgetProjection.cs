namespace Octocare.Domain.Entities;

public class BudgetProjection
{
    public Guid Id { get; private set; }
    public Guid BudgetCategoryId { get; private set; }
    public long AllocatedAmount { get; private set; } // cents
    public long CommittedAmount { get; private set; } // cents — from active service bookings
    public long SpentAmount { get; private set; } // cents — from paid/approved invoices
    public long PendingAmount { get; private set; } // cents — from submitted/under_review invoices
    public long AvailableAmount => AllocatedAmount - CommittedAmount - SpentAmount;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public BudgetCategory BudgetCategory { get; private set; } = null!;

    private BudgetProjection() { }

    public static BudgetProjection Create(Guid budgetCategoryId, long allocatedAmountCents)
    {
        return new BudgetProjection
        {
            Id = Guid.NewGuid(),
            BudgetCategoryId = budgetCategoryId,
            AllocatedAmount = allocatedAmountCents,
            CommittedAmount = 0,
            SpentAmount = 0,
            PendingAmount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateFromEvent(long allocatedCents, long committedCents, long spentCents, long pendingCents)
    {
        AllocatedAmount = allocatedCents;
        CommittedAmount = committedCents;
        SpentAmount = spentCents;
        PendingAmount = pendingCents;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
