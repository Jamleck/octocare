using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Application.Services;

public class ParticipantService
{
    private readonly IParticipantRepository _participantRepo;
    private readonly ITenantContext _tenantContext;

    public ParticipantService(IParticipantRepository participantRepo, ITenantContext tenantContext)
    {
        _participantRepo = participantRepo;
        _tenantContext = tenantContext;
    }

    public async Task<ParticipantDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var participant = await _participantRepo.GetByIdAsync(id, ct);
        return participant is null ? null : MapToDto(participant);
    }

    public async Task<PagedResult<ParticipantDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        var (items, totalCount) = await _participantRepo.GetPagedAsync(page, pageSize, search, ct);
        return new PagedResult<ParticipantDto>(
            items.Select(MapToDto).ToList(),
            totalCount, page, pageSize);
    }

    public async Task<ParticipantDto> CreateAsync(CreateParticipantRequest request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        if (await _participantRepo.ExistsByNdisNumberAsync(request.NdisNumber, ct: ct))
            throw new InvalidOperationException("A participant with this NDIS number already exists.");

        var participant = Participant.Create(
            tenantId, request.NdisNumber, request.FirstName, request.LastName,
            request.DateOfBirth, request.Email, request.Phone, request.Address,
            request.NomineeName, request.NomineeEmail, request.NomineePhone,
            request.NomineeRelationship);

        await _participantRepo.AddAsync(participant, ct);
        return MapToDto(participant);
    }

    public async Task<ParticipantDto> UpdateAsync(Guid id, UpdateParticipantRequest request, CancellationToken ct)
    {
        var participant = await _participantRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Participant not found.");

        participant.Update(
            request.FirstName, request.LastName, request.DateOfBirth,
            request.Email, request.Phone, request.Address,
            request.NomineeName, request.NomineeEmail, request.NomineePhone,
            request.NomineeRelationship);

        await _participantRepo.UpdateAsync(participant, ct);
        return MapToDto(participant);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct)
    {
        var participant = await _participantRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Participant not found.");

        participant.Deactivate();
        await _participantRepo.UpdateAsync(participant, ct);
    }

    private static ParticipantDto MapToDto(Participant p)
    {
        return new ParticipantDto(
            p.Id, p.NdisNumber, p.FirstName, p.LastName,
            p.DateOfBirth, p.Email, p.Phone, p.Address,
            p.NomineeName, p.NomineeEmail, p.NomineePhone,
            p.NomineeRelationship, p.IsActive, p.CreatedAt);
    }
}
