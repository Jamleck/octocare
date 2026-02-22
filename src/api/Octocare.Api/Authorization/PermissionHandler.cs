using Microsoft.AspNetCore.Authorization;
using Octocare.Application.Interfaces;

namespace Octocare.Api.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserService _currentUserService;

    public PermissionHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (await _currentUserService.HasPermissionAsync(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
