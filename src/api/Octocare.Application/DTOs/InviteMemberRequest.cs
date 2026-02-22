namespace Octocare.Application.DTOs;

public record InviteMemberRequest(
    string Email,
    string FirstName,
    string LastName,
    string Role);
