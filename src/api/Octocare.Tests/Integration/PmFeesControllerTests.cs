using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Octocare.Application.DTOs;

namespace Octocare.Tests.Integration;

public class PmFeesControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Creates an HttpRequestMessage with the admin user header (required for CanManageOrg).
    /// Default auth user is admin, but we set it explicitly for clarity.
    /// </summary>
    private static HttpRequestMessage AdminRequest(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Dev-User", "admin");
        if (body is not null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        return request;
    }

    [Fact]
    public async Task GenerateMonthly_WithValidMonth_ReturnsOkWithInvoices()
    {
        // Arrange — the seeder creates Sarah Johnson with an active plan (2025-07-01 to 2026-06-30)
        var requestBody = new GenerateMonthlyFeesRequest(Month: 1, Year: 2026);

        // Act
        var response = await Client.SendAsync(AdminRequest(HttpMethod.Post, "/api/admin/pm-fees/generate-monthly", requestBody));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GenerateMonthlyFeesResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.InvoicesGenerated >= 1, $"Expected at least 1 invoice, got {result.InvoicesGenerated}");
        Assert.Equal(result.InvoicesGenerated, result.InvoiceIds.Count);
    }

    [Fact]
    public async Task GenerateMonthly_SecondRun_IsIdempotent()
    {
        // Arrange
        var requestBody = new GenerateMonthlyFeesRequest(Month: 2, Year: 2026);

        // Act — generate twice
        var response1 = await Client.SendAsync(AdminRequest(HttpMethod.Post, "/api/admin/pm-fees/generate-monthly", requestBody));
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var result1 = await response1.Content.ReadFromJsonAsync<GenerateMonthlyFeesResponse>(JsonOptions);
        Assert.NotNull(result1);
        var firstRunCount = result1.InvoicesGenerated;

        var response2 = await Client.SendAsync(AdminRequest(HttpMethod.Post, "/api/admin/pm-fees/generate-monthly", requestBody));
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var result2 = await response2.Content.ReadFromJsonAsync<GenerateMonthlyFeesResponse>(JsonOptions);
        Assert.NotNull(result2);

        // Assert — second run should generate 0 (duplicates skipped)
        Assert.Equal(0, result2.InvoicesGenerated);
        Assert.True(firstRunCount >= 1, "First run should have generated at least 1 invoice");
    }

    [Fact]
    public async Task GenerateMonthly_OutsidePlanPeriod_GeneratesNoInvoices()
    {
        // Arrange — plan is 2025-07-01 to 2026-06-30, so July 2027 is outside
        var requestBody = new GenerateMonthlyFeesRequest(Month: 7, Year: 2027);

        // Act
        var response = await Client.SendAsync(AdminRequest(HttpMethod.Post, "/api/admin/pm-fees/generate-monthly", requestBody));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GenerateMonthlyFeesResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(0, result.InvoicesGenerated);
    }

    [Fact]
    public async Task GenerateMonthly_InvalidMonth_ReturnsValidationError()
    {
        // Arrange
        var requestBody = new GenerateMonthlyFeesRequest(Month: 13, Year: 2026);

        // Act
        var response = await Client.SendAsync(AdminRequest(HttpMethod.Post, "/api/admin/pm-fees/generate-monthly", requestBody));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateSetup_WithValidParticipant_ReturnsOk()
    {
        // Arrange — get Sarah Johnson's ID (she has an active plan)
        var participantsResponse = await Client.GetAsync("/api/participants?search=431234567");
        var participants = await participantsResponse.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        var participantId = participants!.Items[0].Id;

        // Act
        var response = await Client.SendAsync(AdminRequest(HttpMethod.Post, $"/api/admin/pm-fees/generate-setup/{participantId}"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GenerateSetupFeeResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.InvoiceId);
    }

    [Fact]
    public async Task GenerateSetup_DuplicateForSameParticipant_ReturnsConflict()
    {
        // Arrange — get Sarah Johnson's ID
        var participantsResponse = await Client.GetAsync("/api/participants?search=431234567");
        var participants = await participantsResponse.Content.ReadFromJsonAsync<PagedResult<ParticipantDto>>(JsonOptions);
        var participantId = participants!.Items[0].Id;

        // Generate setup fee first time
        var response1 = await Client.SendAsync(AdminRequest(HttpMethod.Post, $"/api/admin/pm-fees/generate-setup/{participantId}"));

        // The first call might succeed or conflict (if the previous test already created one).
        // Generate setup fee second time
        var response2 = await Client.SendAsync(AdminRequest(HttpMethod.Post, $"/api/admin/pm-fees/generate-setup/{participantId}"));

        // Assert — second call should be conflict
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task GenerateSetup_NonExistentParticipant_ReturnsNotFound()
    {
        // Act
        var response = await Client.SendAsync(AdminRequest(HttpMethod.Post, $"/api/admin/pm-fees/generate-setup/{Guid.NewGuid()}"));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GenerateMonthly_CreatesInvoiceWithCorrectSource()
    {
        // Arrange
        var requestBody = new GenerateMonthlyFeesRequest(Month: 3, Year: 2026);

        // Act — generate monthly fees
        var response = await Client.SendAsync(AdminRequest(HttpMethod.Post, "/api/admin/pm-fees/generate-monthly", requestBody));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GenerateMonthlyFeesResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.True(result.InvoicesGenerated >= 1);

        // Verify the invoice has the correct source by fetching it
        var invoiceId = result.InvoiceIds[0];
        var invoiceRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/invoices/{invoiceId}");
        invoiceRequest.Headers.Add("X-Dev-User", "finance");
        var invoiceResponse = await Client.SendAsync(invoiceRequest);
        Assert.Equal(HttpStatusCode.OK, invoiceResponse.StatusCode);

        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions);
        Assert.NotNull(invoice);
        Assert.Equal("pm_fee_auto", invoice.Source);
        Assert.Equal("submitted", invoice.Status);
        Assert.Single(invoice.LineItems);
        Assert.Equal("15_037_0117_1_3", invoice.LineItems[0].SupportItemNumber);
        Assert.Equal(36.28m, invoice.TotalAmount);
    }
}
