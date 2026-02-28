namespace Octocare.Application.DTOs;

public record ServiceAgreementItemDto(
    Guid Id,
    string SupportItemNumber,
    decimal AgreedRate,
    string? Frequency);
