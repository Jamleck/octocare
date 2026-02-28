using Octocare.Application.DTOs;

namespace Octocare.Application.Validators;

public static class InvoiceValidation
{
    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateCreate(CreateInvoiceRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
            errors["InvoiceNumber"] = ["Invoice number is required."];

        if (request.ServicePeriodStart >= request.ServicePeriodEnd)
            errors["ServicePeriodEnd"] = ["Service period end must be after service period start."];

        if (request.LineItems is null || request.LineItems.Count == 0)
            errors["LineItems"] = ["At least one line item is required."];
        else
        {
            for (int i = 0; i < request.LineItems.Count; i++)
            {
                var item = request.LineItems[i];
                if (item.Quantity <= 0)
                    errors[$"LineItems[{i}].Quantity"] = ["Quantity must be greater than zero."];
                if (item.Rate <= 0)
                    errors[$"LineItems[{i}].Rate"] = ["Rate must be greater than zero."];
                if (string.IsNullOrWhiteSpace(item.SupportItemNumber))
                    errors[$"LineItems[{i}].SupportItemNumber"] = ["Support item number is required."];
                if (string.IsNullOrWhiteSpace(item.Description))
                    errors[$"LineItems[{i}].Description"] = ["Description is required."];
            }
        }

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateReject(RejectInvoiceRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Reason))
            errors["Reason"] = ["A reason is required when rejecting an invoice."];

        return (errors.Count == 0, errors);
    }

    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateDispute(DisputeInvoiceRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Reason))
            errors["Reason"] = ["A reason is required when disputing an invoice."];

        return (errors.Count == 0, errors);
    }
}
