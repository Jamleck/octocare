using Microsoft.AspNetCore.Authorization;
using Octocare.Domain.Enums;

namespace Octocare.Api.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddOctocareAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanReadOrg", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.OrganisationRead)));
            options.AddPolicy("CanManageOrg", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.OrganisationManage)));
            options.AddPolicy("CanReadMembers", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.MembersRead)));
            options.AddPolicy("CanManageMembers", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.MembersManage)));
            options.AddPolicy("CanReadParticipants", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.ParticipantsRead)));
            options.AddPolicy("CanWriteParticipants", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.ParticipantsWrite)));
            options.AddPolicy("CanReadFinance", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.FinanceRead)));
            options.AddPolicy("CanWriteFinance", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.FinanceWrite)));
            options.AddPolicy("CanReadProviders", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.ProvidersRead)));
            options.AddPolicy("CanWriteProviders", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.ProvidersWrite)));
        });

        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        return services;
    }
}
