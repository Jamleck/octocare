using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class InvoicesControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Creates an HttpRequestMessage with the finance user header (required for CanWriteFinance).
    /// </summary>
    private static HttpRequestMessage FinanceRequest(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Dev-User", "finance");
        if (body is not null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        return request;
    }

    /// <summary>
    /// Creates an HttpRequestMessage with the finance user header for GET (CanReadFinance).
    /// The admin user also has CanReadFinance but we use finance for consistency.
    /// </summary>
    private static HttpRequestMessage FinanceGet(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Dev-User", "finance");
        return request;
    }

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
    public async Task GetPaged_ReturnsOk_WithSeededInvoices()
    {
        // Act — use finance user for CanReadFinance
        var response = await Client.SendAsync(FinanceGet("/api/invoices?page=1&pageSize=20"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<InvoicePagedResult>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 3, $"Expected at least 3 invoices, got {result.TotalCount}");
        Assert.True(result.Items.Count >= 3);
    }

    [Fact]
    public async Task GetPaged_FilterByStatus_ReturnsFilteredResults()
    {
        // Act
        var response = await Client.SendAsync(FinanceGet("/api/invoices?page=1&pageSize=20&status=submitted"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<InvoicePagedResult>(JsonOptions);
        Assert.NotNull(result);
        Assert.All(result.Items, i => Assert.Equal("submitted", i.Status));
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var (participantId, providerId, planId) = await GetSeededIdsAsync();

        var requestBody = new CreateInvoiceRequest(
            ProviderId: providerId,
            ParticipantId: participantId,
            PlanId: planId,
            InvoiceNumber: "INV-TEST-001",
            ServicePeriodStart: new DateOnly(2025, 10, 1),
            ServicePeriodEnd: new DateOnly(2025, 10, 31),
            Notes: "Test invoice",
            LineItems: new List<CreateInvoiceLineItemRequest>
            {
                new("01_002_0107_1_1", "Assistance with Self-Care", new DateOnly(2025, 10, 7), 2m, 84.45m, null)
            });

        // Act — use finance user for CanWriteFinance
        var response = await Client.SendAsync(FinanceRequest(HttpMethod.Post, "/api/invoices", requestBody));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        Assert.NotNull(invoice);
        Assert.Equal("INV-TEST-001", invoice.InvoiceNumber);
        Assert.Equal("submitted", invoice.Status);
        Assert.Single(invoice.LineItems);
        Assert.True(invoice.TotalAmount > 0);
    }

    [Fact]
    public async Task Create_DuplicateInvoiceNumber_ReturnsConflict()
    {
        var (participantId, providerId, planId) = await GetSeededIdsAsync();

        var requestBody = new CreateInvoiceRequest(
            ProviderId: providerId,
            ParticipantId: participantId,
            PlanId: planId,
            InvoiceNumber: "INV-DUP-001",
            ServicePeriodStart: new DateOnly(2025, 10, 1),
            ServicePeriodEnd: new DateOnly(2025, 10, 31),
            Notes: null,
            LineItems: new List<CreateInvoiceLineItemRequest>
            {
                new("01_002_0107_1_1", "Desc", new DateOnly(2025, 10, 7), 1m, 50.00m, null)
            });

        // Create the first one
        await Client.SendAsync(FinanceRequest(HttpMethod.Post, "/api/invoices", requestBody));

        // Act — try to create a duplicate
        var response = await Client.SendAsync(FinanceRequest(HttpMethod.Post, "/api/invoices", requestBody));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Approve_SubmittedInvoice_ReturnsOk()
    {
        var (participantId, providerId, planId) = await GetSeededIdsAsync();

        // Create an invoice
        var createBody = new CreateInvoiceRequest(
            ProviderId: providerId,
            ParticipantId: participantId,
            PlanId: planId,
            InvoiceNumber: "INV-APPROVE-001",
            ServicePeriodStart: new DateOnly(2025, 10, 1),
            ServicePeriodEnd: new DateOnly(2025, 10, 31),
            Notes: null,
            LineItems: new List<CreateInvoiceLineItemRequest>
            {
                new("01_002_0107_1_1", "Assistance", new DateOnly(2025, 10, 7), 1m, 84.45m, null)
            });

        var createResponse = await Client.SendAsync(FinanceRequest(HttpMethod.Post, "/api/invoices", createBody));
        var created = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        Assert.NotNull(created);

        // Act
        var approveResponse = await Client.SendAsync(FinanceRequest(HttpMethod.Post, $"/api/invoices/{created.Id}/approve"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

        var approved = await approveResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        Assert.NotNull(approved);
        Assert.Equal("approved", approved.Status);
    }

    [Fact]
    public async Task Reject_SubmittedInvoice_ReturnsOk()
    {
        var (participantId, providerId, planId) = await GetSeededIdsAsync();

        // Create an invoice
        var createBody = new CreateInvoiceRequest(
            ProviderId: providerId,
            ParticipantId: participantId,
            PlanId: planId,
            InvoiceNumber: "INV-REJECT-001",
            ServicePeriodStart: new DateOnly(2025, 10, 1),
            ServicePeriodEnd: new DateOnly(2025, 10, 31),
            Notes: null,
            LineItems: new List<CreateInvoiceLineItemRequest>
            {
                new("01_002_0107_1_1", "Assistance", new DateOnly(2025, 10, 7), 1m, 84.45m, null)
            });

        var createResponse = await Client.SendAsync(FinanceRequest(HttpMethod.Post, "/api/invoices", createBody));
        var created = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        Assert.NotNull(created);

        var rejectBody = new RejectInvoiceRequest("Incorrect line items");

        // Act
        var rejectResponse = await Client.SendAsync(FinanceRequest(HttpMethod.Post, $"/api/invoices/{created.Id}/reject", rejectBody));

        // Assert
        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

        var rejected = await rejectResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        Assert.NotNull(rejected);
        Assert.Equal("rejected", rejected.Status);
        Assert.Equal("Incorrect line items", rejected.Notes);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Act — use finance user for CanReadFinance
        var response = await Client.SendAsync(FinanceGet($"/api/invoices/{Guid.NewGuid()}"));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithNoLineItems_ReturnsValidationError()
    {
        var (participantId, providerId, planId) = await GetSeededIdsAsync();

        var requestBody = new CreateInvoiceRequest(
            ProviderId: providerId,
            ParticipantId: participantId,
            PlanId: planId,
            InvoiceNumber: "INV-EMPTY-001",
            ServicePeriodStart: new DateOnly(2025, 10, 1),
            ServicePeriodEnd: new DateOnly(2025, 10, 31),
            Notes: null,
            LineItems: new List<CreateInvoiceLineItemRequest>());

        // Act — use finance user for CanWriteFinance
        var response = await Client.SendAsync(FinanceRequest(HttpMethod.Post, "/api/invoices", requestBody));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
