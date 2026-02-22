using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Domain;

public class UserOrgMembershipTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var membership = UserOrgMembership.Create(userId, orgId, OrgRole.PlanManager);

        Assert.NotEqual(Guid.Empty, membership.Id);
        Assert.Equal(userId, membership.UserId);
        Assert.Equal(orgId, membership.OrganisationId);
        Assert.Equal(orgId, membership.TenantId);
        Assert.Equal(OrgRole.PlanManager, membership.Role);
        Assert.True(membership.IsActive);
    }

    [Fact]
    public void Create_ThrowsForInvalidRole()
    {
        Assert.Throws<ArgumentException>(() =>
            UserOrgMembership.Create(Guid.NewGuid(), Guid.NewGuid(), "invalid_role"));
    }

    [Fact]
    public void UpdateRole_ChangesRole()
    {
        var membership = UserOrgMembership.Create(Guid.NewGuid(), Guid.NewGuid(), OrgRole.PlanManager);
        membership.UpdateRole(OrgRole.OrgAdmin);
        Assert.Equal(OrgRole.OrgAdmin, membership.Role);
    }

    [Fact]
    public void UpdateRole_ThrowsForInvalidRole()
    {
        var membership = UserOrgMembership.Create(Guid.NewGuid(), Guid.NewGuid(), OrgRole.PlanManager);
        Assert.Throws<ArgumentException>(() => membership.UpdateRole("invalid"));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var membership = UserOrgMembership.Create(Guid.NewGuid(), Guid.NewGuid(), OrgRole.Finance);
        membership.Deactivate();
        Assert.False(membership.IsActive);
    }
}
