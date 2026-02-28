using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly AlertService _alertService;

    public AlertsController(AlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet("api/alerts")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<IReadOnlyList<AlertDto>>> GetAlerts(
        [FromQuery] Guid? planId = null,
        CancellationToken ct = default)
    {
        var alerts = await _alertService.GetAlertsAsync(planId, ct);
        return Ok(alerts);
    }

    [HttpGet("api/alerts/summary")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<AlertSummaryDto>> GetSummary(CancellationToken ct)
    {
        var summary = await _alertService.GetSummaryAsync(ct);
        return Ok(summary);
    }

    [HttpPost("api/alerts/generate")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<IReadOnlyList<AlertDto>>> GenerateAlerts(CancellationToken ct)
    {
        try
        {
            var alerts = await _alertService.GenerateAlertsForAllActivePlansAsync(ct);
            return Ok(alerts);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPut("api/alerts/{id:guid}/read")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<AlertDto>> MarkRead(Guid id, CancellationToken ct)
    {
        var alert = await _alertService.MarkReadAsync(id, ct);
        return alert is not null ? Ok(alert) : NotFound();
    }

    [HttpPut("api/alerts/{id:guid}/dismiss")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<AlertDto>> Dismiss(Guid id, CancellationToken ct)
    {
        var alert = await _alertService.DismissAsync(id, ct);
        return alert is not null ? Ok(alert) : NotFound();
    }
}
