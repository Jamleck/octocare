using System.Text;
using Octocare.Application.DTOs;
using Octocare.Application.Services;

namespace Octocare.Tests.Unit;

public class CsvExportServiceTests
{
    private readonly CsvExportService _service = new();

    [Fact]
    public void GenerateCsv_WithData_ReturnsNonEmptyByteArray()
    {
        var data = new[]
        {
            new ParticipantSummaryRow("John Doe", "430000001", true, "PLAN-001", new DateOnly(2026, 12, 31), 50000m, 25000m, 50.0m),
            new ParticipantSummaryRow("Jane Smith", "430000002", true, "PLAN-002", new DateOnly(2026, 6, 30), 30000m, 10000m, 33.3m),
        };

        var result = _service.GenerateCsv(data);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateCsv_WithData_ContainsHeaders()
    {
        var data = new[]
        {
            new BudgetUtilisationReportRow("John Doe", "430000001", "PLAN-001", "Core", "Daily Activities", 10000m, 5000m, 5000m, 50.0m),
        };

        var result = _service.GenerateCsv(data);
        var content = Encoding.UTF8.GetString(result);

        Assert.Contains("ParticipantName", content);
        Assert.Contains("NdisNumber", content);
        Assert.Contains("PlanNumber", content);
        Assert.Contains("Category", content);
        Assert.Contains("Allocated", content);
        Assert.Contains("Spent", content);
        Assert.Contains("Available", content);
        Assert.Contains("UtilisationPercent", content);
    }

    [Fact]
    public void GenerateCsv_WithData_ContainsDataValues()
    {
        var data = new[]
        {
            new BudgetUtilisationReportRow("John Doe", "430000001", "PLAN-001", "Core", "Daily Activities", 10000m, 5000m, 5000m, 50.0m),
        };

        var result = _service.GenerateCsv(data);
        var content = Encoding.UTF8.GetString(result);

        Assert.Contains("John Doe", content);
        Assert.Contains("430000001", content);
        Assert.Contains("PLAN-001", content);
    }

    [Fact]
    public void GenerateCsv_WithEmptyCollection_ReturnsHeadersOnly()
    {
        var data = Array.Empty<BudgetUtilisationReportRow>();

        var result = _service.GenerateCsv(data);
        var content = Encoding.UTF8.GetString(result);

        Assert.Contains("ParticipantName", content);
        // Should only have headers, no data lines
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines);
    }

    [Fact]
    public void GenerateCsv_MultipleRows_HasCorrectLineCount()
    {
        var data = new[]
        {
            new ClaimStatusRow("CLM-001", "submitted", 1000m, 3, 0, 0, new DateOnly(2026, 1, 15)),
            new ClaimStatusRow("CLM-002", "accepted", 2000m, 5, 5, 0, new DateOnly(2026, 1, 20)),
            new ClaimStatusRow("CLM-003", "rejected", 500m, 2, 0, 2, new DateOnly(2026, 1, 25)),
        };

        var result = _service.GenerateCsv(data);
        var content = Encoding.UTF8.GetString(result);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // 1 header + 3 data rows
        Assert.Equal(4, lines.Length);
    }
}
