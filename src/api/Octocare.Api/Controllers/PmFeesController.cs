using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class PmFeesController : ControllerBase
{
    private readonly PmFeeService _pmFeeService;

    public PmFeesController(PmFeeService pmFeeService)
    {
        _pmFeeService = pmFeeService;
    }

    [HttpPost("api/admin/pm-fees/generate-monthly")]
    [Authorize(Policy = "CanManageOrg")]
    public async Task<ActionResult<GenerateMonthlyFeesResponse>> GenerateMonthly(
        GenerateMonthlyFeesRequest request, CancellationToken ct)
    {
        if (request.Month < 1 || request.Month > 12)
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["Month"] = ["Month must be between 1 and 12."]
                }));

        if (request.Year < 2020 || request.Year > 2100)
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    ["Year"] = ["Year must be between 2020 and 2100."]
                }));

        try
        {
            var result = await _pmFeeService.GenerateMonthlyFeesAsync(request.Month, request.Year, ct);
            return Ok(result);
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

    [HttpPost("api/admin/pm-fees/generate-setup/{participantId:guid}")]
    [Authorize(Policy = "CanManageOrg")]
    public async Task<ActionResult<GenerateSetupFeeResponse>> GenerateSetup(Guid participantId, CancellationToken ct)
    {
        try
        {
            var result = await _pmFeeService.GenerateSetupFeeAsync(participantId, ct);
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
