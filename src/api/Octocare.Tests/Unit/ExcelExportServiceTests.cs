using ClosedXML.Excel;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Tests.Unit;

public class ExcelExportServiceTests
{
    private readonly ExcelExportService _service = new();

    [Fact]
    public void GenerateExcel_WithData_ReturnsNonEmptyByteArray()
    {
        var data = new[]
        {
            new ParticipantSummaryRow("John Doe", "430000001", true, "PLAN-001", new DateOnly(2026, 12, 31), 50000m, 25000m, 50.0m),
        };

        var result = _service.GenerateExcel(data, "Test");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateExcel_WithData_ProducesValidWorkbook()
    {
        var data = new[]
        {
            new BudgetUtilisationReportRow("John Doe", "430000001", "PLAN-001", "Core", "Daily Activities", 10000m, 5000m, 5000m, 50.0m),
            new BudgetUtilisationReportRow("Jane Smith", "430000002", "PLAN-002", "Capital", "Assistive Tech", 20000m, 15000m, 5000m, 75.0m),
        };

        var result = _service.GenerateExcel(data, "Budget Report");

        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);

        Assert.Single(workbook.Worksheets);
        var worksheet = workbook.Worksheets.First();
        Assert.Equal("Budget Report", worksheet.Name);
    }

    [Fact]
    public void GenerateExcel_WithData_HasCorrectHeaderRow()
    {
        var data = new[]
        {
            new ClaimStatusRow("CLM-001", "submitted", 1000m, 3, 0, 0, new DateOnly(2026, 1, 15)),
        };

        var result = _service.GenerateExcel(data, "Claims");

        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Headers should be in row 1
        Assert.Equal("Batch Number", worksheet.Cell(1, 1).GetString());
        Assert.Equal("Status", worksheet.Cell(1, 2).GetString());
        Assert.Equal("Total Amount", worksheet.Cell(1, 3).GetString());

        // Header cells should be bold
        Assert.True(worksheet.Cell(1, 1).Style.Font.Bold);
    }

    [Fact]
    public void GenerateExcel_WithData_HasCorrectRowCount()
    {
        var data = new[]
        {
            new OutstandingInvoiceRow("INV-001", "Provider A", "John Doe", new DateOnly(2026, 1, 15), 1500m, "submitted", 30, "0-30 days"),
            new OutstandingInvoiceRow("INV-002", "Provider B", "Jane Smith", new DateOnly(2026, 1, 10), 2500m, "approved", 45, "31-60 days"),
        };

        var result = _service.GenerateExcel(data, "Invoices");

        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Row 1 = headers, Row 2-3 = data
        Assert.False(string.IsNullOrEmpty(worksheet.Cell(2, 1).GetString()));
        Assert.False(string.IsNullOrEmpty(worksheet.Cell(3, 1).GetString()));
    }

    [Fact]
    public void GenerateExcel_EmptyCollection_HasOnlyHeaders()
    {
        var data = Array.Empty<ParticipantSummaryRow>();

        var result = _service.GenerateExcel(data, "Empty");

        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Should still have header row
        Assert.Equal("Name", worksheet.Cell(1, 1).GetString());
        // Row 2 should be empty
        Assert.True(worksheet.Cell(2, 1).IsEmpty());
    }
}
