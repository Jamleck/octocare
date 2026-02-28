using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Domain;

public class PlanTransitionTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var oldPlanId = Guid.NewGuid();
        var checklistJson = "[{\"label\":\"Review budget\",\"completed\":false}]";

        var transition = PlanTransition.Create(tenantId, oldPlanId, checklistJson);

        Assert.NotEqual(Guid.Empty, transition.Id);
        Assert.Equal(tenantId, transition.TenantId);
        Assert.Equal(oldPlanId, transition.OldPlanId);
        Assert.Null(transition.NewPlanId);
        Assert.Equal(PlanTransitionStatus.Pending, transition.Status);
        Assert.Equal(checklistJson, transition.ChecklistItems);
        Assert.Null(transition.Notes);
        Assert.Null(transition.CompletedAt);
    }

    [Fact]
    public void UpdateChecklist_TransitionsToPending_ToInProgress()
    {
        var transition = PlanTransition.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            "[{\"label\":\"Review budget\",\"completed\":false}]");

        transition.UpdateChecklist("[{\"label\":\"Review budget\",\"completed\":true}]");

        Assert.Equal(PlanTransitionStatus.InProgress, transition.Status);
        Assert.Contains("true", transition.ChecklistItems);
    }

    [Fact]
    public void UpdateNotes_SetsNotes()
    {
        var transition = PlanTransition.Create(
            Guid.NewGuid(), Guid.NewGuid(), "[]");

        transition.UpdateNotes("Participant prefers to keep current provider.");

        Assert.Equal("Participant prefers to keep current provider.", transition.Notes);
    }

    [Fact]
    public void SetNewPlan_SetsNewPlanId()
    {
        var transition = PlanTransition.Create(
            Guid.NewGuid(), Guid.NewGuid(), "[]");
        var newPlanId = Guid.NewGuid();

        transition.SetNewPlan(newPlanId);

        Assert.Equal(newPlanId, transition.NewPlanId);
    }

    [Fact]
    public void Complete_TransitionsToCompleted()
    {
        var transition = PlanTransition.Create(
            Guid.NewGuid(), Guid.NewGuid(), "[]");

        transition.Complete();

        Assert.Equal(PlanTransitionStatus.Completed, transition.Status);
        Assert.NotNull(transition.CompletedAt);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_Throws()
    {
        var transition = PlanTransition.Create(
            Guid.NewGuid(), Guid.NewGuid(), "[]");
        transition.Complete();

        Assert.Throws<InvalidOperationException>(() => transition.Complete());
    }
}
