using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;
using Octocare.Application.Validators;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class PlansController : ControllerBase
{
    private readonly PlanService _planService;

    public PlansController(PlanService planService)
    {
        _planService = planService;
    }

    [HttpGet("api/participants/{participantId:guid}/plans")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<IReadOnlyList<PlanDto>>> GetByParticipant(
        Guid participantId, CancellationToken ct)
    {
        var plans = await _planService.GetByParticipantIdAsync(participantId, ct);
        return Ok(plans);
    }

    [HttpGet("api/plans/{id:guid}")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<PlanDto>> GetById(Guid id, CancellationToken ct)
    {
        var plan = await _planService.GetByIdAsync(id, ct);
        return plan is not null ? Ok(plan) : NotFound();
    }

    [HttpPost("api/participants/{participantId:guid}/plans")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<PlanDto>> Create(
        Guid participantId, CreatePlanRequest request, CancellationToken ct)
    {
        var (isValid, errors) = PlanValidation.ValidateCreate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _planService.CreateAsync(participantId, request, ct);
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

    [HttpPut("api/plans/{id:guid}")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<PlanDto>> Update(
        Guid id, UpdatePlanRequest request, CancellationToken ct)
    {
        var (isValid, errors) = PlanValidation.ValidateUpdate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _planService.UpdateAsync(id, request, ct);
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

    [HttpPost("api/plans/{id:guid}/activate")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<PlanDto>> Activate(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _planService.ActivateAsync(id, ct);
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

    [HttpPost("api/plans/{id:guid}/budget-categories")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<BudgetCategoryDto>> AddBudgetCategory(
        Guid id, CreateBudgetCategoryRequest request, CancellationToken ct)
    {
        var (isValid, errors) = PlanValidation.ValidateBudgetCategory(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _planService.AddBudgetCategoryAsync(id, request, ct);
            return Created($"/api/plans/{id}", result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("api/plans/{id:guid}/budget-categories/{categoryId:guid}")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<BudgetCategoryDto>> UpdateBudgetCategory(
        Guid id, Guid categoryId, UpdateBudgetCategoryRequest request, CancellationToken ct)
    {
        var (isValid, errors) = PlanValidation.ValidateBudgetCategoryUpdate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _planService.UpdateBudgetCategoryAsync(id, categoryId, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
