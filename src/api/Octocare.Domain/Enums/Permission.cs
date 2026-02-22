namespace Octocare.Domain.Enums;

public static class Permission
{
    public const string OrganisationRead = "org:read";
    public const string OrganisationManage = "org:manage";
    public const string MembersRead = "members:read";
    public const string MembersManage = "members:manage";
    public const string ParticipantsRead = "participants:read";
    public const string ParticipantsWrite = "participants:write";
    public const string FinanceRead = "finance:read";
    public const string FinanceWrite = "finance:write";
}
