using Octocare.Application.DTOs;

namespace Octocare.Application.Validators;

public static class PriceGuideValidation
{
    public static (bool IsValid, Dictionary<string, string[]> Errors) ValidateImport(ImportPriceGuideRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["Name"] = ["Name is required."];

        if (request.EffectiveFrom >= request.EffectiveTo)
            errors["EffectiveTo"] = ["Effective to date must be after effective from date."];

        if (request.Items is null || request.Items.Count == 0)
            errors["Items"] = ["At least one support item is required."];
        else
        {
            var itemNumbers = new HashSet<string>();
            for (var i = 0; i < request.Items.Count; i++)
            {
                var item = request.Items[i];
                var prefix = $"Items[{i}]";

                if (string.IsNullOrWhiteSpace(item.ItemNumber))
                    errors[$"{prefix}.ItemNumber"] = ["Item number is required."];
                else if (!itemNumbers.Add(item.ItemNumber))
                    errors[$"{prefix}.ItemNumber"] = [$"Duplicate item number: {item.ItemNumber}."];

                if (string.IsNullOrWhiteSpace(item.Name))
                    errors[$"{prefix}.Name"] = ["Item name is required."];

                if (item.PriceLimitNational < 0)
                    errors[$"{prefix}.PriceLimitNational"] = ["Price limit cannot be negative."];

                if (item.PriceLimitRemote < 0)
                    errors[$"{prefix}.PriceLimitRemote"] = ["Price limit cannot be negative."];

                if (item.PriceLimitVeryRemote < 0)
                    errors[$"{prefix}.PriceLimitVeryRemote"] = ["Price limit cannot be negative."];
            }
        }

        return (errors.Count == 0, errors);
    }
}
