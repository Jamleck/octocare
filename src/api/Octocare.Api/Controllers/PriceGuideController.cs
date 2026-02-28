using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;
using Octocare.Application.Validators;
using Octocare.Domain.Enums;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class PriceGuideController : ControllerBase
{
    private readonly PriceGuideService _priceGuideService;

    public PriceGuideController(PriceGuideService priceGuideService)
    {
        _priceGuideService = priceGuideService;
    }

    [HttpGet("api/price-guide/versions")]
    public async Task<ActionResult<IReadOnlyList<PriceGuideVersionDto>>> GetVersions(CancellationToken ct)
    {
        var versions = await _priceGuideService.GetVersionsAsync(ct);
        return Ok(versions);
    }

    [HttpGet("api/price-guide/versions/current")]
    public async Task<ActionResult<PriceGuideVersionDto>> GetCurrentVersion(CancellationToken ct)
    {
        var version = await _priceGuideService.GetCurrentVersionAsync(ct);
        return version is not null ? Ok(version) : NotFound();
    }

    [HttpGet("api/price-guide/versions/{id:guid}/items")]
    public async Task<ActionResult<PagedResult<SupportItemDto>>> GetItems(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] SupportCategory? category = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _priceGuideService.GetItemsAsync(id, page, pageSize, search, category, ct);
        return Ok(result);
    }

    [HttpPost("api/admin/price-guide/import")]
    [Authorize(Policy = "CanManageOrg")]
    public async Task<ActionResult<PriceGuideVersionDto>> Import(
        ImportPriceGuideRequest request, CancellationToken ct)
    {
        var (isValid, errors) = PriceGuideValidation.ValidateImport(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _priceGuideService.ImportVersionAsync(request, ct);
            return CreatedAtAction(nameof(GetCurrentVersion), result);
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
