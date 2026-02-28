using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("api/payments")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<PaymentBatchPagedResult>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _paymentService.GetPagedAsync(page, pageSize, status, ct);
        return Ok(result);
    }

    [HttpGet("api/payments/{id:guid}")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<ActionResult<PaymentBatchDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var batch = await _paymentService.GetByIdAsync(id, ct);
        return batch is not null ? Ok(batch) : NotFound();
    }

    [HttpPost("api/payments")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<PaymentBatchDetailDto>> Create(CancellationToken ct)
    {
        try
        {
            var result = await _paymentService.CreateBatchAsync(ct);
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

    [HttpGet("api/payments/{id:guid}/aba")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<IActionResult> DownloadAba(Guid id, CancellationToken ct)
    {
        try
        {
            var abaContent = await _paymentService.GenerateAbaAsync(id, ct);
            var bytes = Encoding.ASCII.GetBytes(abaContent);
            return File(bytes, "text/plain", $"payment-{id}.aba");
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

    [HttpPost("api/payments/{id:guid}/send")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<PaymentBatchDetailDto>> MarkSent(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _paymentService.MarkSentAsync(id, ct);
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

    [HttpPost("api/payments/{id:guid}/confirm")]
    [Authorize(Policy = "CanWriteFinance")]
    public async Task<ActionResult<PaymentBatchDetailDto>> MarkConfirmed(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _paymentService.MarkConfirmedAsync(id, ct);
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
