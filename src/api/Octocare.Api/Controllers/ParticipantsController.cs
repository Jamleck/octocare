using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;
using Octocare.Application.Validators;

namespace Octocare.Api.Controllers;

[ApiController]
[Route("api/participants")]
[Authorize]
public class ParticipantsController : ControllerBase
{
    private readonly ParticipantService _participantService;

    public ParticipantsController(ParticipantService participantService)
    {
        _participantService = participantService;
    }

    [HttpGet]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<PagedResult<ParticipantDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _participantService.GetPagedAsync(page, pageSize, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<ParticipantDto>> GetById(Guid id, CancellationToken ct)
    {
        var participant = await _participantService.GetByIdAsync(id, ct);
        return participant is not null ? Ok(participant) : NotFound();
    }

    [HttpPost]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<ParticipantDto>> Create(
        CreateParticipantRequest request, CancellationToken ct)
    {
        var (isValid, errors) = ParticipantValidation.ValidateCreate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _participantService.CreateAsync(request, ct);
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
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<ParticipantDto>> Update(
        Guid id, UpdateParticipantRequest request, CancellationToken ct)
    {
        var (isValid, errors) = ParticipantValidation.ValidateUpdate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _participantService.UpdateAsync(id, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await _participantService.DeactivateAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
