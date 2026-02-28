using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class StatementsController : ControllerBase
{
    private readonly StatementService _statementService;

    public StatementsController(StatementService statementService)
    {
        _statementService = statementService;
    }

    [HttpGet("api/participants/{participantId:guid}/statements")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<ActionResult<IReadOnlyList<StatementDto>>> GetStatements(Guid participantId, CancellationToken ct)
    {
        var statements = await _statementService.GetStatementsAsync(participantId, ct);
        return Ok(statements);
    }

    [HttpPost("api/participants/{participantId:guid}/statements")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<StatementDto>> GenerateStatement(Guid participantId, GenerateStatementRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _statementService.GenerateStatementAsync(participantId, request, ct);
            return CreatedAtAction(nameof(GetStatements), new { participantId }, result);
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

    [HttpGet("api/statements/{id:guid}/pdf")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken ct)
    {
        try
        {
            var pdf = await _statementService.GeneratePdfAsync(id, ct);
            return File(pdf, "application/pdf", $"Statement_{id}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("api/statements/{id:guid}/send")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<StatementDto>> SendStatement(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _statementService.SendStatementAsync(id, ct);
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

    [HttpPost("api/admin/statements/batch")]
    [Authorize(Policy = "CanWriteParticipants")]
    public async Task<ActionResult<IReadOnlyList<StatementDto>>> GenerateBatch(CancellationToken ct)
    {
        try
        {
            var results = await _statementService.GenerateBatchAsync(ct);
            return Ok(results);
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
