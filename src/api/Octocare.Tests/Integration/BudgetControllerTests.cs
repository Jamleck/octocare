using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class BudgetControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Creates an HttpRequestMessage with the finance user header for GET (CanReadFinance).
    /// </summary>
    private static HttpRequestMessage FinanceGet(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Dev-User", "finance");
        return request;
    }

    private async Task<Guid> GetActivePlanIdAsync()
    {
        // Get participant (Sarah Johnson)
        var participantsResponse = await Client.GetAsync("/api/participants?search=431234567");
        var participants = await participantsResponse.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        var participantId = participants!.Items[0].Id;

        // Get plans for this participant to find an active plan
        var plansResponse = await Client.GetAsync($"/api/participants/{participantId}/plans");
        var plans = await plansResponse.Content.ReadFromJsonAsync<List<PlanDto>>(JsonOptions);
        return plans!.First(p => p.Status == "active").Id;
    }

    [Fact]
    public async Task GetBudgetOverview_ReturnsOk_WithProjectionData()
    {
        var planId = await GetActivePlanIdAsync();

        // Act
        var response = await Client.SendAsync(FinanceGet($"/api/plans/{planId}/budget-overview"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var overview = await response.Content.ReadFromJsonAsync<BudgetOverviewDto>(JsonOptions);
        Assert.NotNull(overview);
        Assert.Equal(planId, overview.PlanId);
        Assert.True(overview.TotalAllocated > 0, "Total allocated should be > 0");
        Assert.True(overview.Categories.Count >= 3, $"Expected at least 3 categories, got {overview.Categories.Count}");

        // Verify each category has realistic data
        foreach (var cat in overview.Categories)
        {
            Assert.True(cat.Allocated > 0, $"Category {cat.SupportCategory} should have allocation > 0");
            Assert.True(cat.UtilisationPercentage >= 0, "Utilisation should be >= 0");
        }
    }

    [Fact]
    public async Task GetBudgetOverview_WithNonExistentPlan_ReturnsNotFound()
    {
        // Act
        var response = await Client.SendAsync(FinanceGet($"/api/plans/{Guid.NewGuid()}/budget-overview"));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBudgetOverview_HasCorrectUtilisationPercentage()
    {
        var planId = await GetActivePlanIdAsync();

        // Act
        var response = await Client.SendAsync(FinanceGet($"/api/plans/{planId}/budget-overview"));
        var overview = await response.Content.ReadFromJsonAsync<BudgetOverviewDto>(JsonOptions);

        Assert.NotNull(overview);
        Assert.True(overview.UtilisationPercentage >= 0, "Utilisation should be >= 0");
        Assert.True(overview.UtilisationPercentage <= 100, "Utilisation should be <= 100 for seeded data");

        // Verify the total available = total allocated - total committed - total spent
        var expectedAvailable = overview.TotalAllocated - overview.TotalCommitted - overview.TotalSpent;
        Assert.Equal(expectedAvailable, overview.TotalAvailable);
    }
}
