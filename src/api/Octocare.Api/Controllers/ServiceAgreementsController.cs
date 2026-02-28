using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;
using Octocare.Application.Validators;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class ServiceAgreementsController : ControllerBase
{
    private readonly ServiceAgreementService _agreementService;

    public ServiceAgreementsController(ServiceAgreementService agreementService)
    {
        _agreementService = agreementService;
    }

    [HttpGet("api/participants/{participantId:guid}/agreements")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<IReadOnlyList<ServiceAgreementDto>>> GetByParticipant(
        Guid participantId, CancellationToken ct)
    {
        var agreements = await _agreementService.GetByParticipantIdAsync(participantId, ct);
        return Ok(agreements);
    }

    [HttpGet("api/agreements/{id:guid}")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<ServiceAgreementDto>> GetById(Guid id, CancellationToken ct)
    {
        var agreement = await _agreementService.GetByIdAsync(id, ct);
        return agreement is not null ? Ok(agreement) : NotFound();
    }

    [HttpPost("api/participants/{participantId:guid}/agreements")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<ServiceAgreementDto>> Create(
        Guid participantId, CreateServiceAgreementRequest request, CancellationToken ct)
    {
        var (isValid, errors) = ServiceAgreementValidation.ValidateCreate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _agreementService.CreateAsync(participantId, request, ct);
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

    [HttpPost("api/agreements/{id:guid}/activate")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<ServiceAgreementDto>> Activate(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _agreementService.ActivateAsync(id, ct);
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

    [HttpPost("api/agreements/{id:guid}/bookings")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<ServiceBookingDto>> AddBooking(
        Guid id, CreateServiceBookingRequest request, CancellationToken ct)
    {
        var (isValid, errors) = ServiceAgreementValidation.ValidateBooking(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _agreementService.AddBookingAsync(id, request, ct);
            return Created($"/api/agreements/{id}", result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("api/agreements/{id:guid}/bookings/{bookingId:guid}/cancel")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<ServiceBookingDto>> CancelBooking(
        Guid id, Guid bookingId, CancellationToken ct)
    {
        try
        {
            var result = await _agreementService.CancelBookingAsync(id, bookingId, ct);
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
