using Octocare.Domain.Authorization;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Authorization;

public class RolePermissionsTests
{
    [Fact]
    public void OrgAdmin_HasAllExpectedPermissions()
    {
        Assert.True(RolePermissions.HasPermission(OrgRole.OrgAdmin, Permission.OrganisationRead));
        Assert.True(RolePermissions.HasPermission(OrgRole.OrgAdmin, Permission.OrganisationManage));
        Assert.True(RolePermissions.HasPermission(OrgRole.OrgAdmin, Permission.MembersRead));
        Assert.True(RolePermissions.HasPermission(OrgRole.OrgAdmin, Permission.MembersManage));
        Assert.True(RolePermissions.HasPermission(OrgRole.OrgAdmin, Permission.ParticipantsRead));
        Assert.True(RolePermissions.HasPermission(OrgRole.OrgAdmin, Permission.ParticipantsWrite));
    }

    [Fact]
    public void PlanManager_CannotManageOrg()
    {
        Assert.True(RolePermissions.HasPermission(OrgRole.PlanManager, Permission.OrganisationRead));
        Assert.False(RolePermissions.HasPermission(OrgRole.PlanManager, Permission.OrganisationManage));
        Assert.False(RolePermissions.HasPermission(OrgRole.PlanManager, Permission.MembersManage));
    }

    [Fact]
    public void PlanManager_CanManageParticipants()
    {
        Assert.True(RolePermissions.HasPermission(OrgRole.PlanManager, Permission.ParticipantsRead));
        Assert.True(RolePermissions.HasPermission(OrgRole.PlanManager, Permission.ParticipantsWrite));
    }

    [Fact]
    public void Finance_HasFinancePermissions()
    {
        Assert.True(RolePermissions.HasPermission(OrgRole.Finance, Permission.FinanceRead));
        Assert.True(RolePermissions.HasPermission(OrgRole.Finance, Permission.FinanceWrite));
    }

    [Fact]
    public void Finance_CannotWriteParticipants()
    {
        Assert.True(RolePermissions.HasPermission(OrgRole.Finance, Permission.ParticipantsRead));
        Assert.False(RolePermissions.HasPermission(OrgRole.Finance, Permission.ParticipantsWrite));
    }

    [Fact]
    public void InvalidRole_HasNoPermissions()
    {
        Assert.False(RolePermissions.HasPermission("invalid_role", Permission.OrganisationRead));
        Assert.Empty(RolePermissions.GetPermissions("invalid_role"));
    }
}
