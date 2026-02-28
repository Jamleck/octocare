using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Octocare.Application.DTOs;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Integration;

public class PlansControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task GetByParticipant_ReturnsOk_WithSeededPlans()
    {
        // Get the participant that has seeded plans (Sarah Johnson, NDIS 431234567)
        var participantsResponse = await Client.GetAsync("/api/participants?search=431234567");
        var participants = await participantsResponse.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        Assert.NotNull(participants);
        Assert.NotEmpty(participants.Items);

        var participantId = participants.Items[0].Id;

        // Act
        var response = await Client.GetAsync($"/api/participants/{participantId}/plans");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var plans = await response.Content.ReadFromJsonAsync<List<PlanDto>>(JsonOptions);
        Assert.NotNull(plans);
        // Seeder creates 2 plans for Sarah Johnson
        Assert.True(plans.Count >= 2, "Seeder creates 2 plans for Sarah Johnson");
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        // Get a participant ID
        var participantsResponse = await Client.GetAsync("/api/participants");
        var participants = await participantsResponse.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        Assert.NotNull(participants);
        var participantId = participants.Items[0].Id;

        var request = new CreatePlanRequest(
            PlanNumber: "NDIS-TEST-001",
            StartDate: new DateOnly(2027, 1, 1),
            EndDate: new DateOnly(2027, 12, 31));

        // Act
        var response = await Client.PostAsJsonAsync($"/api/participants/{participantId}/plans", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var plan = await response.Content.ReadFromJsonAsync<PlanDto>(JsonOptions);
        Assert.NotNull(plan);
        Assert.Equal("NDIS-TEST-001", plan.PlanNumber);
        Assert.Equal("draft", plan.Status);
        Assert.NotEqual(Guid.Empty, plan.Id);
    }

    [Fact]
    public async Task Activate_DraftPlan_ReturnsOk()
    {
        // Create a draft plan first
        var participantsResponse = await Client.GetAsync("/api/participants");
        var participants = await participantsResponse.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        Assert.NotNull(participants);
        var participantId = participants.Items[0].Id;

        var createRequest = new CreatePlanRequest(
            PlanNumber: "NDIS-ACTIVATE-001",
            StartDate: new DateOnly(2027, 1, 1),
            EndDate: new DateOnly(2027, 12, 31));

        var createResponse = await Client.PostAsJsonAsync($"/api/participants/{participantId}/plans", createRequest);
        var createdPlan = await createResponse.Content.ReadFromJsonAsync<PlanDto>(JsonOptions);
        Assert.NotNull(createdPlan);

        // Act
        var activateResponse = await Client.PostAsync($"/api/plans/{createdPlan.Id}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);

        var activatedPlan = await activateResponse.Content.ReadFromJsonAsync<PlanDto>(JsonOptions);
        Assert.NotNull(activatedPlan);
        Assert.Equal("active", activatedPlan.Status);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/plans/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddBudgetCategory_ToDraftPlan_ReturnsCreated()
    {
        // Create a draft plan first
        var participantsResponse = await Client.GetAsync("/api/participants");
        var participants = await participantsResponse.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        Assert.NotNull(participants);
        var participantId = participants.Items[0].Id;

        var createRequest = new CreatePlanRequest(
            PlanNumber: "NDIS-BUDGET-001",
            StartDate: new DateOnly(2027, 1, 1),
            EndDate: new DateOnly(2027, 12, 31));

        var createResponse = await Client.PostAsJsonAsync($"/api/participants/{participantId}/plans", createRequest);
        var createdPlan = await createResponse.Content.ReadFromJsonAsync<PlanDto>(JsonOptions);
        Assert.NotNull(createdPlan);

        var budgetRequest = new CreateBudgetCategoryRequest(
            SupportCategory: SupportCategory.Core,
            SupportPurpose: SupportPurpose.DailyActivities,
            AllocatedAmount: 15000.00m);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/plans/{createdPlan.Id}/budget-categories", budgetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var category = await response.Content.ReadFromJsonAsync<BudgetCategoryDto>(JsonOptions);
        Assert.NotNull(category);
        Assert.Equal(15000.00m, category.AllocatedAmount);
    }
}
