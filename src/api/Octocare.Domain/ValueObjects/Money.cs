namespace Octocare.Domain.ValueObjects;

/// <summary>
/// Represents a monetary value stored as integer cents to avoid floating-point precision issues.
/// All arithmetic is performed in cents; convert to dollars only at the display layer.
/// </summary>
public readonly record struct Money : IComparable<Money>
{
    public long Cents { get; }

    public Money(long cents)
    {
        Cents = cents;
    }

    public static Money Zero => new(0);

    public static Money FromDollars(decimal dollars)
    {
        return new Money((long)decimal.Round(dollars * 100, MidpointRounding.ToEven));
    }

    public decimal ToDollars() => Cents / 100m;

    /// <summary>
    /// Applies a multiplier (e.g., 1.175 for TTP loading) using banker's rounding.
    /// </summary>
    public Money ApplyMultiplier(decimal multiplier)
    {
        var result = Cents * multiplier;
        return new Money((long)decimal.Round(result, MidpointRounding.ToEven));
    }

    public int CompareTo(Money other) => Cents.CompareTo(other.Cents);

    public static Money operator +(Money left, Money right) => new(left.Cents + right.Cents);
    public static Money operator -(Money left, Money right) => new(left.Cents - right.Cents);
    public static Money operator -(Money value) => new(-value.Cents);
    public static bool operator >(Money left, Money right) => left.Cents > right.Cents;
    public static bool operator <(Money left, Money right) => left.Cents < right.Cents;
    public static bool operator >=(Money left, Money right) => left.Cents >= right.Cents;
    public static bool operator <=(Money left, Money right) => left.Cents <= right.Cents;

    public override string ToString() => $"${ToDollars():N2}";
}
