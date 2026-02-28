using Octocare.Domain.Enums;

namespace Octocare.Domain.Entities;

public static class PlanStatus
{
    public const string Draft = "draft";
    public const string Active = "active";
    public const string Expiring = "expiring";
    public const string Expired = "expired";
    public const string Transitioned = "transitioned";

    public static readonly string[] ValidStatuses = [Draft, Active, Expiring, Expired, Transitioned];
}

public class Plan
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public string PlanNumber { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string Status { get; private set; } = PlanStatus.Draft;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Participant Participant { get; private set; } = null!;
    public ICollection<BudgetCategory> BudgetCategories { get; private set; } = new List<BudgetCategory>();

    private Plan() { }

    public static Plan Create(Guid tenantId, Guid participantId, string planNumber,
        DateOnly startDate, DateOnly endDate)
    {
        return new Plan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ParticipantId = participantId,
            PlanNumber = planNumber,
            StartDate = startDate,
            EndDate = endDate,
            Status = PlanStatus.Draft,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Activate()
    {
        if (Status != PlanStatus.Draft)
            throw new InvalidOperationException($"Cannot activate a plan with status '{Status}'. Plan must be in Draft status.");

        Status = PlanStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkExpiring()
    {
        if (Status != PlanStatus.Active)
            throw new InvalidOperationException($"Cannot mark plan as expiring with status '{Status}'. Plan must be Active.");

        Status = PlanStatus.Expiring;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Expire()
    {
        if (Status != PlanStatus.Active && Status != PlanStatus.Expiring)
            throw new InvalidOperationException($"Cannot expire a plan with status '{Status}'. Plan must be Active or Expiring.");

        Status = PlanStatus.Expired;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Transition()
    {
        if (Status != PlanStatus.Active && Status != PlanStatus.Expiring)
            throw new InvalidOperationException($"Cannot transition a plan with status '{Status}'. Plan must be Active or Expiring.");

        Status = PlanStatus.Transitioned;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string planNumber, DateOnly startDate, DateOnly endDate)
    {
        if (Status != PlanStatus.Draft)
            throw new InvalidOperationException($"Cannot update a plan with status '{Status}'. Plan must be in Draft status.");

        PlanNumber = planNumber;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
