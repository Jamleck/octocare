using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class BudgetController : ControllerBase
{
    private readonly BudgetProjectionService _budgetProjectionService;

    public BudgetController(BudgetProjectionService budgetProjectionService)
    {
        _budgetProjectionService = budgetProjectionService;
    }

    [HttpGet("api/plans/{planId:guid}/budget-overview")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<BudgetOverviewDto>> GetBudgetOverview(
        Guid planId, CancellationToken ct)
    {
        var overview = await _budgetProjectionService.GetProjectionsForPlanAsync(planId, ct);
        return overview is not null ? Ok(overview) : NotFound();
    }
}
