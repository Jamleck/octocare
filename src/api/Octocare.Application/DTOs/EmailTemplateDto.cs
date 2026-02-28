namespace Octocare.Application.DTOs;

public record EmailTemplateDto(
    Guid Id,
    string Name,
    string Subject,
    string Body,
    bool IsActive,
    DateTimeOffset UpdatedAt);

public record UpdateEmailTemplateRequest(
    string Subject,
    string Body);

public record PreviewEmailTemplateRequest(
    Dictionary<string, string> Variables);

public record EmailTemplatePreviewDto(
    string Subject,
    string Body);
