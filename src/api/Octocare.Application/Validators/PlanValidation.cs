using Octocare.Application.DTOs;

namespace Octocare.Application.Validators;

public static class PlanValidation
{
    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateCreate(CreatePlanRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.PlanNumber))
            errors["PlanNumber"] = ["Plan number is required."];

        ValidateDates(request.StartDate, request.EndDate, errors);

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateUpdate(UpdatePlanRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.PlanNumber))
            errors["PlanNumber"] = ["Plan number is required."];

        ValidateDates(request.StartDate, request.EndDate, errors);

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateBudgetCategory(CreateBudgetCategoryRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.AllocatedAmount <= 0)
            errors["AllocatedAmount"] = ["Allocated amount must be greater than zero."];

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateBudgetCategoryUpdate(UpdateBudgetCategoryRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.AllocatedAmount <= 0)
            errors["AllocatedAmount"] = ["Allocated amount must be greater than zero."];

        return (errors.Count == 0, errors);
    }

    private static void ValidateDates(DateOnly startDate, DateOnly endDate, Dictionary<string, string[]> errors)
    {
        if (startDate >= endDate)
            errors["EndDate"] = ["End date must be after start date."];

        if (endDate <= DateOnly.FromDateTime(DateTime.Today))
            errors["EndDate"] = ["End date must be in the future."];
    }
}
