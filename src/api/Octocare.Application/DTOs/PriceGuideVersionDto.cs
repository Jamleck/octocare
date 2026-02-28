namespace Octocare.Application.DTOs;

public record PriceGuideVersionDto(
    Guid Id,
    string Name,
    DateOnly EffectiveFrom,
    DateOnly EffectiveTo,
    bool IsCurrent);
