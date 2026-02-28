namespace Octocare.Domain.Entities;

public class ParticipantStatement
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public string? PdfUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Participant Participant { get; private set; } = null!;
    public Plan Plan { get; private set; } = null!;

    private ParticipantStatement() { }

    public static ParticipantStatement Create(Guid tenantId, Guid participantId, Guid planId,
        DateOnly periodStart, DateOnly periodEnd)
    {
        return new ParticipantStatement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ParticipantId = participantId,
            PlanId = planId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GeneratedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkSent()
    {
        SentAt = DateTimeOffset.UtcNow;
    }

    public void SetPdfUrl(string url)
    {
        PdfUrl = url;
    }
}
