using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Route("api/proda")]
[Authorize]
public class ProdaController : ControllerBase
{
    private readonly IProdaPaceClient _prodaClient;
    private readonly ProdaSyncService _syncService;

    public ProdaController(IProdaPaceClient prodaClient, ProdaSyncService syncService)
    {
        _prodaClient = prodaClient;
        _syncService = syncService;
    }

    [HttpGet("participants/{ndisNumber}")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<ProdaParticipantInfo>> GetParticipant(
        string ndisNumber, CancellationToken ct)
    {
        var result = await _prodaClient.GetParticipantInfoAsync(ndisNumber, ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("plans/{ndisNumber}")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<ProdaPlanInfo>> GetPlan(
        string ndisNumber, CancellationToken ct)
    {
        var result = await _prodaClient.GetPlanInfoAsync(ndisNumber, ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("sync/participant/{participantId:guid}")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<SyncResult>> SyncParticipantPlan(
        Guid participantId, CancellationToken ct)
    {
        try
        {
            var result = await _syncService.SyncParticipantPlanAsync(participantId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("sync/plan/{planId:guid}/budget")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<SyncResult>> VerifyBudget(
        Guid planId, CancellationToken ct)
    {
        try
        {
            var result = await _syncService.VerifyBudgetAsync(planId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
