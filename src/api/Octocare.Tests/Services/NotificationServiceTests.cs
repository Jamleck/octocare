using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Application.Services;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Services;

public class NotificationServiceTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task CreateAsync_CreatesNotification()
    {
        var (service, notificationRepo, _, _, _) = CreateService();

        var result = await service.CreateAsync(_userId, "Test Title", "Test Message",
            NotificationType.General, "/test");

        Assert.NotNull(result);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("Test Message", result.Message);
        Assert.Equal("General", result.Type);
        Assert.Equal("/test", result.Link);
        Assert.False(result.IsRead);
        Assert.Single(notificationRepo.Notifications);
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsPagedNotifications()
    {
        var (service, notificationRepo, _, _, _) = CreateService();

        // Create 3 notifications
        await service.CreateAsync(_userId, "Title 1", "Message 1", NotificationType.General);
        await service.CreateAsync(_userId, "Title 2", "Message 2", NotificationType.InvoiceSubmitted);
        await service.CreateAsync(_userId, "Title 3", "Message 3", NotificationType.PlanExpiring);

        var result = await service.GetForUserAsync(_userId, page: 1, pageSize: 10);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        var (service, _, _, _, _) = CreateService();

        await service.CreateAsync(_userId, "Title 1", "Message 1", NotificationType.General);
        await service.CreateAsync(_userId, "Title 2", "Message 2", NotificationType.General);

        var result = await service.GetUnreadCountAsync(_userId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task MarkReadAsync_MarksNotificationAsRead()
    {
        var (service, notificationRepo, _, _, _) = CreateService();

        var notification = await service.CreateAsync(_userId, "Title", "Message", NotificationType.General);

        var result = await service.MarkReadAsync(notification.Id);

        Assert.NotNull(result);
        Assert.True(result.IsRead);
        Assert.NotNull(result.ReadAt);
    }

    [Fact]
    public async Task MarkReadAsync_ReturnsNull_WhenNotFound()
    {
        var (service, _, _, _, _) = CreateService();

        var result = await service.MarkReadAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task MarkAllReadAsync_MarksAllUnreadAsRead()
    {
        var (service, notificationRepo, _, _, _) = CreateService();

        await service.CreateAsync(_userId, "Title 1", "Message 1", NotificationType.General);
        await service.CreateAsync(_userId, "Title 2", "Message 2", NotificationType.General);

        await service.MarkAllReadAsync(_userId);

        var unreadCount = await service.GetUnreadCountAsync(_userId);
        Assert.Equal(0, unreadCount.Count);
    }

    [Fact]
    public async Task NotifyInvoiceSubmittedAsync_CreatesNotificationAndSendsEmail()
    {
        var (service, notificationRepo, _, emailSender, _) = CreateService();

        var result = await service.NotifyInvoiceSubmittedAsync(_userId,
            "INV-001", "Allied Health", "Sarah Johnson", "$1,234.56",
            recipientEmail: "admin@test.com");

        Assert.NotNull(result);
        Assert.Equal("InvoiceSubmitted", result.Type);
        Assert.Contains("INV-001", result.Title);
        Assert.Single(emailSender.SentEmails);
    }

    [Fact]
    public async Task NotifyPlanExpiringAsync_CreatesNotificationAndSendsEmail()
    {
        var (service, _, _, emailSender, _) = CreateService();

        var result = await service.NotifyPlanExpiringAsync(_userId,
            "NDIS-2025-001", "Sarah Johnson", "2026-06-30", 30,
            recipientEmail: "admin@test.com");

        Assert.NotNull(result);
        Assert.Equal("PlanExpiring", result.Type);
        Assert.Contains("NDIS-2025-001", result.Title);
        Assert.Single(emailSender.SentEmails);
    }

    [Fact]
    public async Task NotifyBudgetAlertAsync_CreatesNotification()
    {
        var (service, _, _, _, _) = CreateService();

        var result = await service.NotifyBudgetAlertAsync(_userId,
            "NDIS-2025-001", "Core - Daily Activities", "85.5",
            "Budget is approaching threshold.");

        Assert.NotNull(result);
        Assert.Equal("BudgetAlert", result.Type);
        Assert.Contains("Core - Daily Activities", result.Title);
    }

    private (NotificationService, FakeNotificationRepository, FakeEmailTemplateRepository, FakeEmailSender, FakeCommunicationLogRepository) CreateService()
    {
        var notificationRepo = new FakeNotificationRepository();
        var tenantContext = new FakeTenantContext(_tenantId);
        var emailTemplateRepo = new FakeEmailTemplateRepository();
        var emailSender = new FakeEmailSender();
        var commLogRepo = new FakeCommunicationLogRepository();

        // Seed a default template so email sending works
        var invoiceTemplate = EmailTemplate.Create(_tenantId, "invoice_submitted",
            "Invoice {{invoice_number}} Submitted", "<p>Invoice {{invoice_number}}</p>");
        var planTemplate = EmailTemplate.Create(_tenantId, "plan_expiring",
            "Plan {{plan_number}} Expiring", "<p>Plan {{plan_number}}</p>");
        var budgetTemplate = EmailTemplate.Create(_tenantId, "budget_alert",
            "Budget Alert: {{category_name}}", "<p>{{alert_message}}</p>");
        emailTemplateRepo.AddTemplate(invoiceTemplate);
        emailTemplateRepo.AddTemplate(planTemplate);
        emailTemplateRepo.AddTemplate(budgetTemplate);

        var emailTemplateService = new EmailTemplateService(emailTemplateRepo, tenantContext);
        var commLogService = new CommunicationLogService(commLogRepo, tenantContext);
        var notificationService = new NotificationService(notificationRepo, tenantContext,
            emailTemplateService, emailSender, commLogService);

        return (notificationService, notificationRepo, emailTemplateRepo, emailSender, commLogRepo);
    }

    #region Test Doubles

    private class FakeNotificationRepository : INotificationRepository
    {
        public List<Notification> Notifications { get; } = new();

        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Notifications.FirstOrDefault(n => n.Id == id));

        public Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserIdAsync(
            Guid userId, int page, int pageSize, bool? unreadOnly = null,
            string? type = null, CancellationToken ct = default)
        {
            var query = Notifications.Where(n => n.UserId == userId);
            if (unreadOnly == true) query = query.Where(n => !n.IsRead);
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, out var notificationType))
                query = query.Where(n => n.Type == notificationType);

            var list = query.OrderByDescending(n => n.CreatedAt).ToList();
            var totalCount = list.Count;
            var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult<(IReadOnlyList<Notification>, int)>((items, totalCount));
        }

        public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(Notifications.Count(n => n.UserId == userId && !n.IsRead));

        public Task<Notification> AddAsync(Notification notification, CancellationToken ct = default)
        {
            Notifications.Add(notification);
            return Task.FromResult(notification);
        }

        public Task SaveAsync(CancellationToken ct = default)
            => Task.CompletedTask;

        public Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
        {
            foreach (var n in Notifications.Where(n => n.UserId == userId && !n.IsRead))
                n.MarkRead();
            return Task.CompletedTask;
        }
    }

    private class FakeEmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly List<EmailTemplate> _templates = new();

        public void AddTemplate(EmailTemplate template) => _templates.Add(template);

        public Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_templates.FirstOrDefault(t => t.Id == id));

        public Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken ct = default)
            => Task.FromResult(_templates.FirstOrDefault(t => t.Name == name && t.IsActive));

        public Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<EmailTemplate>>(_templates.ToList());

        public Task<EmailTemplate> AddAsync(EmailTemplate template, CancellationToken ct = default)
        {
            _templates.Add(template);
            return Task.FromResult(template);
        }

        public Task SaveAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
            => Task.FromResult(_templates.Any(t => t.Name == name && (!excludeId.HasValue || t.Id != excludeId.Value)));
    }

    private class FakeEmailSender : IEmailSender
    {
        public List<(string To, string Subject, string Body)> SentEmails { get; } = new();

        public Task SendAsync(string to, string subject, string body, byte[]? attachment = null,
            string? attachmentName = null, CancellationToken ct = default)
        {
            SentEmails.Add((to, subject, body));
            return Task.CompletedTask;
        }
    }

    private class FakeCommunicationLogRepository : ICommunicationLogRepository
    {
        private readonly List<CommunicationLog> _logs = new();

        public Task<(IReadOnlyList<CommunicationLog> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? recipientEmail = null,
            string? templateName = null, CancellationToken ct = default)
            => Task.FromResult<(IReadOnlyList<CommunicationLog>, int)>((_logs.ToList(), _logs.Count));

        public Task<CommunicationLog> AddAsync(CommunicationLog log, CancellationToken ct = default)
        {
            _logs.Add(log);
            return Task.FromResult(log);
        }

        public Task SaveAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private class FakeTenantContext : ITenantContext
    {
        public FakeTenantContext(Guid tenantId) => TenantId = tenantId;
        public Guid? TenantId { get; }
        public void SetTenant(Guid tenantId) { }
    }

    #endregion
}
