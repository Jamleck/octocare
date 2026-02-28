using System.Text.RegularExpressions;
using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Application.Services;

public partial class EmailTemplateService
{
    private readonly IEmailTemplateRepository _templateRepo;
    private readonly ITenantContext _tenantContext;

    public EmailTemplateService(IEmailTemplateRepository templateRepo, ITenantContext tenantContext)
    {
        _templateRepo = templateRepo;
        _tenantContext = tenantContext;
    }

    public async Task<EmailTemplateDto?> GetTemplateAsync(string name, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByNameAsync(name, ct);
        return template is not null ? MapToDto(template) : null;
    }

    public async Task<EmailTemplateDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByIdAsync(id, ct);
        return template is not null ? MapToDto(template) : null;
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> GetAllAsync(CancellationToken ct = default)
    {
        var templates = await _templateRepo.GetAllAsync(ct);
        return templates.Select(MapToDto).ToList();
    }

    public async Task<EmailTemplateDto> UpdateAsync(Guid id, UpdateEmailTemplateRequest request, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Email template with ID {id} not found.");

        template.Update(request.Subject, request.Body);
        await _templateRepo.SaveAsync(ct);
        return MapToDto(template);
    }

    public async Task<EmailTemplatePreviewDto> PreviewAsync(Guid id, Dictionary<string, string> variables, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Email template with ID {id} not found.");

        var renderedSubject = RenderTemplate(template.Subject, variables);
        var renderedBody = RenderTemplate(template.Body, variables);

        return new EmailTemplatePreviewDto(renderedSubject, renderedBody);
    }

    public string RenderTemplate(string template, Dictionary<string, string> variables)
    {
        return VariablePlaceholderRegex().Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    public async Task<(string Subject, string Body)?> RenderAsync(string templateName, Dictionary<string, string> variables, CancellationToken ct = default)
    {
        var template = await _templateRepo.GetByNameAsync(templateName, ct);
        if (template is null) return null;

        var subject = RenderTemplate(template.Subject, variables);
        var body = RenderTemplate(template.Body, variables);
        return (subject, body);
    }

    public async Task SeedDefaultTemplatesAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var templates = new (string Name, string Subject, string Body)[]
        {
            ("invoice_submitted",
             "Invoice {{invoice_number}} Submitted",
             "<h2>Invoice Submitted</h2><p>Invoice <strong>{{invoice_number}}</strong> has been submitted by {{provider_name}} for participant {{participant_name}}.</p><p>Amount: {{amount}}</p><p>Please review and approve the invoice at your earliest convenience.</p>"),

            ("plan_expiring",
             "Plan {{plan_number}} Expiring Soon",
             "<h2>Plan Expiring</h2><p>Plan <strong>{{plan_number}}</strong> for {{participant_name}} will expire on {{expiry_date}}.</p><p>Days remaining: {{days_remaining}}</p><p>Please begin the plan transition process to ensure continuity of services.</p>"),

            ("budget_alert",
             "Budget Alert: {{category_name}}",
             "<h2>Budget Alert</h2><p>A budget alert has been generated for plan {{plan_number}}.</p><p>Category: {{category_name}}</p><p>Utilisation: {{utilisation}}%</p><p>{{alert_message}}</p>"),

            ("statement_ready",
             "Participant Statement Ready - {{participant_name}}",
             "<h2>Statement Ready</h2><p>A new participant statement has been generated for <strong>{{participant_name}}</strong>.</p><p>Period: {{period_start}} to {{period_end}}</p><p>The statement is now available for review and distribution.</p>")
        };

        foreach (var (name, subject, body) in templates)
        {
            if (!await _templateRepo.ExistsByNameAsync(name, ct: ct))
            {
                var template = EmailTemplate.Create(tenantId, name, subject, body);
                await _templateRepo.AddAsync(template, ct);
            }
        }
    }

    private static EmailTemplateDto MapToDto(EmailTemplate template)
    {
        return new EmailTemplateDto(
            template.Id,
            template.Name,
            template.Subject,
            template.Body,
            template.IsActive,
            template.UpdatedAt);
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex VariablePlaceholderRegex();
}
