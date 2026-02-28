using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class MembersControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetMembers_ReturnsOk_WithSeededMembers()
    {
        // Act
        var response = await Client.GetAsync("/api/organisations/current/members");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var members = await response.Content.ReadFromJsonAsync<List<MemberDto>>(JsonOptions);
        Assert.NotNull(members);
        Assert.True(members.Count >= 3, "Seeder creates 3 members (admin, pm, finance)");
    }
}
