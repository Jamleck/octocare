using Octocare.Domain.Validation;

namespace Octocare.Tests.Domain;

public class NdisNumberValidatorTests
{
    [Theory]
    [InlineData("431234567", true)]   // Valid
    [InlineData("430000000", true)]   // Valid (starts with 43)
    [InlineData("439999999", true)]   // Valid
    [InlineData("441234567", false)]  // Doesn't start with 43
    [InlineData("43123456", false)]   // Too short (8 digits)
    [InlineData("4312345678", false)] // Too long (10 digits)
    [InlineData("43abc4567", false)]  // Contains letters
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpected(string? ndisNumber, bool expected)
    {
        Assert.Equal(expected, NdisNumberValidator.IsValid(ndisNumber));
    }
}
