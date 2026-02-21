using Octocare.Domain.ValueObjects;

namespace Octocare.Tests.Domain;

public class MoneyTests
{
    [Fact]
    public void FromDollars_ConvertsCorrectly()
    {
        var money = Money.FromDollars(1234.56m);
        Assert.Equal(123456, money.Cents);
    }

    [Fact]
    public void ToDollars_ConvertsCorrectly()
    {
        var money = new Money(123456);
        Assert.Equal(1234.56m, money.ToDollars());
    }

    [Fact]
    public void Addition_AddsTwoCents()
    {
        var a = Money.FromDollars(10.00m);
        var b = Money.FromDollars(5.50m);
        var result = a + b;
        Assert.Equal(1550, result.Cents);
    }

    [Fact]
    public void Subtraction_SubtractsCents()
    {
        var a = Money.FromDollars(10.00m);
        var b = Money.FromDollars(3.25m);
        var result = a - b;
        Assert.Equal(675, result.Cents);
    }

    [Fact]
    public void ApplyMultiplier_UsesBankersRounding()
    {
        // $65.47/hr x 1.175 TTP loading = 6547 * 1.175 = 7692.725 -> 7693 (banker's rounds .5 to even)
        var rate = Money.FromDollars(65.47m);
        var result = rate.ApplyMultiplier(1.175m);
        // 6547 * 1.175 = 7692.725 -> rounds to 7692 (banker's rounding: .725... wait)
        // Actually: 6547 * 1.175 = 7692.725. Banker's rounding of 7692.725 to nearest integer:
        // .725 > .5 so rounds up to 7693
        Assert.Equal(7693, result.Cents);
    }

    [Fact]
    public void ApplyMultiplier_BankersRounding_HalfToEven()
    {
        // Test exact .5 case: 100 * 1.005 = 100.5 -> rounds to 100 (even)
        var money = new Money(100);
        var result = money.ApplyMultiplier(1.005m);
        Assert.Equal(100, result.Cents); // Banker's rounding: .5 rounds to even (100)
    }

    [Fact]
    public void Zero_HasZeroCents()
    {
        Assert.Equal(0, Money.Zero.Cents);
    }

    [Fact]
    public void Comparison_WorksCorrectly()
    {
        var small = Money.FromDollars(10.00m);
        var large = Money.FromDollars(20.00m);

        Assert.True(small < large);
        Assert.True(large > small);
        Assert.True(small <= large);
        Assert.True(large >= small);
        var same = Money.FromDollars(10.00m);
        Assert.True(small <= same);
        Assert.True(small >= same);
    }

    [Fact]
    public void ToString_FormatsAsDollars()
    {
        var money = Money.FromDollars(1234.56m);
        Assert.Equal("$1,234.56", money.ToString());
    }
}
