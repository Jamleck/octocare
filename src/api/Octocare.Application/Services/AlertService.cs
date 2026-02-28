using System.Text.Json;
using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Application.Services;

public class AlertService
{
    private readonly IAlertRepository _alertRepo;
    private readonly IPlanRepository _planRepo;
    private readonly IBudgetProjectionRepository _projectionRepo;
    private readonly ITenantContext _tenantContext;

    public AlertService(
        IAlertRepository alertRepo,
        IPlanRepository planRepo,
        IBudgetProjectionRepository projectionRepo,
        ITenantContext tenantContext)
    {
        _alertRepo = alertRepo;
        _planRepo = planRepo;
        _projectionRepo = projectionRepo;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<AlertDto>> GetAlertsAsync(Guid? planId, CancellationToken ct)
    {
        IReadOnlyList<BudgetAlert> alerts;

        if (planId.HasValue)
            alerts = await _alertRepo.GetByPlanIdAsync(planId.Value, ct);
        else
            alerts = await _alertRepo.GetAllAsync(includeRead: true, includeDismissed: false, ct);

        return alerts.Select(MapToDto).ToList();
    }

    public async Task<AlertSummaryDto> GetSummaryAsync(CancellationToken ct)
    {
        var (info, warning, critical) = await _alertRepo.GetUnreadCountsBySeverityAsync(ct);
        return new AlertSummaryDto(
            Total: info + warning + critical,
            UnreadInfo: info,
            UnreadWarning: warning,
            UnreadCritical: critical);
    }

    public async Task<AlertDto?> MarkReadAsync(Guid id, CancellationToken ct)
    {
        var alert = await _alertRepo.GetByIdAsync(id, ct);
        if (alert is null) return null;

        alert.MarkRead();
        await _alertRepo.SaveAsync(ct);
        return MapToDto(alert);
    }

    public async Task<AlertDto?> DismissAsync(Guid id, CancellationToken ct)
    {
        var alert = await _alertRepo.GetByIdAsync(id, ct);
        if (alert is null) return null;

        alert.Dismiss();
        await _alertRepo.SaveAsync(ct);
        return MapToDto(alert);
    }

    public async Task<IReadOnlyList<AlertDto>> GenerateAlertsForPlanAsync(Guid planId, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(planId, ct);
        if (plan is null) return [];

        // Clear existing non-dismissed alerts for this plan to regenerate
        await _alertRepo.DeleteByPlanIdAsync(planId, ct);

        var newAlerts = new List<BudgetAlert>();

        // Check budget thresholds per category
        var projections = await _projectionRepo.GetByPlanIdAsync(planId, ct);
        foreach (var bc in plan.BudgetCategories)
        {
            var projection = projections.FirstOrDefault(p => p.BudgetCategoryId == bc.Id);
            if (projection is null || bc.AllocatedAmount <= 0) continue;

            var utilisationPct = (decimal)(projection.CommittedAmount + projection.SpentAmount) / bc.AllocatedAmount * 100;

            if (utilisationPct >= 90)
            {
                newAlerts.Add(BudgetAlert.Create(
                    tenantId, planId, bc.Id,
                    AlertType.BudgetThreshold90,
                    AlertSeverity.Critical,
                    $"Budget category {bc.SupportCategory} ({bc.SupportPurpose}) has reached {utilisationPct:F1}% utilisation.",
                    JsonSerializer.Serialize(new { utilisationPct = Math.Round(utilisationPct, 1) })));
            }
            else if (utilisationPct >= 75)
            {
                newAlerts.Add(BudgetAlert.Create(
                    tenantId, planId, bc.Id,
                    AlertType.BudgetThreshold75,
                    AlertSeverity.Warning,
                    $"Budget category {bc.SupportCategory} ({bc.SupportPurpose}) has reached {utilisationPct:F1}% utilisation.",
                    JsonSerializer.Serialize(new { utilisationPct = Math.Round(utilisationPct, 1) })));
            }

            // Projected overspend: committed + spent + pending > allocated
            var totalUsage = projection.CommittedAmount + projection.SpentAmount + projection.PendingAmount;
            if (totalUsage > bc.AllocatedAmount)
            {
                newAlerts.Add(BudgetAlert.Create(
                    tenantId, planId, bc.Id,
                    AlertType.ProjectedOverspend,
                    AlertSeverity.Critical,
                    $"Budget category {bc.SupportCategory} ({bc.SupportPurpose}) is projected to exceed allocation. Total committed/spent/pending exceeds budget.",
                    JsonSerializer.Serialize(new { allocatedCents = bc.AllocatedAmount, totalUsageCents = totalUsage })));
            }
        }

        // Check plan expiry
        if (plan.Status == PlanStatus.Active || plan.Status == PlanStatus.Expiring)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var daysUntilExpiry = plan.EndDate.DayNumber - today.DayNumber;

            if (daysUntilExpiry <= 30 && daysUntilExpiry > 0)
            {
                newAlerts.Add(BudgetAlert.Create(
                    tenantId, planId, null,
                    AlertType.PlanExpiry30Days,
                    AlertSeverity.Critical,
                    $"Plan {plan.PlanNumber} expires in {daysUntilExpiry} days ({plan.EndDate:d MMM yyyy}).",
                    JsonSerializer.Serialize(new { daysUntilExpiry, endDate = plan.EndDate.ToString("yyyy-MM-dd") })));
            }
            else if (daysUntilExpiry <= 60 && daysUntilExpiry > 30)
            {
                newAlerts.Add(BudgetAlert.Create(
                    tenantId, planId, null,
                    AlertType.PlanExpiry60Days,
                    AlertSeverity.Warning,
                    $"Plan {plan.PlanNumber} expires in {daysUntilExpiry} days ({plan.EndDate:d MMM yyyy}).",
                    JsonSerializer.Serialize(new { daysUntilExpiry, endDate = plan.EndDate.ToString("yyyy-MM-dd") })));
            }
            else if (daysUntilExpiry <= 90 && daysUntilExpiry > 60)
            {
                newAlerts.Add(BudgetAlert.Create(
                    tenantId, planId, null,
                    AlertType.PlanExpiry90Days,
                    AlertSeverity.Info,
                    $"Plan {plan.PlanNumber} expires in {daysUntilExpiry} days ({plan.EndDate:d MMM yyyy}).",
                    JsonSerializer.Serialize(new { daysUntilExpiry, endDate = plan.EndDate.ToString("yyyy-MM-dd") })));
            }

            // Check for underspend if plan is more than halfway through
            var totalDays = plan.EndDate.DayNumber - plan.StartDate.DayNumber;
            var elapsedDays = today.DayNumber - plan.StartDate.DayNumber;
            if (totalDays > 0 && elapsedDays > totalDays / 2)
            {
                var expectedUtilisation = (decimal)elapsedDays / totalDays * 100;
                foreach (var bc in plan.BudgetCategories)
                {
                    var projection = projections.FirstOrDefault(p => p.BudgetCategoryId == bc.Id);
                    if (projection is null || bc.AllocatedAmount <= 0) continue;

                    var actualUtilisation = (decimal)(projection.CommittedAmount + projection.SpentAmount) / bc.AllocatedAmount * 100;

                    // If actual utilisation is less than half of the expected proportional utilisation
                    if (actualUtilisation < expectedUtilisation * 0.5m)
                    {
                        newAlerts.Add(BudgetAlert.Create(
                            tenantId, planId, bc.Id,
                            AlertType.ProjectedUnderspend,
                            AlertSeverity.Info,
                            $"Budget category {bc.SupportCategory} ({bc.SupportPurpose}) may be significantly underutilised ({actualUtilisation:F1}% used, expected ~{expectedUtilisation:F1}% at this point in plan).",
                            JsonSerializer.Serialize(new
                            {
                                actualUtilisation = Math.Round(actualUtilisation, 1),
                                expectedUtilisation = Math.Round(expectedUtilisation, 1)
                            })));
                    }
                }
            }
        }

        if (newAlerts.Count > 0)
            await _alertRepo.AddRangeAsync(newAlerts, ct);

        return newAlerts.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<AlertDto>> GenerateAlertsForAllActivePlansAsync(CancellationToken ct)
    {
        var plans = await _planRepo.GetActivePlansAsync(ct);
        var allAlerts = new List<AlertDto>();

        foreach (var plan in plans)
        {
            var alerts = await GenerateAlertsForPlanAsync(plan.Id, ct);
            allAlerts.AddRange(alerts);
        }

        return allAlerts;
    }

    private static AlertDto MapToDto(BudgetAlert alert)
    {
        return new AlertDto(
            alert.Id,
            alert.PlanId,
            alert.BudgetCategoryId,
            alert.AlertType.ToString(),
            alert.Severity.ToString(),
            alert.Message,
            alert.IsRead,
            alert.IsDismissed,
            alert.CreatedAt,
            alert.ReadAt,
            alert.Data);
    }
}
