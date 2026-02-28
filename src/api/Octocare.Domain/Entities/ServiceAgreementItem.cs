namespace Octocare.Domain.Entities;

public class ServiceAgreementItem
{
    public Guid Id { get; private set; }
    public Guid ServiceAgreementId { get; private set; }
    public string SupportItemNumber { get; private set; } = string.Empty;
    public long AgreedRate { get; private set; } // stored as cents
    public string? Frequency { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public ServiceAgreement Agreement { get; private set; } = null!;

    private ServiceAgreementItem() { }

    public static ServiceAgreementItem Create(Guid serviceAgreementId, string supportItemNumber,
        long agreedRateCents, string? frequency = null)
    {
        return new ServiceAgreementItem
        {
            Id = Guid.NewGuid(),
            ServiceAgreementId = serviceAgreementId,
            SupportItemNumber = supportItemNumber,
            AgreedRate = agreedRateCents,
            Frequency = frequency,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
