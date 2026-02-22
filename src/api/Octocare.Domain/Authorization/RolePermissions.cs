using Octocare.Domain.Enums;

namespace Octocare.Domain.Authorization;

public static class RolePermissions
{
    private static readonly Dictionary<string, string[]> Mapping = new()
    {
        [OrgRole.OrgAdmin] =
        [
            Permission.OrganisationRead, Permission.OrganisationManage,
            Permission.MembersRead, Permission.MembersManage,
            Permission.ParticipantsRead, Permission.ParticipantsWrite,
            Permission.FinanceRead
        ],
        [OrgRole.PlanManager] =
        [
            Permission.OrganisationRead,
            Permission.MembersRead,
            Permission.ParticipantsRead, Permission.ParticipantsWrite
        ],
        [OrgRole.Finance] =
        [
            Permission.OrganisationRead,
            Permission.MembersRead,
            Permission.ParticipantsRead,
            Permission.FinanceRead, Permission.FinanceWrite
        ]
    };

    public static bool HasPermission(string role, string permission)
        => Mapping.TryGetValue(role, out var perms) && perms.Contains(permission);

    public static string[] GetPermissions(string role)
        => Mapping.TryGetValue(role, out var perms) ? perms : [];
}
