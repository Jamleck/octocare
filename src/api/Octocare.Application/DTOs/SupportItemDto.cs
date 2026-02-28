using Octocare.Domain.Enums;

namespace Octocare.Application.DTOs;

public record SupportItemDto(
    Guid Id,
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
