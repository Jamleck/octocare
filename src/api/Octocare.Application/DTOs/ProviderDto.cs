namespace Octocare.Application.DTOs;

public record ProviderDto(
    Guid Id,
    string Name,
    string? Abn,
    string? ContactEmail,
    string? ContactPhone,
    string? Address,
    bool IsActive,
    DateTimeOffset CreatedAt);
