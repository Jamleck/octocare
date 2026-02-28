using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Domain;

public class BudgetAlertTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var alert = BudgetAlert.Create(
            tenantId, planId, categoryId,
            AlertType.BudgetThreshold90,
            AlertSeverity.Critical,
            "Budget at 92% utilisation.",
            "{\"utilisationPct\":92.0}");

        Assert.NotEqual(Guid.Empty, alert.Id);
        Assert.Equal(tenantId, alert.TenantId);
        Assert.Equal(planId, alert.PlanId);
        Assert.Equal(categoryId, alert.BudgetCategoryId);
        Assert.Equal(AlertType.BudgetThreshold90, alert.AlertType);
        Assert.Equal(AlertSeverity.Critical, alert.Severity);
        Assert.Equal("Budget at 92% utilisation.", alert.Message);
        Assert.False(alert.IsRead);
        Assert.False(alert.IsDismissed);
        Assert.Null(alert.ReadAt);
        Assert.Equal("{\"utilisationPct\":92.0}", alert.Data);
    }

    [Fact]
    public void Create_WithNullBudgetCategoryId_Works()
    {
        var alert = BudgetAlert.Create(
            Guid.NewGuid(), Guid.NewGuid(), null,
            AlertType.PlanExpiry30Days,
            AlertSeverity.Critical,
            "Plan expires in 25 days.");

        Assert.Null(alert.BudgetCategoryId);
    }

    [Fact]
    public void MarkRead_SetsIsReadAndReadAt()
    {
        var alert = BudgetAlert.Create(
            Guid.NewGuid(), Guid.NewGuid(), null,
            AlertType.PlanExpiry60Days,
            AlertSeverity.Warning,
            "Plan expires in 55 days.");

        alert.MarkRead();

        Assert.True(alert.IsRead);
        Assert.NotNull(alert.ReadAt);
    }

    [Fact]
    public void Dismiss_SetsIsDismissed()
    {
        var alert = BudgetAlert.Create(
            Guid.NewGuid(), Guid.NewGuid(), null,
            AlertType.ProjectedUnderspend,
            AlertSeverity.Info,
            "Category underspend detected.");

        alert.Dismiss();

        Assert.True(alert.IsDismissed);
    }
}
