using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class ParticipantsControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetAll_ReturnsOk_WithSeededParticipants()
    {
        // Act
        var response = await Client.GetAsync("/api/participants");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 5, "Seeder creates 5 participants");
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        // Arrange - use a unique NDIS number that doesn't conflict with seeded data
        var request = new CreateParticipantRequest(
            NdisNumber: "436789012",
            FirstName: "Test",
            LastName: "Participant",
            DateOfBirth: new DateOnly(1990, 5, 15),
            Email: "test@example.com",
            Phone: "0400111222",
            Address: "1 Test St, Sydney NSW 2000",
            NomineeName: null,
            NomineeEmail: null,
            NomineePhone: null,
            NomineeRelationship: null);

        // Act
        var response = await Client.PostAsJsonAsync("/api/participants", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ParticipantDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal("436789012", result.NdisNumber);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("Participant", result.LastName);
        Assert.True(result.IsActive);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Create_WithInvalidNdisNumber_ReturnsBadRequest()
    {
        // Arrange - NDIS numbers must be 9 digits starting with 43
        var request = new CreateParticipantRequest(
            NdisNumber: "123456789",
            FirstName: "Test",
            LastName: "Participant",
            DateOfBirth: new DateOnly(1990, 5, 15),
            Email: "test@example.com",
            Phone: null,
            Address: null,
            NomineeName: null,
            NomineeEmail: null,
            NomineePhone: null,
            NomineeRelationship: null);

        // Act
        var response = await Client.PostAsJsonAsync("/api/participants", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/participants/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
