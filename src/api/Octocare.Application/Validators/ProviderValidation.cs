using Octocare.Application.DTOs;
using Octocare.Domain.Validation;

namespace Octocare.Application.Validators;

public static class ProviderValidation
{
    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateCreate(CreateProviderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateCommon(request.Name, request.Abn, request.ContactEmail, errors);

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateUpdate(UpdateProviderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateCommon(request.Name, request.Abn, request.ContactEmail, errors);

        return (errors.Count == 0, errors);
    }

    private static void ValidateCommon(string name, string? abn, string? contactEmail,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(name))
            errors["Name"] = ["Provider name is required."];

        if (abn is not null && !AbnValidator.IsValid(abn))
            errors["Abn"] = ["Invalid ABN. Must be 11 digits with valid checksum."];

        if (contactEmail is not null)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(contactEmail);
                if (addr.Address != contactEmail)
                    errors["ContactEmail"] = ["Invalid email address."];
            }
            catch
            {
                errors["ContactEmail"] = ["Invalid email address."];
            }
        }
    }
}
