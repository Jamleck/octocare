namespace Octocare.Domain.Validation;

/// <summary>
/// Validates NDIS participant numbers.
/// NDIS numbers are 9 digits starting with 43.
/// </summary>
public static class NdisNumberValidator
{
    public static bool IsValid(string? ndisNumber)
    {
        if (string.IsNullOrWhiteSpace(ndisNumber))
            return false;

        return ndisNumber.Length == 9
            && ndisNumber.All(char.IsDigit)
            && ndisNumber.StartsWith("43");
    }
}
