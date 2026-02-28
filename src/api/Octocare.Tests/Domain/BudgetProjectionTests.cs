using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class BudgetProjectionTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var categoryId = Guid.NewGuid();
        var projection = BudgetProjection.Create(categoryId, 4500000);

        Assert.NotEqual(Guid.Empty, projection.Id);
        Assert.Equal(categoryId, projection.BudgetCategoryId);
        Assert.Equal(4500000, projection.AllocatedAmount);
        Assert.Equal(0, projection.CommittedAmount);
        Assert.Equal(0, projection.SpentAmount);
        Assert.Equal(0, projection.PendingAmount);
        Assert.Equal(4500000, projection.AvailableAmount);
    }

    [Fact]
    public void AvailableAmount_IsComputed_FromAllocatedMinusCommittedMinusSpent()
    {
        var projection = BudgetProjection.Create(Guid.NewGuid(), 4500000);
        projection.UpdateFromEvent(4500000, 2000000, 500000, 100000);

        // Available = Allocated - Committed - Spent = 4500000 - 2000000 - 500000 = 2000000
        Assert.Equal(2000000, projection.AvailableAmount);
    }

    [Fact]
    public void UpdateFromEvent_UpdatesAllAmounts()
    {
        var projection = BudgetProjection.Create(Guid.NewGuid(), 4500000);

        projection.UpdateFromEvent(
            allocatedCents: 5000000,
            committedCents: 1500000,
            spentCents: 800000,
            pendingCents: 200000);

        Assert.Equal(5000000, projection.AllocatedAmount);
        Assert.Equal(1500000, projection.CommittedAmount);
        Assert.Equal(800000, projection.SpentAmount);
        Assert.Equal(200000, projection.PendingAmount);
        Assert.Equal(2700000, projection.AvailableAmount); // 5000000 - 1500000 - 800000
    }

    [Fact]
    public void AvailableAmount_CanBeNegative_WhenOverspent()
    {
        var projection = BudgetProjection.Create(Guid.NewGuid(), 100000);
        projection.UpdateFromEvent(100000, 60000, 50000, 10000);

        // Available = 100000 - 60000 - 50000 = -10000 (overspent)
        Assert.Equal(-10000, projection.AvailableAmount);
    }

    [Fact]
    public void Create_WithZeroAllocation_HasZeroAvailable()
    {
        var projection = BudgetProjection.Create(Guid.NewGuid(), 0);

        Assert.Equal(0, projection.AllocatedAmount);
        Assert.Equal(0, projection.AvailableAmount);
    }
}
