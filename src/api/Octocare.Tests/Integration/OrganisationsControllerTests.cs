using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class OrganisationsControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetCurrent_ReturnsOk_WithSeededOrganisation()
    {
        // Act
        var response = await Client.GetAsync("/api/organisations/current");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var org = await response.Content.ReadFromJsonAsync<OrganisationDto>(JsonOptions);
        Assert.NotNull(org);
        Assert.Equal("Acme Plan Management", org.Name);
        Assert.Equal("51824753556", org.Abn);
        Assert.True(org.IsActive);
    }

    [Fact]
    public async Task UpdateCurrent_WithValidData_ReturnsOk()
    {
        // Arrange
        var request = new UpdateOrganisationRequest(
            Name: "Updated Org Name",
            Abn: "51824753556",
            ContactEmail: "updated@example.com",
            ContactPhone: "0400999888",
            Address: "99 New St, Sydney NSW 2000");

        // Act
        var response = await Client.PutAsJsonAsync("/api/organisations/current", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var org = await response.Content.ReadFromJsonAsync<OrganisationDto>(JsonOptions);
        Assert.NotNull(org);
        Assert.Equal("Updated Org Name", org.Name);
        Assert.Equal("updated@example.com", org.ContactEmail);
    }
}
