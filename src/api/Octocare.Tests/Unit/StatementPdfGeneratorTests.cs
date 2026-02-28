using Octocare.Application.DTOs;
using Octocare.Application.Services;
using QuestPDF.Infrastructure;

namespace Octocare.Tests.Unit;

public class StatementPdfGeneratorTests
{
    private readonly StatementPdfGenerator _generator;

    public StatementPdfGeneratorTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _generator = new StatementPdfGenerator();
    }

    private static StatementData CreateSampleData() => new(
        OrganisationName: "Test Organisation",
        OrganisationEmail: "info@test.org",
        OrganisationPhone: "02 1234 5678",
        ParticipantName: "Sarah Johnson",
        NdisNumber: "431234567",
        PlanNumber: "NDIS-2025-001",
        PlanStart: new DateOnly(2025, 7, 1),
        PlanEnd: new DateOnly(2026, 6, 30),
        PeriodStart: new DateOnly(2025, 10, 1),
        PeriodEnd: new DateOnly(2025, 10, 31),
        BudgetLines: new List<StatementBudgetLine>
        {
            new("Core", "DailyActivities", 45000.00m, 5000.00m, 40000.00m, 11.1m),
            new("CapacityBuilding", "IncreasedSocialAndCommunityParticipation", 15000.00m, 2000.00m, 13000.00m, 13.3m),
            new("Capital", "AssistiveTechnology", 8000.00m, 950.00m, 7050.00m, 11.9m),
        },
        RecentInvoices: new List<StatementInvoiceLine>
        {
            new(new DateOnly(2025, 10, 15), "Therapy Solutions", "Invoice INV-001", 1500.00m, "approved"),
            new(new DateOnly(2025, 10, 20), "Care Plus", "Invoice INV-002", 2200.00m, "paid"),
        },
        TotalAllocated: 68000.00m,
        TotalSpent: 7950.00m,
        TotalAvailable: 60050.00m);

    [Fact]
    public void Generate_ReturnsNonEmptyByteArray()
    {
        var data = CreateSampleData();

        var result = _generator.Generate(data);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Generate_ReturnsPdfBytes()
    {
        var data = CreateSampleData();

        var result = _generator.Generate(data);

        // PDF files start with %PDF
        Assert.True(result.Length > 4);
        Assert.Equal((byte)'%', result[0]);
        Assert.Equal((byte)'P', result[1]);
        Assert.Equal((byte)'D', result[2]);
        Assert.Equal((byte)'F', result[3]);
    }

    [Fact]
    public void Generate_WithEmptyBudgetLines_ReturnsValidPdf()
    {
        var data = CreateSampleData() with
        {
            BudgetLines = new List<StatementBudgetLine>(),
            TotalAllocated = 0,
            TotalSpent = 0,
            TotalAvailable = 0
        };

        var result = _generator.Generate(data);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Generate_WithEmptyInvoices_ReturnsValidPdf()
    {
        var data = CreateSampleData() with
        {
            RecentInvoices = new List<StatementInvoiceLine>()
        };

        var result = _generator.Generate(data);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Generate_WithNullOptionalFields_ReturnsValidPdf()
    {
        var data = CreateSampleData() with
        {
            OrganisationEmail = null,
            OrganisationPhone = null
        };

        var result = _generator.Generate(data);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
