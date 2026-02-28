using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class PlanTransitionsController : ControllerBase
{
    private readonly PlanTransitionService _transitionService;

    public PlanTransitionsController(PlanTransitionService transitionService)
    {
        _transitionService = transitionService;
    }

    [HttpGet("api/plan-transitions")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<IReadOnlyList<PlanTransitionDto>>> GetTransitions(
        [FromQuery] Guid? planId = null,
        CancellationToken ct = default)
    {
        IReadOnlyList<PlanTransitionDto> transitions;

        if (planId.HasValue)
            transitions = await _transitionService.GetByPlanIdAsync(planId.Value, ct);
        else
            transitions = await _transitionService.GetAllAsync(ct);

        return Ok(transitions);
    }

    [HttpGet("api/plan-transitions/{id:guid}")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<PlanTransitionDto>> GetById(Guid id, CancellationToken ct)
    {
        var transition = await _transitionService.GetByIdAsync(id, ct);
        return transition is not null ? Ok(transition) : NotFound();
    }

    [HttpPost("api/plan-transitions")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<PlanTransitionDto>> InitiateTransition(
        InitiateTransitionRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _transitionService.InitiateTransitionAsync(request.OldPlanId, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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

    [HttpPut("api/plan-transitions/{id:guid}")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<PlanTransitionDto>> UpdateTransition(
        Guid id, UpdateTransitionRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _transitionService.UpdateChecklistAsync(id, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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

    [HttpPost("api/plan-transitions/{id:guid}/complete")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<PlanTransitionDto>> CompleteTransition(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _transitionService.CompleteTransitionAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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
}
