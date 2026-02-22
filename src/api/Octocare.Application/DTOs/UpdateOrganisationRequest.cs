namespace Octocare.Application.DTOs;

public record UpdateOrganisationRequest(
    string Name,
    string? Abn,
    string? ContactEmail,
    string? ContactPhone,
    string? Address);
