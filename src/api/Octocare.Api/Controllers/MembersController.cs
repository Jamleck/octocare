using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Octocare.Application.DTOs;
using Octocare.Application.Services;
using Octocare.Application.Validators;

namespace Octocare.Api.Controllers;

[ApiController]
[Route("api/organisations/current/members")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly MemberService _memberService;

    public MembersController(MemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    [Authorize(Policy = "CanReadMembers")]
    public async Task<ActionResult<IReadOnlyList<MemberDto>>> GetMembers(CancellationToken ct)
    {
        var members = await _memberService.GetMembersAsync(ct);
        return Ok(members);
    }

    [HttpPost("invite")]
    [Authorize(Policy = "CanManageMembers")]
    public async Task<ActionResult<MemberDto>> InviteMember(
        InviteMemberRequest request, CancellationToken ct)
    {
        var (isValid, errors) = MemberValidation.ValidateInvite(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _memberService.InviteMemberAsync(request, ct);
            return CreatedAtAction(nameof(GetMembers), result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPut("{userId:guid}/role")]
    [Authorize(Policy = "CanManageMembers")]
    public async Task<ActionResult<MemberDto>> UpdateRole(
        Guid userId, UpdateMemberRoleRequest request, CancellationToken ct)
    {
        var (isValid, errors) = MemberValidation.ValidateRoleUpdate(request);
        if (!isValid)
            return ValidationProblem(new ValidationProblemDetails(errors));

        try
        {
            var result = await _memberService.UpdateMemberRoleAsync(userId, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{userId:guid}/deactivate")]
    [Authorize(Policy = "CanManageMembers")]
    public async Task<IActionResult> DeactivateMember(Guid userId, CancellationToken ct)
    {
        try
        {
            await _memberService.DeactivateMemberAsync(userId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
