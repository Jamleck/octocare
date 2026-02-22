namespace Octocare.Application.DTOs;

public record MemberDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTimeOffset JoinedAt);
