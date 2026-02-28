using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Domain;

public class BudgetCategoryTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var planId = Guid.NewGuid();
        var category = BudgetCategory.Create(planId, SupportCategory.Core,
            SupportPurpose.DailyActivities, 4500000);

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(planId, category.PlanId);
        Assert.Equal(SupportCategory.Core, category.SupportCategory);
        Assert.Equal(SupportPurpose.DailyActivities, category.SupportPurpose);
        Assert.Equal(4500000, category.AllocatedAmount);
        Assert.True(category.IsActive);
    }

    [Fact]
    public void UpdateAllocation_ModifiesAmount()
    {
        var category = BudgetCategory.Create(Guid.NewGuid(), SupportCategory.Core,
            SupportPurpose.DailyActivities, 4500000);

        category.UpdateAllocation(5000000);

        Assert.Equal(5000000, category.AllocatedAmount);
    }

    [Fact]
    public void Create_WithDifferentCategories_SetsCorrectly()
    {
        var capitalCategory = BudgetCategory.Create(Guid.NewGuid(), SupportCategory.Capital,
            SupportPurpose.AssistiveTechnology, 800000);

        Assert.Equal(SupportCategory.Capital, capitalCategory.SupportCategory);
        Assert.Equal(SupportPurpose.AssistiveTechnology, capitalCategory.SupportPurpose);
    }
}
