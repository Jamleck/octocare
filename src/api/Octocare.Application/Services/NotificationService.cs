using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Application.Services;

public class NotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly ITenantContext _tenantContext;
    private readonly EmailTemplateService _emailTemplateService;
    private readonly IEmailSender _emailSender;
    private readonly CommunicationLogService _communicationLogService;

    public NotificationService(
        INotificationRepository notificationRepo,
        ITenantContext tenantContext,
        EmailTemplateService emailTemplateService,
        IEmailSender emailSender,
        CommunicationLogService communicationLogService)
    {
        _notificationRepo = notificationRepo;
        _tenantContext = tenantContext;
        _emailTemplateService = emailTemplateService;
        _emailSender = emailSender;
        _communicationLogService = communicationLogService;
    }

    public async Task<NotificationDto> CreateAsync(Guid userId, string title, string message,
        NotificationType type, string? link = null, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var notification = Notification.Create(tenantId, userId, title, message, type, link);
        await _notificationRepo.AddAsync(notification, ct);
        return MapToDto(notification);
    }

    public async Task<NotificationPagedResult> GetForUserAsync(Guid userId, int page = 1, int pageSize = 20,
        bool? unreadOnly = null, string? type = null, CancellationToken ct = default)
    {
        var (items, totalCount) = await _notificationRepo.GetByUserIdAsync(userId, page, pageSize, unreadOnly, type, ct);
        return new NotificationPagedResult(
            items.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize);
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var count = await _notificationRepo.GetUnreadCountAsync(userId, ct);
        return new UnreadCountDto(count);
    }

    public async Task<NotificationDto?> MarkReadAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await _notificationRepo.GetByIdAsync(id, ct);
        if (notification is null) return null;

        notification.MarkRead();
        await _notificationRepo.SaveAsync(ct);
        return MapToDto(notification);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        await _notificationRepo.MarkAllReadAsync(userId, ct);
    }

    public async Task<NotificationDto> NotifyInvoiceSubmittedAsync(Guid userId, string invoiceNumber,
        string providerName, string participantName, string amount, string? recipientEmail = null,
        CancellationToken ct = default)
    {
        var notification = await CreateAsync(userId,
            $"Invoice {invoiceNumber} Submitted",
            $"Invoice {invoiceNumber} has been submitted by {providerName} for {participantName}. Amount: {amount}.",
            NotificationType.InvoiceSubmitted,
            link: null,
            ct);

        if (recipientEmail is not null)
        {
            await SendTemplateEmailAsync("invoice_submitted", recipientEmail,
                new Dictionary<string, string>
                {
                    ["invoice_number"] = invoiceNumber,
                    ["provider_name"] = providerName,
                    ["participant_name"] = participantName,
                    ["amount"] = amount
                },
                "Invoice", null, ct);
        }

        return notification;
    }

    public async Task<NotificationDto> NotifyPlanExpiringAsync(Guid userId, string planNumber,
        string participantName, string expiryDate, int daysRemaining, string? recipientEmail = null,
        CancellationToken ct = default)
    {
        var notification = await CreateAsync(userId,
            $"Plan {planNumber} Expiring",
            $"Plan {planNumber} for {participantName} expires on {expiryDate} ({daysRemaining} days remaining).",
            NotificationType.PlanExpiring,
            link: null,
            ct);

        if (recipientEmail is not null)
        {
            await SendTemplateEmailAsync("plan_expiring", recipientEmail,
                new Dictionary<string, string>
                {
                    ["plan_number"] = planNumber,
                    ["participant_name"] = participantName,
                    ["expiry_date"] = expiryDate,
                    ["days_remaining"] = daysRemaining.ToString()
                },
                "Plan", null, ct);
        }

        return notification;
    }

    public async Task<NotificationDto> NotifyBudgetAlertAsync(Guid userId, string planNumber,
        string categoryName, string utilisation, string alertMessage, string? recipientEmail = null,
        CancellationToken ct = default)
    {
        var notification = await CreateAsync(userId,
            $"Budget Alert: {categoryName}",
            alertMessage,
            NotificationType.BudgetAlert,
            link: null,
            ct);

        if (recipientEmail is not null)
        {
            await SendTemplateEmailAsync("budget_alert", recipientEmail,
                new Dictionary<string, string>
                {
                    ["plan_number"] = planNumber,
                    ["category_name"] = categoryName,
                    ["utilisation"] = utilisation,
                    ["alert_message"] = alertMessage
                },
                "BudgetAlert", null, ct);
        }

        return notification;
    }

    private async Task SendTemplateEmailAsync(string templateName, string recipientEmail,
        Dictionary<string, string> variables, string? relatedEntityType, string? relatedEntityId,
        CancellationToken ct)
    {
        try
        {
            var rendered = await _emailTemplateService.RenderAsync(templateName, variables, ct);
            if (rendered is null) return;

            var (subject, body) = rendered.Value;
            await _emailSender.SendAsync(recipientEmail, subject, body, ct: ct);
            await _communicationLogService.LogAsync(recipientEmail, subject, body, "sent",
                templateName, relatedEntityType, relatedEntityId, ct: ct);
        }
        catch (Exception ex)
        {
            await _communicationLogService.LogAsync(recipientEmail, $"[{templateName}]", "",
                "failed", templateName, relatedEntityType, relatedEntityId, ex.Message, ct);
        }
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type.ToString(),
            notification.IsRead,
            notification.Link,
            notification.CreatedAt,
            notification.ReadAt);
    }
}
