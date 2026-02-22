using Octocare.Domain.Validation;

namespace Octocare.Tests.Domain;

public class AbnValidatorTests
{
    [Theory]
    [InlineData("51824753556", true)]  // Valid ABN
    [InlineData("53004085616", true)]  // Valid ABN (Telstra)
    [InlineData("12345678901", false)] // Invalid checksum
    [InlineData("1234567890", false)]  // Too short
    [InlineData("123456789012", false)] // Too long
    [InlineData("abcdefghijk", false)] // Non-numeric
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("51 824 753 556", true)] // With spaces (should still validate)
    public void IsValid_ReturnsExpected(string? abn, bool expected)
    {
        Assert.Equal(expected, AbnValidator.IsValid(abn));
    }
}
