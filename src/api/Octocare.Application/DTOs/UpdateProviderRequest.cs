namespace Octocare.Application.DTOs;

public record UpdateProviderRequest(
    string Name,
    string? Abn,
    string? ContactEmail,
    string? ContactPhone,
    string? Address);
