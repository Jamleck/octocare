namespace Octocare.Domain.Entities;

public class CommunicationLog
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? TemplateName { get; private set; }
    public DateTimeOffset SentAt { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public string? RelatedEntityId { get; private set; }

    private CommunicationLog() { }

    public static CommunicationLog Create(Guid tenantId, string recipientEmail, string subject,
        string body, string status, string? templateName = null,
        string? relatedEntityType = null, string? relatedEntityId = null,
        string? errorMessage = null)
    {
        return new CommunicationLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecipientEmail = recipientEmail,
            Subject = subject,
            Body = body,
            TemplateName = templateName,
            SentAt = DateTimeOffset.UtcNow,
            Status = status,
            ErrorMessage = errorMessage,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        };
    }
}
