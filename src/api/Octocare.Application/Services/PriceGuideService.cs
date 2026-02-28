using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class PriceGuideService
{
    private readonly IPriceGuideRepository _priceGuideRepo;

    public PriceGuideService(IPriceGuideRepository priceGuideRepo)
    {
        _priceGuideRepo = priceGuideRepo;
    }

    public async Task<IReadOnlyList<PriceGuideVersionDto>> GetVersionsAsync(CancellationToken ct)
    {
        var versions = await _priceGuideRepo.GetAllVersionsAsync(ct);
        return versions.Select(MapVersionToDto).ToList();
    }

    public async Task<PriceGuideVersionDto?> GetCurrentVersionAsync(CancellationToken ct)
    {
        var version = await _priceGuideRepo.GetCurrentVersionAsync(ct);
        return version is null ? null : MapVersionToDto(version);
    }

    public async Task<PagedResult<SupportItemDto>> GetItemsAsync(
        Guid versionId, int page, int pageSize, string? search,
        SupportCategory? category, CancellationToken ct)
    {
        var (items, totalCount) = await _priceGuideRepo.GetItemsAsync(
            versionId, page, pageSize, search, category, ct);
        return new PagedResult<SupportItemDto>(
            items.Select(MapItemToDto).ToList(),
            totalCount, page, pageSize);
    }

    public async Task<PriceGuideVersionDto> ImportVersionAsync(ImportPriceGuideRequest request, CancellationToken ct)
    {
        var version = PriceGuideVersion.Create(request.Name, request.EffectiveFrom, request.EffectiveTo);
        version.SetCurrent(true);

        await _priceGuideRepo.AddVersionAsync(version, ct);

        var items = request.Items.Select(item => SupportItem.Create(
            version.Id,
            item.ItemNumber,
            item.Name,
            item.SupportCategory,
            item.SupportPurpose,
            item.Unit,
            Money.FromDollars(item.PriceLimitNational).Cents,
            Money.FromDollars(item.PriceLimitRemote).Cents,
            Money.FromDollars(item.PriceLimitVeryRemote).Cents,
            item.IsTtpEligible,
            item.CancellationRule,
            item.ClaimType));

        await _priceGuideRepo.AddItemsAsync(items, ct);

        return MapVersionToDto(version);
    }

    private static PriceGuideVersionDto MapVersionToDto(PriceGuideVersion v)
    {
        return new PriceGuideVersionDto(v.Id, v.Name, v.EffectiveFrom, v.EffectiveTo, v.IsCurrent);
    }

    private static SupportItemDto MapItemToDto(SupportItem i)
    {
        return new SupportItemDto(
            i.Id,
            i.ItemNumber,
            i.Name,
            i.SupportCategory,
            i.SupportPurpose,
            i.Unit,
            new Money(i.PriceLimitNational).ToDollars(),
            new Money(i.PriceLimitRemote).ToDollars(),
            new Money(i.PriceLimitVeryRemote).ToDollars(),
            i.IsTtpEligible,
            i.CancellationRule,
            i.ClaimType);
    }
}
