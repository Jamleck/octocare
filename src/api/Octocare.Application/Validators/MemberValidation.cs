using Octocare.Application.DTOs;
using Octocare.Domain.Enums;

namespace Octocare.Application.Validators;

public static class MemberValidation
{
    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateInvite(InviteMemberRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors["Email"] = ["Email is required."];

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors["FirstName"] = ["First name is required."];

        if (string.IsNullOrWhiteSpace(request.LastName))
            errors["LastName"] = ["Last name is required."];

        if (!OrgRole.IsValid(request.Role))
            errors["Role"] = [$"Invalid role. Must be one of: {string.Join(", ", OrgRole.All)}."];

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateRoleUpdate(UpdateMemberRoleRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!OrgRole.IsValid(request.Role))
            errors["Role"] = [$"Invalid role. Must be one of: {string.Join(", ", OrgRole.All)}."];

        return (errors.Count == 0, errors);
    }
}
