using Octocare.Domain.Enums;

namespace Octocare.Application.DTOs;

public record ImportPriceGuideRequest(
    string Name,
    DateOnly EffectiveFrom,
    DateOnly EffectiveTo,
    List<ImportSupportItemRequest> Items);

public record ImportSupportItemRequest(
    string ItemNumber,
    string Name,
    SupportCategory SupportCategory,
    SupportPurpose SupportPurpose,
    UnitOfMeasure Unit,
    decimal PriceLimitNational,
    decimal PriceLimitRemote,
    decimal PriceLimitVeryRemote,
    bool IsTtpEligible,
    CancellationRule CancellationRule,
    ClaimType ClaimType);
