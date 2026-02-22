namespace Octocare.Application.DTOs;

public record OrganisationDto(
    Guid Id,
    string Name,
    string? Abn,
    string? ContactEmail,
    string? ContactPhone,
    string? Address,
    bool IsActive,
    DateTimeOffset CreatedAt);
