namespace Octocare.Domain.Validation;

/// <summary>
/// Validates Australian Business Numbers (ABN).
/// An ABN is 11 digits with a weighted checksum.
/// </summary>
public static class AbnValidator
{
    private static readonly int[] Weights = [10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19];

    public static bool IsValid(string? abn)
    {
        if (string.IsNullOrWhiteSpace(abn))
            return false;

        // Remove spaces
        var cleaned = abn.Replace(" ", "");

        if (cleaned.Length != 11 || !cleaned.All(char.IsDigit))
            return false;

        var digits = cleaned.Select(c => c - '0').ToArray();

        // Subtract 1 from the first digit
        digits[0] -= 1;

        // Calculate weighted sum
        var sum = 0;
        for (var i = 0; i < 11; i++)
            sum += digits[i] * Weights[i];

        return sum % 89 == 0;
    }
}
