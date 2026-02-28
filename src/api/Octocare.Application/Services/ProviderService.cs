using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Application.Services;

public class ProviderService
{
    private readonly IProviderRepository _providerRepo;

    public ProviderService(IProviderRepository providerRepo)
    {
        _providerRepo = providerRepo;
    }

    public async Task<ProviderDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var provider = await _providerRepo.GetByIdAsync(id, ct);
        return provider is null ? null : MapToDto(provider);
    }

    public async Task<PagedResult<ProviderDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        var (items, totalCount) = await _providerRepo.GetPagedAsync(page, pageSize, search, ct);
        return new PagedResult<ProviderDto>(
            items.Select(MapToDto).ToList(),
            totalCount, page, pageSize);
    }

    public async Task<ProviderDto> CreateAsync(CreateProviderRequest request, CancellationToken ct)
    {
        if (request.Abn is not null && await _providerRepo.ExistsByAbnAsync(request.Abn, ct: ct))
            throw new InvalidOperationException("A provider with this ABN already exists.");

        var provider = Provider.Create(
            request.Name, request.Abn, request.ContactEmail,
            request.ContactPhone, request.Address);

        await _providerRepo.AddAsync(provider, ct);
        return MapToDto(provider);
    }

    public async Task<ProviderDto> UpdateAsync(Guid id, UpdateProviderRequest request, CancellationToken ct)
    {
        var provider = await _providerRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Provider not found.");

        if (request.Abn is not null && await _providerRepo.ExistsByAbnAsync(request.Abn, provider.Id, ct))
            throw new InvalidOperationException("A provider with this ABN already exists.");

        provider.Update(
            request.Name, request.Abn, request.ContactEmail,
            request.ContactPhone, request.Address);

        await _providerRepo.UpdateAsync(provider, ct);
        return MapToDto(provider);
    }

    private static ProviderDto MapToDto(Provider p)
    {
        return new ProviderDto(
            p.Id, p.Name, p.Abn, p.ContactEmail,
            p.ContactPhone, p.Address, p.IsActive, p.CreatedAt);
    }
}
