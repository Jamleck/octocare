namespace Octocare.Domain.Entities;

public class EmailTemplate
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private EmailTemplate() { }

    public static EmailTemplate Create(Guid tenantId, string name, string subject, string body)
    {
        return new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Subject = subject,
            Body = body,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string subject, string body)
    {
        Subject = subject;
        Body = body;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
