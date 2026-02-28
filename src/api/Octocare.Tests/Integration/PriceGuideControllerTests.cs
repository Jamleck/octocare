using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Octocare.Application.DTOs;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Integration;

public class PriceGuideControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetVersions_ReturnsOk_WithSeededVersions()
    {
        // Act
        var response = await Client.GetAsync("/api/price-guide/versions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var versions = await response.Content.ReadFromJsonAsync<List<PriceGuideVersionDto>>(JsonOptions);
        Assert.NotNull(versions);
        Assert.NotEmpty(versions);
        Assert.Contains(versions, v => v.Name == "2025-26");
    }

    [Fact]
    public async Task GetCurrentVersion_ReturnsOk_WithCurrentVersion()
    {
        // Act
        var response = await Client.GetAsync("/api/price-guide/versions/current");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var version = await response.Content.ReadFromJsonAsync<PriceGuideVersionDto>(JsonOptions);
        Assert.NotNull(version);
        Assert.True(version.IsCurrent);
        Assert.Equal("2025-26", version.Name);
    }

    [Fact]
    public async Task GetItems_ReturnsOk_WithSeededItems()
    {
        // Arrange — get the current version first
        var versionResponse = await Client.GetAsync("/api/price-guide/versions/current");
        var version = await versionResponse.Content.ReadFromJsonAsync<PriceGuideVersionDto>(JsonOptions);
        Assert.NotNull(version);

        // Act
        var response = await Client.GetAsync($"/api/price-guide/versions/{version.Id}/items?page=1&pageSize=50");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<SupportItemDto>>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 10, "Seeder creates at least 10 support items");
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetItems_WithCategoryFilter_ReturnsFilteredItems()
    {
        // Arrange — get the current version first
        var versionResponse = await Client.GetAsync("/api/price-guide/versions/current");
        var version = await versionResponse.Content.ReadFromJsonAsync<PriceGuideVersionDto>(JsonOptions);
        Assert.NotNull(version);

        // Act — filter by Capital category
        var response = await Client.GetAsync(
            $"/api/price-guide/versions/{version.Id}/items?page=1&pageSize=50&category=Capital");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<SupportItemDto>>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.All(result.Items, item =>
            Assert.Equal(SupportCategory.Capital, item.SupportCategory));
    }

    [Fact]
    public async Task GetItems_WithSearchFilter_ReturnsMatchingItems()
    {
        // Arrange
        var versionResponse = await Client.GetAsync("/api/price-guide/versions/current");
        var version = await versionResponse.Content.ReadFromJsonAsync<PriceGuideVersionDto>(JsonOptions);
        Assert.NotNull(version);

        // Act — search by item name
        var response = await Client.GetAsync(
            $"/api/price-guide/versions/{version.Id}/items?page=1&pageSize=50&search=Plan+Management");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<SupportItemDto>>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 2, "Seeder creates at least 2 plan management items");
    }
}
