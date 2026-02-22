using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;
using Octocare.Application.Validators;

namespace Octocare.Api.Controllers;

[ApiController]
[Route("api/organisations")]
[Authorize]
public class OrganisationsController : ControllerBase
{
    private readonly OrganisationService _orgService;

    public OrganisationsController(OrganisationService orgService)
    {
        _orgService = orgService;
    }

    [HttpGet("current")]
    [Authorize(Policy = "CanReadOrg")]
    public async Task<ActionResult<OrganisationDto>> GetCurrent(CancellationToken ct)
    {
        var org = await _orgService.GetCurrentOrganisationAsync(ct);
        return org is not null ? Ok(org) : NotFound();
    }

    [HttpPut("current")]
    [Authorize(Policy = "CanManageOrg")]
    public async Task<ActionResult<OrganisationDto>> UpdateCurrent(
        UpdateOrganisationRequest request, CancellationToken ct)
    {
        var (isValid, errors) = OrganisationValidation.Validate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        var result = await _orgService.UpdateOrganisationAsync(request, ct);
        return Ok(result);
    }
}
