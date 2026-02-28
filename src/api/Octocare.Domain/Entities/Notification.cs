using Octocare.Domain.Enums;

namespace Octocare.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public string? Link { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    private Notification() { }

    public static Notification Create(Guid tenantId, Guid userId, string title, string message,
        NotificationType type, string? link = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            Link = link,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkRead()
    {
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
    }
}
