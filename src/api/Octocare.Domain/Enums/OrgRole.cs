namespace Octocare.Domain.Enums;

public static class OrgRole
{
    public const string OrgAdmin = "org_admin";
    public const string PlanManager = "plan_manager";
    public const string Finance = "finance";

    public static readonly string[] All = [OrgAdmin, PlanManager, Finance];

    public static bool IsValid(string role) => All.Contains(role);
}
