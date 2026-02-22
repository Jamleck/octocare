using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;

namespace Octocare.Application.Services;

public class OrganisationService
{
    private readonly IOrganisationRepository _orgRepo;
    private readonly ITenantContext _tenantContext;

    public OrganisationService(IOrganisationRepository orgRepo, ITenantContext tenantContext)
    {
        _orgRepo = orgRepo;
        _tenantContext = tenantContext;
    }

    public async Task<OrganisationDto?> GetCurrentOrganisationAsync(CancellationToken ct)
    {
        if (_tenantContext.TenantId is not { } tenantId)
            return null;

        var org = await _orgRepo.GetByIdAsync(tenantId, ct);
        return org is null ? null : MapToDto(org);
    }

    public async Task<OrganisationDto> UpdateOrganisationAsync(UpdateOrganisationRequest request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var org = await _orgRepo.GetByIdAsync(tenantId, ct)
            ?? throw new KeyNotFoundException("Organisation not found.");

        org.Update(request.Name, request.Abn, request.ContactEmail, request.ContactPhone, request.Address);
        await _orgRepo.UpdateAsync(org, ct);
        return MapToDto(org);
    }

    private static OrganisationDto MapToDto(Domain.Entities.Organisation org)
    {
        return new OrganisationDto(
            org.Id, org.Name, org.Abn, org.ContactEmail,
            org.ContactPhone, org.Address, org.IsActive, org.CreatedAt);
    }
}
