using Octocare.Application.DTOs;
using Octocare.Domain.Validation;

namespace Octocare.Application.Validators;

public static class ParticipantValidation
{
    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateCreate(CreateParticipantRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!NdisNumberValidator.IsValid(request.NdisNumber))
            errors["NdisNumber"] = ["NDIS number must be 9 digits starting with 43."];

        ValidateCommon(request.FirstName, request.LastName, request.DateOfBirth, request.Email, errors);

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateUpdate(UpdateParticipantRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateCommon(request.FirstName, request.LastName, request.DateOfBirth, request.Email, errors);

        return (errors.Count == 0, errors);
    }

    private static void ValidateCommon(string firstName, string lastName, DateOnly dateOfBirth,
        string? email, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            errors["FirstName"] = ["First name is required."];

        if (string.IsNullOrWhiteSpace(lastName))
            errors["LastName"] = ["Last name is required."];

        if (dateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            errors["DateOfBirth"] = ["Date of birth cannot be in the future."];

        if (email is not null)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    errors["Email"] = ["Invalid email address."];
            }
            catch
            {
                errors["Email"] = ["Invalid email address."];
            }
        }
    }
}
