namespace Octocare.Application.DTOs;

public record ParticipantDto(
    Guid Id,
    string NdisNumber,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Email,
    string? Phone,
    string? Address,
    string? NomineeName,
    string? NomineeEmail,
    string? NomineePhone,
    string? NomineeRelationship,
    bool IsActive,
    DateTimeOffset CreatedAt);
