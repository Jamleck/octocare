using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Octocare.Api.Authentication;
using Octocare.Infrastructure.Data;

namespace Octocare.Tests.Integration;

/// <summary>
/// WebApplicationFactory that replaces PostgreSQL with SQLite in-memory for integration tests.
/// Uses the "Testing" environment to skip the Program.cs migration/seed block, then
/// sets up DevAuth and SQLite manually.
/// </summary>
public class OctocareTestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"octocare_test_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use "Testing" environment to skip the Development-only migration/seed block in Program.cs.
        // We'll configure DevAuth manually below.
        builder.UseEnvironment("Testing");

        // Set Auth:DevBypass via UseSetting so the DevAuthHandler can be resolved
        builder.UseSetting("Auth:DevBypass", "true");

        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext-related registrations (Npgsql, Aspire enrichment, etc.)
            // We must be thorough because EnrichNpgsqlDbContext registers internal Npgsql services
            // that conflict with SQLite.
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<OctocareDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(OctocareDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                    d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
                    d.ServiceType.FullName?.Contains("Npgsql") == true ||
                    d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
                    (d.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") == true &&
                     d.ServiceType.FullName?.Contains("OctocareDbContext") == true))
                .ToList();

            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            // Add SQLite in-memory with a unique name per factory instance.
            // SetTenantAsync in OctocareDbContext auto-detects the provider and skips
            // PostgreSQL-specific SET commands when running on SQLite.
            services.AddDbContext<OctocareDbContext>(options =>
            {
                options.UseSqlite($"DataSource=file:{_dbName}?mode=memory&cache=shared");
            });

            // Replace authentication with DevAuth (since we're not in Development environment,
            // Program.cs won't set it up, so we do it here)
            services.AddAuthentication(DevAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, _ => { });
        });
    }
}
