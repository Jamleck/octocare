using Microsoft.Extensions.Logging;
using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;

namespace Octocare.Infrastructure.External;

/// <summary>
/// Mock implementation of the PRODA/PACE client for MVP.
/// Returns realistic data for known NDIS numbers from the dev seeder.
/// Real integration requires PRODA registration, which is a separate process.
/// </summary>
public class MockProdaPaceClient : IProdaPaceClient
{
    private readonly ILogger<MockProdaPaceClient> _logger;

    // Mock data keyed by NDIS number, matching dev seeder participants
    private static readonly Dictionary<string, MockParticipantData> MockData = new()
    {
        ["431234567"] = new MockParticipantData(
            new ProdaParticipantInfo("431234567", "Sarah", "Johnson",
                new DateOnly(1985, 3, 15), "0412345678", "sarah.j@email.com"),
            new ProdaPlanInfo("NDIS-2025-001", "active",
                new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30), 68000.00m),
            new ProdaBudgetInfo("NDIS-2025-001", new List<ProdaBudgetLine>
            {
                new("Core", "DailyActivities", 45000.00m, 1353.26m, 43646.74m),
                new("CapacityBuilding", "IncreasedSocialAndCommunityParticipation", 15000.00m, 2186.64m, 12813.36m),
                new("Capital", "AssistiveTechnology", 8000.00m, 950.00m, 7050.00m),
            })),
        ["432345678"] = new MockParticipantData(
            new ProdaParticipantInfo("432345678", "Michael", "Chen",
                new DateOnly(1992, 7, 22), "0423456789", "m.chen@email.com"),
            new ProdaPlanInfo("NDIS-2025-002", "active",
                new DateOnly(2025, 4, 1), new DateOnly(2026, 3, 31), 52000.00m),
            new ProdaBudgetInfo("NDIS-2025-002", new List<ProdaBudgetLine>
            {
                new("Core", "DailyActivities", 35000.00m, 5200.00m, 29800.00m),
                new("CapacityBuilding", "FindingAndKeepingAJob", 12000.00m, 0.00m, 12000.00m),
                new("Capital", "AssistiveTechnology", 5000.00m, 0.00m, 5000.00m),
            })),
        ["433456789"] = new MockParticipantData(
            new ProdaParticipantInfo("433456789", "Emily", "Williams",
                new DateOnly(1978, 11, 3), "0434567890", "emily.w@email.com"),
            new ProdaPlanInfo("NDIS-2025-003", "active",
                new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31), 41000.00m),
            new ProdaBudgetInfo("NDIS-2025-003", new List<ProdaBudgetLine>
            {
                new("Core", "DailyActivities", 28000.00m, 8400.00m, 19600.00m),
                new("CapacityBuilding", "ImprovedDailyLivingSkills", 8000.00m, 1200.00m, 6800.00m),
                new("Capital", "HomeModifications", 5000.00m, 0.00m, 5000.00m),
            })),
        ["434567890"] = new MockParticipantData(
            new ProdaParticipantInfo("434567890", "James", "Brown",
                new DateOnly(2001, 1, 30), "0445678901", "james.b@email.com"),
            new ProdaPlanInfo("NDIS-2025-004", "active",
                new DateOnly(2025, 10, 1), new DateOnly(2026, 9, 30), 37500.00m),
            new ProdaBudgetInfo("NDIS-2025-004", new List<ProdaBudgetLine>
            {
                new("Core", "DailyActivities", 22000.00m, 0.00m, 22000.00m),
                new("CapacityBuilding", "IncreasedSocialAndCommunityParticipation", 10500.00m, 0.00m, 10500.00m),
                new("Capital", "AssistiveTechnology", 5000.00m, 0.00m, 5000.00m),
            })),
        ["435678901"] = new MockParticipantData(
            new ProdaParticipantInfo("435678901", "Olivia", "Taylor",
                new DateOnly(1995, 9, 8), "0456789012", "olivia.t@email.com"),
            new ProdaPlanInfo("NDIS-2025-005", "active",
                new DateOnly(2025, 5, 1), new DateOnly(2026, 4, 30), 55000.00m),
            new ProdaBudgetInfo("NDIS-2025-005", new List<ProdaBudgetLine>
            {
                new("Core", "DailyActivities", 30000.00m, 3500.00m, 26500.00m),
                new("CapacityBuilding", "CoordinationOfSupports", 15000.00m, 2000.00m, 13000.00m),
                new("Capital", "AssistiveTechnology", 10000.00m, 0.00m, 10000.00m),
            })),
    };

    public MockProdaPaceClient(ILogger<MockProdaPaceClient> logger)
    {
        _logger = logger;
    }

    public async Task<ProdaPlanInfo?> GetPlanInfoAsync(string ndisNumber, CancellationToken ct = default)
    {
        _logger.LogWarning("Using MOCK PRODA/PACE client — not connected to real PRODA. NDIS Number: {NdisNumber}", ndisNumber);
        await SimulateNetworkLatencyAsync(ct);

        return MockData.TryGetValue(ndisNumber, out var data) ? data.PlanInfo : null;
    }

    public async Task<ProdaBudgetInfo?> GetBudgetInfoAsync(string ndisNumber, string planId, CancellationToken ct = default)
    {
        _logger.LogWarning("Using MOCK PRODA/PACE client — not connected to real PRODA. NDIS Number: {NdisNumber}, PlanId: {PlanId}", ndisNumber, planId);
        await SimulateNetworkLatencyAsync(ct);

        if (!MockData.TryGetValue(ndisNumber, out var data))
            return null;

        // Only return budget if the plan ID matches the mock plan number
        return data.BudgetInfo.PlanNumber == planId ? data.BudgetInfo : null;
    }

    public async Task<ProdaParticipantInfo?> GetParticipantInfoAsync(string ndisNumber, CancellationToken ct = default)
    {
        _logger.LogWarning("Using MOCK PRODA/PACE client — not connected to real PRODA. NDIS Number: {NdisNumber}", ndisNumber);
        await SimulateNetworkLatencyAsync(ct);

        return MockData.TryGetValue(ndisNumber, out var data) ? data.ParticipantInfo : null;
    }

    public async Task<bool> VerifyPlanAsync(string ndisNumber, string planNumber, CancellationToken ct = default)
    {
        _logger.LogWarning("Using MOCK PRODA/PACE client — not connected to real PRODA. NDIS Number: {NdisNumber}, Plan: {PlanNumber}", ndisNumber, planNumber);
        await SimulateNetworkLatencyAsync(ct);

        return MockData.TryGetValue(ndisNumber, out var data) && data.PlanInfo.PlanNumber == planNumber;
    }

    private static async Task SimulateNetworkLatencyAsync(CancellationToken ct)
    {
        var delay = Random.Shared.Next(100, 301);
        await Task.Delay(delay, ct);
    }

    private record MockParticipantData(
        ProdaParticipantInfo ParticipantInfo,
        ProdaPlanInfo PlanInfo,
        ProdaBudgetInfo BudgetInfo);
}
