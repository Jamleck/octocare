using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class ClaimsControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static HttpRequestMessage FinanceRequest(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Dev-User", "finance");
        if (body is not null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        return request;
    }

    private static HttpRequestMessage FinanceGet(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Dev-User", "finance");
        return request;
    }

    [Fact]
    public async Task GetPaged_ReturnsOk_WithSeededClaims()
    {
        var response = await Client.SendAsync(FinanceGet("/api/claims?page=1&pageSize=20"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ClaimPagedResult>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1, $"Expected at least 1 claim, got {result.TotalCount}");
    }

    [Fact]
    public async Task CreateClaim_WithApprovedInvoiceLineItems_ReturnsCreated()
    {
        // Get approved invoices to find line item IDs
        var invoicesResponse = await Client.SendAsync(FinanceGet("/api/invoices?page=1&pageSize=20&status=approved"));
        var invoices = await invoicesResponse.Content.ReadFromJsonAsync<InvoicePagedResult>(JsonOptions);
        Assert.NotNull(invoices);
        Assert.True(invoices.Items.Count > 0, "Need at least one approved invoice");

        var lineItemIds = invoices.Items[0].LineItems.Select(li => li.Id).ToList();

        var request = new CreateClaimRequest(lineItemIds);
        var response = await Client.SendAsync(FinanceRequest(HttpMethod.Post, "/api/claims", request));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var claim = await response.Content.ReadFromJsonAsync<ClaimDto>(JsonOptions);
        Assert.NotNull(claim);
        Assert.StartsWith("CLM-", claim.BatchNumber);
        Assert.Equal("draft", claim.Status);
        Assert.Equal(lineItemIds.Count, claim.LineItems.Count);
    }

    [Fact]
    public async Task GetCsvDownload_ReturnsValidCsvFile()
    {
        // Get the seeded claim
        var claimsResponse = await Client.SendAsync(FinanceGet("/api/claims?page=1&pageSize=20"));
        var claims = await claimsResponse.Content.ReadFromJsonAsync<ClaimPagedResult>(JsonOptions);
        Assert.NotNull(claims);
        Assert.True(claims.Items.Count > 0, "Need at least one claim");

        var claimId = claims.Items[0].Id;

        var response = await Client.SendAsync(FinanceGet($"/api/claims/{claimId}/csv"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csvContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("RegistrationNumber", csvContent);
        Assert.Contains("NDISNumber", csvContent);
        Assert.Contains("SupportItemNumber", csvContent);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.SendAsync(FinanceGet($"/api/claims/{Guid.NewGuid()}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SubmitClaim_FromDraft_ReturnsOk()
    {
        // Create a new claim first
        var invoicesResponse = await Client.SendAsync(FinanceGet("/api/invoices?page=1&pageSize=20&status=approved"));
        var invoices = await invoicesResponse.Content.ReadFromJsonAsync<InvoicePagedResult>(JsonOptions);
        Assert.NotNull(invoices);
        Assert.True(invoices.Items.Count > 0);

        var lineItemIds = invoices.Items[0].LineItems.Select(li => li.Id).ToList();
        var createResponse = await Client.SendAsync(FinanceRequest(HttpMethod.Post, "/api/claims",
            new CreateClaimRequest(lineItemIds)));
        var created = await createResponse.Content.ReadFromJsonAsync<ClaimDto>(JsonOptions);
        Assert.NotNull(created);

        // Submit it
        var submitResponse = await Client.SendAsync(FinanceRequest(HttpMethod.Post, $"/api/claims/{created.Id}/submit"));

        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        var submitted = await submitResponse.Content.ReadFromJsonAsync<ClaimDto>(JsonOptions);
        Assert.NotNull(submitted);
        Assert.Equal("submitted", submitted.Status);
        Assert.NotNull(submitted.SubmissionDate);
    }
}
