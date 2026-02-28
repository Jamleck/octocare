using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class ClaimsController : ControllerBase
{
    private readonly ClaimService _claimService;

    public ClaimsController(ClaimService claimService)
    {
        _claimService = claimService;
    }

    [HttpGet("api/claims")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<ClaimPagedResult>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _claimService.GetPagedAsync(page, pageSize, status, ct);
        return Ok(result);
    }

    [HttpGet("api/claims/{id:guid}")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<ClaimDto>> GetById(Guid id, CancellationToken ct)
    {
        var claim = await _claimService.GetByIdAsync(id, ct);
        return claim is not null ? Ok(claim) : NotFound();
    }

    [HttpPost("api/claims")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<ClaimDto>> Create(CreateClaimRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _claimService.CreateBatchAsync(request, ct);
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

    [HttpPost("api/claims/{id:guid}/submit")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<ClaimDto>> Submit(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _claimService.SubmitAsync(id, ct);
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

    [HttpPost("api/claims/{id:guid}/outcome")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<ClaimDto>> RecordOutcome(Guid id, RecordClaimOutcomeRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _claimService.RecordOutcomeAsync(id, request, ct);
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

    [HttpGet("api/claims/{id:guid}/csv")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<IActionResult> DownloadCsv(Guid id, CancellationToken ct)
    {
        try
        {
            var csvBytes = await _claimService.GenerateCsvAsync(id, ct);
            return File(csvBytes, "text/csv", $"claim-{id}.csv");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
