using Microsoft.Extensions.DependencyInjection;
using Octocare.Application.Services;

namespace Octocare.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<OrganisationService>();
        services.AddScoped<MemberService>();
        services.AddScoped<ParticipantService>();
        services.AddScoped<ProviderService>();
        services.AddScoped<PriceGuideService>();

        return services;
    }
}
