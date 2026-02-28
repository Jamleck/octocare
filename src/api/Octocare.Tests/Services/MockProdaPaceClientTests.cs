using Microsoft.Extensions.Logging;
using Octocare.Infrastructure.External;

namespace Octocare.Tests.Services;

public class MockProdaPaceClientTests
{
    private readonly MockProdaPaceClient _client;

    public MockProdaPaceClientTests()
    {
        var logger = LoggerFactory
            .Create(b => b.AddConsole())
            .CreateLogger<MockProdaPaceClient>();
        _client = new MockProdaPaceClient(logger);
    }

    [Theory]
    [InlineData("431234567", "Sarah", "Johnson")]
    [InlineData("432345678", "Michael", "Chen")]
    [InlineData("433456789", "Emily", "Williams")]
    [InlineData("434567890", "James", "Brown")]
    [InlineData("435678901", "Olivia", "Taylor")]
    public async Task GetParticipantInfoAsync_KnownNumber_ReturnsData(
        string ndisNumber, string expectedFirstName, string expectedLastName)
    {
        var result = await _client.GetParticipantInfoAsync(ndisNumber);

        Assert.NotNull(result);
        Assert.Equal(ndisNumber, result.NdisNumber);
        Assert.Equal(expectedFirstName, result.FirstName);
        Assert.Equal(expectedLastName, result.LastName);
    }

    [Fact]
    public async Task GetParticipantInfoAsync_UnknownNumber_ReturnsNull()
    {
        var result = await _client.GetParticipantInfoAsync("439999999");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlanInfoAsync_KnownNumber_ReturnsPlanInfo()
    {
        var result = await _client.GetPlanInfoAsync("431234567");

        Assert.NotNull(result);
        Assert.Equal("NDIS-2025-001", result.PlanNumber);
        Assert.Equal("active", result.Status);
        Assert.Equal(new DateOnly(2025, 7, 1), result.StartDate);
        Assert.Equal(new DateOnly(2026, 6, 30), result.EndDate);
        Assert.Equal(68000.00m, result.TotalBudget);
    }

    [Fact]
    public async Task GetPlanInfoAsync_UnknownNumber_ReturnsNull()
    {
        var result = await _client.GetPlanInfoAsync("439999999");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBudgetInfoAsync_KnownNumberAndPlan_ReturnsBudget()
    {
        var result = await _client.GetBudgetInfoAsync("431234567", "NDIS-2025-001");

        Assert.NotNull(result);
        Assert.Equal("NDIS-2025-001", result.PlanNumber);
        Assert.Equal(3, result.Categories.Count);
        Assert.Contains(result.Categories, c => c.Category == "Core");
        Assert.Contains(result.Categories, c => c.Category == "CapacityBuilding");
        Assert.Contains(result.Categories, c => c.Category == "Capital");
    }

    [Fact]
    public async Task GetBudgetInfoAsync_KnownNumberWrongPlan_ReturnsNull()
    {
        var result = await _client.GetBudgetInfoAsync("431234567", "NDIS-WRONG");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBudgetInfoAsync_UnknownNumber_ReturnsNull()
    {
        var result = await _client.GetBudgetInfoAsync("439999999", "NDIS-2025-001");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("431234567", "NDIS-2025-001", true)]
    [InlineData("431234567", "NDIS-WRONG", false)]
    [InlineData("439999999", "NDIS-2025-001", false)]
    public async Task VerifyPlanAsync_ReturnsExpectedResult(
        string ndisNumber, string planNumber, bool expected)
    {
        var result = await _client.VerifyPlanAsync(ndisNumber, planNumber);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetParticipantInfoAsync_SupportsCancellation()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _client.GetParticipantInfoAsync("431234567", cts.Token));
    }
}
