namespace Octocare.Domain.Entities;

public static class ServiceAgreementStatus
{
    public const string Draft = "draft";
    public const string Sent = "sent";
    public const string Active = "active";
    public const string Expired = "expired";
    public const string Terminated = "terminated";

    public static readonly string[] ValidStatuses = [Draft, Sent, Active, Expired, Terminated];
}

public class ServiceAgreement
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid PlanId { get; private set; }
    public string Status { get; private set; } = ServiceAgreementStatus.Draft;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string? SignedDocumentUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Participant Participant { get; private set; } = null!;
    public Provider Provider { get; private set; } = null!;
    public Plan Plan { get; private set; } = null!;
    public ICollection<ServiceAgreementItem> Items { get; private set; } = new List<ServiceAgreementItem>();
    public ICollection<ServiceBooking> Bookings { get; private set; } = new List<ServiceBooking>();

    private ServiceAgreement() { }

    public static ServiceAgreement Create(Guid tenantId, Guid participantId, Guid providerId,
        Guid planId, DateOnly startDate, DateOnly endDate)
    {
        return new ServiceAgreement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ParticipantId = participantId,
            ProviderId = providerId,
            PlanId = planId,
            StartDate = startDate,
            EndDate = endDate,
            Status = ServiceAgreementStatus.Draft,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Send()
    {
        if (Status != ServiceAgreementStatus.Draft)
            throw new InvalidOperationException($"Cannot send an agreement with status '{Status}'. Agreement must be in Draft status.");

        Status = ServiceAgreementStatus.Sent;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (Status != ServiceAgreementStatus.Draft && Status != ServiceAgreementStatus.Sent)
            throw new InvalidOperationException($"Cannot activate an agreement with status '{Status}'. Agreement must be in Draft or Sent status.");

        Status = ServiceAgreementStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Expire()
    {
        if (Status != ServiceAgreementStatus.Active)
            throw new InvalidOperationException($"Cannot expire an agreement with status '{Status}'. Agreement must be Active.");

        Status = ServiceAgreementStatus.Expired;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Terminate()
    {
        if (Status != ServiceAgreementStatus.Active)
            throw new InvalidOperationException($"Cannot terminate an agreement with status '{Status}'. Agreement must be Active.");

        Status = ServiceAgreementStatus.Terminated;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(DateOnly startDate, DateOnly endDate, string? signedDocumentUrl)
    {
        if (Status != ServiceAgreementStatus.Draft)
            throw new InvalidOperationException($"Cannot update an agreement with status '{Status}'. Agreement must be in Draft status.");

        StartDate = startDate;
        EndDate = endDate;
        SignedDocumentUrl = signedDocumentUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
