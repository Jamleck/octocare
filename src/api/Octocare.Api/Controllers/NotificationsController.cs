using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(NotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet("api/notifications")]
    public async Task<ActionResult<NotificationPagedResult>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? unreadOnly = null,
        [FromQuery] string? type = null,
        CancellationToken ct = default)
    {
        var user = await _currentUserService.GetUserAsync(ct);
        if (user is null) return Unauthorized();

        var result = await _notificationService.GetForUserAsync(user.Id, page, pageSize, unreadOnly, type, ct);
        return Ok(result);
    }

    [HttpGet("api/notifications/unread-count")]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount(CancellationToken ct = default)
    {
        var user = await _currentUserService.GetUserAsync(ct);
        if (user is null) return Unauthorized();

        var result = await _notificationService.GetUnreadCountAsync(user.Id, ct);
        return Ok(result);
    }

    [HttpPut("api/notifications/{id:guid}/read")]
    public async Task<ActionResult<NotificationDto>> MarkRead(Guid id, CancellationToken ct = default)
    {
        var result = await _notificationService.MarkReadAsync(id, ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("api/notifications/mark-all-read")]
    public async Task<ActionResult> MarkAllRead(CancellationToken ct = default)
    {
        var user = await _currentUserService.GetUserAsync(ct);
        if (user is null) return Unauthorized();

        await _notificationService.MarkAllReadAsync(user.Id, ct);
        return NoContent();
    }
}
