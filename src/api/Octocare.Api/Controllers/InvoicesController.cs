using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;
using Octocare.Application.Validators;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("api/invoices")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<InvoicePagedResult>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? participantId = null,
        [FromQuery] Guid? providerId = null,
        CancellationToken ct = default)
    {
        var result = await _invoiceService.GetPagedAsync(page, pageSize, status, participantId, providerId, ct);
        return Ok(result);
    }

    [HttpGet("api/invoices/{id:guid}")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken ct)
    {
        var invoice = await _invoiceService.GetByIdAsync(id, ct);
        return invoice is not null ? Ok(invoice) : NotFound();
    }

    [HttpPost("api/invoices")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<InvoiceDto>> Create(CreateInvoiceRequest request, CancellationToken ct)
    {
        var (isValid, errors) = InvoiceValidation.ValidateCreate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _invoiceService.CreateAsync(request, ct);
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

    [HttpPost("api/invoices/{id:guid}/approve")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<InvoiceDto>> Approve(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _invoiceService.ApproveAsync(id, ct);
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

    [HttpPost("api/invoices/{id:guid}/reject")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<InvoiceDto>> Reject(Guid id, RejectInvoiceRequest request, CancellationToken ct)
    {
        var (isValid, errors) = InvoiceValidation.ValidateReject(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _invoiceService.RejectAsync(id, request.Reason, ct);
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

    [HttpPost("api/invoices/{id:guid}/dispute")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<InvoiceDto>> Dispute(Guid id, DisputeInvoiceRequest request, CancellationToken ct)
    {
        var (isValid, errors) = InvoiceValidation.ValidateDispute(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _invoiceService.DisputeAsync(id, request.Reason, ct);
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

    [HttpPost("api/invoices/{id:guid}/mark-paid")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<InvoiceDto>> MarkPaid(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _invoiceService.MarkPaidAsync(id, ct);
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
