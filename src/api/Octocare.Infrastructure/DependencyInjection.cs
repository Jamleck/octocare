using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octocare.Application.Interfaces;
using Octocare.Infrastructure.Data;
using Octocare.Infrastructure.Data.Repositories;
using Octocare.Infrastructure.Data.Seeding;
using Octocare.Infrastructure.Tenancy;

namespace Octocare.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // Register DbContext without pooling (OctocareDbContext depends on scoped ITenantContext).
        // Connection string "octocare" matches the Aspire AppHost database resource name.
        var connectionString = builder.Configuration.GetConnectionString("octocare");
        builder.Services.AddDbContext<OctocareDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add Aspire health checks, retries, and telemetry on top of the manually registered DbContext.
        builder.EnrichNpgsqlDbContext<OctocareDbContext>();

        builder.Services.AddScoped<ITenantContext, TenantContext>();

        // Repositories
        builder.Services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
        builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
        builder.Services.AddScoped<IEventStore, EventStoreRepository>();
        builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
        builder.Services.AddScoped<IPriceGuideRepository, PriceGuideRepository>();
        builder.Services.AddScoped<IPlanRepository, PlanRepository>();
        builder.Services.AddScoped<IServiceAgreementRepository, ServiceAgreementRepository>();
        builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        // Seeding
        builder.Services.AddScoped<DevDataSeeder>();

        return builder;
    }
}
