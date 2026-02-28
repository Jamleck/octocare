using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class PlanTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var plan = Plan.Create(tenantId, participantId, "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        Assert.NotEqual(Guid.Empty, plan.Id);
        Assert.Equal(tenantId, plan.TenantId);
        Assert.Equal(participantId, plan.ParticipantId);
        Assert.Equal("NDIS-2025-001", plan.PlanNumber);
        Assert.Equal(new DateOnly(2025, 7, 1), plan.StartDate);
        Assert.Equal(new DateOnly(2026, 6, 30), plan.EndDate);
        Assert.Equal(PlanStatus.Draft, plan.Status);
        Assert.True(plan.IsActive);
    }

    [Fact]
    public void Activate_FromDraft_TransitionsToActive()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        plan.Activate();

        Assert.Equal(PlanStatus.Active, plan.Status);
        Assert.True(plan.IsActive);
    }

    [Fact]
    public void Activate_FromNonDraft_Throws()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan.Activate();

        Assert.Throws<InvalidOperationException>(() => plan.Activate());
    }

    [Fact]
    public void MarkExpiring_FromActive_TransitionsToExpiring()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan.Activate();

        plan.MarkExpiring();

        Assert.Equal(PlanStatus.Expiring, plan.Status);
        Assert.True(plan.IsActive);
    }

    [Fact]
    public void MarkExpiring_FromDraft_Throws()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        Assert.Throws<InvalidOperationException>(() => plan.MarkExpiring());
    }

    [Fact]
    public void Expire_FromActive_TransitionsToExpired()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan.Activate();

        plan.Expire();

        Assert.Equal(PlanStatus.Expired, plan.Status);
        Assert.False(plan.IsActive);
    }

    [Fact]
    public void Expire_FromExpiring_TransitionsToExpired()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan.Activate();
        plan.MarkExpiring();

        plan.Expire();

        Assert.Equal(PlanStatus.Expired, plan.Status);
        Assert.False(plan.IsActive);
    }

    [Fact]
    public void Expire_FromDraft_Throws()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        Assert.Throws<InvalidOperationException>(() => plan.Expire());
    }

    [Fact]
    public void Transition_FromActive_TransitionsToTransitioned()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan.Activate();

        plan.Transition();

        Assert.Equal(PlanStatus.Transitioned, plan.Status);
        Assert.False(plan.IsActive);
    }

    [Fact]
    public void Transition_FromExpiring_TransitionsToTransitioned()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan.Activate();
        plan.MarkExpiring();

        plan.Transition();

        Assert.Equal(PlanStatus.Transitioned, plan.Status);
        Assert.False(plan.IsActive);
    }

    [Fact]
    public void Transition_FromDraft_Throws()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        Assert.Throws<InvalidOperationException>(() => plan.Transition());
    }

    [Fact]
    public void Update_InDraftStatus_ModifiesFields()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        plan.Update("NDIS-2025-002", new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31));

        Assert.Equal("NDIS-2025-002", plan.PlanNumber);
        Assert.Equal(new DateOnly(2025, 8, 1), plan.StartDate);
        Assert.Equal(new DateOnly(2026, 7, 31), plan.EndDate);
    }

    [Fact]
    public void Update_InActiveStatus_Throws()
    {
        var plan = Plan.Create(Guid.NewGuid(), Guid.NewGuid(), "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        plan.Activate();

        Assert.Throws<InvalidOperationException>(() =>
            plan.Update("NDIS-2025-002", new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31)));
    }
}
