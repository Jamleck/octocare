using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class ServiceAgreementsControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<(Guid ParticipantId, Guid ProviderId, Guid PlanId)> GetSeededIdsAsync()
    {
        // Get participant (Sarah Johnson)
        var participantsResponse = await Client.GetAsync("/api/participants?search=431234567");
        var participants = await participantsResponse.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        var participantId = participants!.Items[0].Id;

        // Get plans for this participant to find an active plan
        var plansResponse = await Client.GetAsync($"/api/participants/{participantId}/plans");
        var plans = await plansResponse.Content.ReadFromJsonAsync<List<PlanDto>>(JsonOptions);
        var activePlan = plans!.First(p => p.Status == "active");

        // Get providers
        var providersResponse = await Client.GetAsync("/api/providers");
        var providers = await providersResponse.Content.ReadFromJsonAsync<PagedResult<ProviderDto>>(JsonOptions);
        var providerId = providers!.Items[0].Id;

        return (participantId, providerId, activePlan.Id);
    }

    [Fact]
    public async Task GetByParticipant_ReturnsOk_WithSeededAgreements()
    {
        var (participantId, _, _) = await GetSeededIdsAsync();

        // Act
        var response = await Client.GetAsync($"/api/participants/{participantId}/agreements");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var agreements = await response.Content.ReadFromJsonAsync<List<ServiceAgreementDto>>(JsonOptions);
        Assert.NotNull(agreements);
        Assert.True(agreements.Count >= 2, "Seeder creates 2 service agreements for Sarah Johnson");
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var (participantId, providerId, planId) = await GetSeededIdsAsync();

        var request = new CreateServiceAgreementRequest(
            ProviderId: providerId,
            PlanId: planId,
            StartDate: new DateOnly(2025, 8, 1),
            EndDate: new DateOnly(2026, 1, 31),
            Items: new List<CreateServiceAgreementItemRequest>
            {
                new("01_002_0107_1_1", 85.00m, "weekly")
            });

        // Act
        var response = await Client.PostAsJsonAsync($"/api/participants/{participantId}/agreements", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var agreement = await response.Content.ReadFromJsonAsync<ServiceAgreementDto>(JsonOptions);
        Assert.NotNull(agreement);
        Assert.Equal("draft", agreement.Status);
        Assert.Single(agreement.Items);
        Assert.Equal(85.00m, agreement.Items[0].AgreedRate);
    }

    [Fact]
    public async Task Activate_DraftAgreement_ReturnsOk()
    {
        var (participantId, providerId, planId) = await GetSeededIdsAsync();

        // Create a draft agreement
        var createRequest = new CreateServiceAgreementRequest(
            ProviderId: providerId,
            PlanId: planId,
            StartDate: new DateOnly(2025, 9, 1),
            EndDate: new DateOnly(2026, 2, 28),
            Items: new List<CreateServiceAgreementItemRequest>
            {
                new("01_015_0107_1_1", 82.14m, null)
            });

        var createResponse = await Client.PostAsJsonAsync($"/api/participants/{participantId}/agreements", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceAgreementDto>(JsonOptions);
        Assert.NotNull(created);

        // Act
        var activateResponse = await Client.PostAsync($"/api/agreements/{created.Id}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);

        var activated = await activateResponse.Content.ReadFromJsonAsync<ServiceAgreementDto>(JsonOptions);
        Assert.NotNull(activated);
        Assert.Equal("active", activated.Status);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/agreements/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithNoItems_ReturnsValidationError()
    {
        var (participantId, providerId, planId) = await GetSeededIdsAsync();

        var request = new CreateServiceAgreementRequest(
            ProviderId: providerId,
            PlanId: planId,
            StartDate: new DateOnly(2025, 8, 1),
            EndDate: new DateOnly(2026, 1, 31),
            Items: new List<CreateServiceAgreementItemRequest>());

        // Act
        var response = await Client.PostAsJsonAsync($"/api/participants/{participantId}/agreements", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
