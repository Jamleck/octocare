using Octocare.Domain.Enums;

namespace Octocare.Domain.Entities;

public class SupportItem
{
    public Guid Id { get; private set; }
    public Guid VersionId { get; private set; }
    public string ItemNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public SupportCategory SupportCategory { get; private set; }
    public SupportPurpose SupportPurpose { get; private set; }
    public UnitOfMeasure Unit { get; private set; }
    public long PriceLimitNational { get; private set; }
    public long PriceLimitRemote { get; private set; }
    public long PriceLimitVeryRemote { get; private set; }
    public bool IsTtpEligible { get; private set; }
    public CancellationRule CancellationRule { get; private set; }
    public ClaimType ClaimType { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public PriceGuideVersion Version { get; private set; } = null!;

    private SupportItem() { }

    public static SupportItem Create(
        Guid versionId,
        string itemNumber,
        string name,
        SupportCategory supportCategory,
        SupportPurpose supportPurpose,
        UnitOfMeasure unit,
        long priceLimitNational,
        long priceLimitRemote,
        long priceLimitVeryRemote,
        bool isTtpEligible,
        CancellationRule cancellationRule,
        ClaimType claimType)
    {
        return new SupportItem
        {
            Id = Guid.NewGuid(),
            VersionId = versionId,
            ItemNumber = itemNumber,
            Name = name,
            SupportCategory = supportCategory,
            SupportPurpose = supportPurpose,
            Unit = unit,
            PriceLimitNational = priceLimitNational,
            PriceLimitRemote = priceLimitRemote,
            PriceLimitVeryRemote = priceLimitVeryRemote,
            IsTtpEligible = isTtpEligible,
            CancellationRule = cancellationRule,
            ClaimType = claimType,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
