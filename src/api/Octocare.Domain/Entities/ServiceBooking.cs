namespace Octocare.Domain.Entities;

public static class ServiceBookingStatus
{
    public const string Active = "active";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";

    public static readonly string[] ValidStatuses = [Active, Completed, Cancelled];
}

public class ServiceBooking
{
    public Guid Id { get; private set; }
    public Guid ServiceAgreementId { get; private set; }
    public Guid BudgetCategoryId { get; private set; }
    public long AllocatedAmount { get; private set; } // stored as cents
    public long UsedAmount { get; private set; } // stored as cents
    public string Status { get; private set; } = ServiceBookingStatus.Active;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ServiceAgreement Agreement { get; private set; } = null!;
    public BudgetCategory BudgetCategory { get; private set; } = null!;

    private ServiceBooking() { }

    public static ServiceBooking Create(Guid serviceAgreementId, Guid budgetCategoryId,
        long allocatedAmountCents)
    {
        return new ServiceBooking
        {
            Id = Guid.NewGuid(),
            ServiceAgreementId = serviceAgreementId,
            BudgetCategoryId = budgetCategoryId,
            AllocatedAmount = allocatedAmountCents,
            UsedAmount = 0,
            Status = ServiceBookingStatus.Active,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Cancel()
    {
        if (Status != ServiceBookingStatus.Active)
            throw new InvalidOperationException($"Cannot cancel a booking with status '{Status}'. Booking must be Active.");

        Status = ServiceBookingStatus.Cancelled;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordUsage(long amountCents)
    {
        if (Status != ServiceBookingStatus.Active)
            throw new InvalidOperationException($"Cannot record usage on a booking with status '{Status}'. Booking must be Active.");

        if (amountCents <= 0)
            throw new ArgumentException("Usage amount must be greater than zero.", nameof(amountCents));

        if (UsedAmount + amountCents > AllocatedAmount)
            throw new InvalidOperationException("Usage would exceed the allocated amount for this booking.");

        UsedAmount += amountCents;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        if (Status != ServiceBookingStatus.Active)
            throw new InvalidOperationException($"Cannot complete a booking with status '{Status}'. Booking must be Active.");

        Status = ServiceBookingStatus.Completed;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
