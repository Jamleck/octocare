using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Application.Services;
using Octocare.Domain.Entities;

namespace Octocare.Tests.Services;

public class EmailTemplateServiceTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void RenderTemplate_ReplacesAllVariables()
    {
        var repo = new FakeEmailTemplateRepository();
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var template = "Hello {{name}}, your invoice {{invoice_number}} is ready.";
        var variables = new Dictionary<string, string>
        {
            ["name"] = "Jane",
            ["invoice_number"] = "INV-001"
        };

        var result = service.RenderTemplate(template, variables);

        Assert.Equal("Hello Jane, your invoice INV-001 is ready.", result);
    }

    [Fact]
    public void RenderTemplate_LeavesMissingVariablesIntact()
    {
        var repo = new FakeEmailTemplateRepository();
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var template = "Hello {{name}}, your plan {{plan_number}} expires on {{date}}.";
        var variables = new Dictionary<string, string>
        {
            ["name"] = "Bob"
        };

        var result = service.RenderTemplate(template, variables);

        Assert.Equal("Hello Bob, your plan {{plan_number}} expires on {{date}}.", result);
    }

    [Fact]
    public void RenderTemplate_HandlesEmptyVariables()
    {
        var repo = new FakeEmailTemplateRepository();
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var template = "No variables here.";
        var variables = new Dictionary<string, string>();

        var result = service.RenderTemplate(template, variables);

        Assert.Equal("No variables here.", result);
    }

    [Fact]
    public void RenderTemplate_HandlesMultipleOccurrences()
    {
        var repo = new FakeEmailTemplateRepository();
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var template = "{{name}} said hello. {{name}} waved goodbye.";
        var variables = new Dictionary<string, string>
        {
            ["name"] = "Alice"
        };

        var result = service.RenderTemplate(template, variables);

        Assert.Equal("Alice said hello. Alice waved goodbye.", result);
    }

    [Fact]
    public async Task RenderAsync_ReturnsRenderedTemplate()
    {
        var template = EmailTemplate.Create(_tenantId, "test_template", "Hello {{name}}", "<p>Hi {{name}}</p>");
        var repo = new FakeEmailTemplateRepository();
        repo.AddTemplate(template);
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var result = await service.RenderAsync("test_template",
            new Dictionary<string, string> { ["name"] = "Jane" });

        Assert.NotNull(result);
        Assert.Equal("Hello Jane", result.Value.Subject);
        Assert.Equal("<p>Hi Jane</p>", result.Value.Body);
    }

    [Fact]
    public async Task RenderAsync_ReturnsNull_WhenTemplateNotFound()
    {
        var repo = new FakeEmailTemplateRepository();
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var result = await service.RenderAsync("nonexistent",
            new Dictionary<string, string> { ["name"] = "Jane" });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTemplates()
    {
        var template1 = EmailTemplate.Create(_tenantId, "template_a", "Subject A", "Body A");
        var template2 = EmailTemplate.Create(_tenantId, "template_b", "Subject B", "Body B");
        var repo = new FakeEmailTemplateRepository();
        repo.AddTemplate(template1);
        repo.AddTemplate(template2);
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesSubjectAndBody()
    {
        var template = EmailTemplate.Create(_tenantId, "test_template", "Old Subject", "Old Body");
        var repo = new FakeEmailTemplateRepository();
        repo.AddTemplate(template);
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var result = await service.UpdateAsync(template.Id,
            new UpdateEmailTemplateRequest("New Subject", "New Body"));

        Assert.Equal("New Subject", result.Subject);
        Assert.Equal("New Body", result.Body);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFound_WhenTemplateNotFound()
    {
        var repo = new FakeEmailTemplateRepository();
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new UpdateEmailTemplateRequest("Subject", "Body")));
    }

    [Fact]
    public async Task PreviewAsync_ReturnsRenderedPreview()
    {
        var template = EmailTemplate.Create(_tenantId, "test_template",
            "Invoice {{invoice_number}} Submitted",
            "<p>Invoice {{invoice_number}} from {{provider}}</p>");
        var repo = new FakeEmailTemplateRepository();
        repo.AddTemplate(template);
        var tenantContext = new FakeTenantContext(_tenantId);
        var service = new EmailTemplateService(repo, tenantContext);

        var result = await service.PreviewAsync(template.Id,
            new Dictionary<string, string>
            {
                ["invoice_number"] = "INV-001",
                ["provider"] = "Allied Health"
            });

        Assert.Equal("Invoice INV-001 Submitted", result.Subject);
        Assert.Equal("<p>Invoice INV-001 from Allied Health</p>", result.Body);
    }

    #region Test Doubles

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

        public Task SaveAsync(CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
            => Task.FromResult(_templates.Any(t => t.Name == name && (!excludeId.HasValue || t.Id != excludeId.Value)));
    }

    private class FakeTenantContext : ITenantContext
    {
        public FakeTenantContext(Guid tenantId) => TenantId = tenantId;
        public Guid? TenantId { get; }
        public void SetTenant(Guid tenantId) { }
    }

    #endregion
}
