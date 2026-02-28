namespace Octocare.Domain.Entities;

public class PriceGuideVersion
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly EffectiveTo { get; private set; }
    public bool IsCurrent { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<SupportItem> Items { get; private set; } = [];

    private PriceGuideVersion() { }

    public static PriceGuideVersion Create(string name, DateOnly effectiveFrom, DateOnly effectiveTo)
    {
        return new PriceGuideVersion
        {
            Id = Guid.NewGuid(),
            Name = name,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsCurrent = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SetCurrent(bool isCurrent)
    {
        IsCurrent = isCurrent;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
