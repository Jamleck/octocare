using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Infrastructure.Data.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly OctocareDbContext _db;

    public NotificationRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId, int page, int pageSize, bool? unreadOnly = null,
        string? type = null, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var query = _db.Notifications.Where(n => n.UserId == userId);

        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, out var notificationType))
            query = query.Where(n => n.Type == notificationType);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task<Notification> AddAsync(Notification notification, CancellationToken ct = default)
    {
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
        return notification;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        await _db.SetTenantAsync(ct);
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in unread)
        {
            notification.MarkRead();
        }

        await _db.SaveChangesAsync(ct);
    }
}
