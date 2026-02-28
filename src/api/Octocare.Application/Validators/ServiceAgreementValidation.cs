using Octocare.Application.DTOs;

namespace Octocare.Application.Validators;

public static class ServiceAgreementValidation
{
    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateCreate(CreateServiceAgreementRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.ProviderId == Guid.Empty)
            errors["ProviderId"] = ["Provider is required."];

        if (request.PlanId == Guid.Empty)
            errors["PlanId"] = ["Plan is required."];

        if (request.StartDate >= request.EndDate)
            errors["EndDate"] = ["End date must be after start date."];

        if (request.Items == null || request.Items.Count == 0)
            errors["Items"] = ["At least one service item is required."];
        else
        {
            for (var i = 0; i < request.Items.Count; i++)
            {
                var item = request.Items[i];
                if (string.IsNullOrWhiteSpace(item.SupportItemNumber))
                    errors[$"Items[{i}].SupportItemNumber"] = ["Support item number is required."];
                if (item.AgreedRate <= 0)
                    errors[$"Items[{i}].AgreedRate"] = ["Agreed rate must be greater than zero."];
            }
        }

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateBooking(CreateServiceBookingRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.BudgetCategoryId == Guid.Empty)
            errors["BudgetCategoryId"] = ["Budget category is required."];

        if (request.AllocatedAmount <= 0)
            errors["AllocatedAmount"] = ["Allocated amount must be greater than zero."];

        return (errors.Count == 0, errors);
    }
}
