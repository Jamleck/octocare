using Octocare.Application.DTOs;
using Octocare.Domain.Validation;

namespace Octocare.Application.Validators;

public static class OrganisationValidation
{
    public static (bool IsValid, Dictionary<string, string[]> Errors) Validate(UpdateOrganisationRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["Name"] = ["Organisation name is required."];

        if (request.Abn is not null && !AbnValidator.IsValid(request.Abn))
            errors["Abn"] = ["ABN is invalid. Must be 11 digits with a valid checksum."];

        if (request.ContactEmail is not null && !IsValidEmail(request.ContactEmail))
            errors["ContactEmail"] = ["Invalid email address."];

        return (errors.Count == 0, errors);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
