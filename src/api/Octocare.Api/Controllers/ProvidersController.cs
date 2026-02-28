using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;
using Octocare.Application.Validators;

namespace Octocare.Api.Controllers;

[ApiController]
[Route("api/providers")]
[Authorize]
public class ProvidersController : ControllerBase
{
    private readonly ProviderService _providerService;

    public ProvidersController(ProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    [Authorize(Policy = "CanReadProviders")]
    public async Task<ActionResult<PagedResult<ProviderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _providerService.GetPagedAsync(page, pageSize, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanReadProviders")]
    public async Task<ActionResult<ProviderDto>> GetById(Guid id, CancellationToken ct)
    {
        var provider = await _providerService.GetByIdAsync(id, ct);
        return provider is not null ? Ok(provider) : NotFound();
    }

    [HttpPost]
    [Authorize(Policy = "CanWriteProviders")]
    public async Task<ActionResult<ProviderDto>> Create(
        CreateProviderRequest request, CancellationToken ct)
    {
        var (isValid, errors) = ProviderValidation.ValidateCreate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _providerService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanWriteProviders")]
    public async Task<ActionResult<ProviderDto>> Update(
        Guid id, UpdateProviderRequest request, CancellationToken ct)
    {
        var (isValid, errors) = ProviderValidation.ValidateUpdate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _providerService.UpdateAsync(id, request, ct);
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
