using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.Services;

namespace Octocare.Api.Controllers;

[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;
    private readonly CsvExportService _csvExportService;
    private readonly ExcelExportService _excelExportService;

    public ReportsController(
        ReportService reportService,
        CsvExportService csvExportService,
        ExcelExportService excelExportService)
    {
        _reportService = reportService;
        _csvExportService = csvExportService;
        _excelExportService = excelExportService;
    }

    [HttpGet("api/reports/budget-utilisation")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<IActionResult> BudgetUtilisation(
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var data = await _reportService.GetBudgetUtilisationAsync(ct);

        return format.ToLowerInvariant() switch
        {
            "csv" => File(_csvExportService.GenerateCsv(data), "text/csv", "budget-utilisation.csv"),
            "xlsx" => File(_excelExportService.GenerateExcel(data, "Budget Utilisation"),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "budget-utilisation.xlsx"),
            _ => Ok(data)
        };
    }

    [HttpGet("api/reports/outstanding-invoices")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<IActionResult> OutstandingInvoices(
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var data = await _reportService.GetOutstandingInvoicesAsync(ct);

        return format.ToLowerInvariant() switch
        {
            "csv" => File(_csvExportService.GenerateCsv(data), "text/csv", "outstanding-invoices.csv"),
            "xlsx" => File(_excelExportService.GenerateExcel(data, "Outstanding Invoices"),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "outstanding-invoices.xlsx"),
            _ => Ok(data)
        };
    }

    [HttpGet("api/reports/claim-status")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<IActionResult> ClaimStatus(
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var data = await _reportService.GetClaimStatusAsync(ct);

        return format.ToLowerInvariant() switch
        {
            "csv" => File(_csvExportService.GenerateCsv(data), "text/csv", "claim-status.csv"),
            "xlsx" => File(_excelExportService.GenerateExcel(data, "Claim Status"),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "claim-status.xlsx"),
            _ => Ok(data)
        };
    }

    [HttpGet("api/reports/participant-summary")]
    [Authorize(Policy = "CanReadParticipants")]
    public async Task<IActionResult> ParticipantSummary(
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var data = await _reportService.GetParticipantSummaryAsync(ct);

        return format.ToLowerInvariant() switch
        {
            "csv" => File(_csvExportService.GenerateCsv(data), "text/csv", "participant-summary.csv"),
            "xlsx" => File(_excelExportService.GenerateExcel(data, "Participant Summary"),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "participant-summary.xlsx"),
            _ => Ok(data)
        };
    }

    [HttpGet("api/reports/audit-trail")]
    [Authorize(Policy = "CanReadFinance")]
    public async Task<IActionResult> AuditTrail(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;

        var data = await _reportService.GetAuditTrailAsync(fromDate, toDate, ct);

        return format.ToLowerInvariant() switch
        {
            "csv" => File(_csvExportService.GenerateCsv(data), "text/csv", "audit-trail.csv"),
            "xlsx" => File(_excelExportService.GenerateExcel(data, "Audit Trail"),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "audit-trail.xlsx"),
            _ => Ok(data)
        };
    }
}
