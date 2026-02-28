using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class CommunicationLogController : ControllerBase
{
    private readonly CommunicationLogService _communicationLogService;

    public CommunicationLogController(CommunicationLogService communicationLogService)
    {
        _communicationLogService = communicationLogService;
    }

    [HttpGet("api/communication-log")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<CommunicationLogPagedResult>> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? recipientEmail = null,
        [FromQuery] string? templateName = null,
        CancellationToken ct = default)
    {
        var result = await _communicationLogService.GetLogsAsync(page, pageSize, recipientEmail, templateName, ct);
        return Ok(result);
    }
}
