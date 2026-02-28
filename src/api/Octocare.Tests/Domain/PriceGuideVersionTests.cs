using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class PriceGuideVersionTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var version = PriceGuideVersion.Create(
            "2025-26",
            new DateOnly(2025, 7, 1),
            new DateOnly(2026, 6, 30));

        Assert.NotEqual(Guid.Empty, version.Id);
        Assert.Equal("2025-26", version.Name);
        Assert.Equal(new DateOnly(2025, 7, 1), version.EffectiveFrom);
        Assert.Equal(new DateOnly(2026, 6, 30), version.EffectiveTo);
        Assert.False(version.IsCurrent);
        Assert.NotEmpty(version.Items.ToString()!); // collection initialized
    }

    [Fact]
    public void SetCurrent_UpdatesIsCurrentFlag()
    {
        var version = PriceGuideVersion.Create(
            "2025-26",
            new DateOnly(2025, 7, 1),
            new DateOnly(2026, 6, 30));

        version.SetCurrent(true);
        Assert.True(version.IsCurrent);

        version.SetCurrent(false);
        Assert.False(version.IsCurrent);
    }
}
