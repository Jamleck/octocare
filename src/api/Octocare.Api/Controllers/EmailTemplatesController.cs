using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class EmailTemplatesController : ControllerBase
{
    private readonly EmailTemplateService _emailTemplateService;

    public EmailTemplatesController(EmailTemplateService emailTemplateService)
    {
        _emailTemplateService = emailTemplateService;
    }

    [HttpGet("api/email-templates")]
    [Authorize(Policy = "CanManageOrg")]
    public async Task<ActionResult<IReadOnlyList<EmailTemplateDto>>> GetAll(CancellationToken ct = default)
    {
        var templates = await _emailTemplateService.GetAllAsync(ct);
        return Ok(templates);
    }

    [HttpGet("api/email-templates/{id:guid}")]
    [Authorize(Policy = "CanManageOrg")]
    public async Task<ActionResult<EmailTemplateDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var template = await _emailTemplateService.GetByIdAsync(id, ct);
        return template is not null ? Ok(template) : NotFound();
    }

    [HttpPut("api/email-templates/{id:guid}")]
    [Authorize(Policy = "CanManageOrg")]
    public async Task<ActionResult<EmailTemplateDto>> Update(Guid id, [FromBody] UpdateEmailTemplateRequest request, CancellationToken ct = default)
    {
        try
        {
            var template = await _emailTemplateService.UpdateAsync(id, request, ct);
            return Ok(template);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("api/email-templates/{id:guid}/preview")]
    [Authorize(Policy = "CanManageOrg")]
    public async Task<ActionResult<EmailTemplatePreviewDto>> Preview(Guid id, [FromBody] PreviewEmailTemplateRequest request, CancellationToken ct = default)
    {
        try
        {
            var preview = await _emailTemplateService.PreviewAsync(id, request.Variables, ct);
            return Ok(preview);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
