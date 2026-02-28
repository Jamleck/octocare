using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class ProvidersControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAll_ReturnsOk_WithSeededProviders()
    {
        // Act
        var response = await Client.GetAsync("/api/providers");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProviderDto>>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 3, "Seeder creates 3 providers");
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateProviderRequest(
            Name: "Test Provider",
            Abn: null,
            ContactEmail: "test@provider.com",
            ContactPhone: "0400111222",
            Address: "1 Test St, Sydney NSW 2000");

        // Act
        var response = await Client.PostAsJsonAsync("/api/providers", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProviderDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("Test Provider", result.Name);
        Assert.Equal("test@provider.com", result.ContactEmail);
        Assert.True(result.IsActive);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateProviderRequest(
            Name: "",
            Abn: null,
            ContactEmail: null,
            ContactPhone: null,
            Address: null);

        // Act
        var response = await Client.PostAsJsonAsync("/api/providers", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/providers/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOk()
    {
        // Arrange — create a provider first
        var createRequest = new CreateProviderRequest(
            Name: "Provider to Update",
            Abn: null,
            ContactEmail: "original@provider.com",
            ContactPhone: null,
            Address: null);

        var createResponse = await Client.PostAsJsonAsync("/api/providers", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProviderDto>(JsonOptions);

        var updateRequest = new UpdateProviderRequest(
            Name: "Updated Provider",
            Abn: null,
            ContactEmail: "updated@provider.com",
            ContactPhone: "0400999888",
            Address: "New Address");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{created!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ProviderDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("Updated Provider", result.Name);
        Assert.Equal("updated@provider.com", result.ContactEmail);
    }
}
