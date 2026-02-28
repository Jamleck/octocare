using System.Net.Http.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Octocare.Infrastructure.Data;
using Octocare.Infrastructure.Data.Seeding;

namespace Octocare.Tests.Integration;

/// <summary>
/// Base class for integration tests. Sets up an in-memory SQLite database,
/// creates the schema, and seeds dev data.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected OctocareTestFactory Factory = null!;
    protected HttpClient Client = null!;
    private SqliteConnection _keepAliveConnection = null!;

    public async Task InitializeAsync()
    {
        Factory = new OctocareTestFactory();

        // We need to open a keep-alive connection to the in-memory SQLite database
        // BEFORE any DbContext operations, so the database persists across scopes.
        // Extract the connection string from the factory's configured services.
        var sp = Factory.Services;
        using (var scope = sp.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OctocareDbContext>();
            var connString = db.Database.GetConnectionString();
            _keepAliveConnection = new SqliteConnection(connString);
            await _keepAliveConnection.OpenAsync();

            // Create the schema from the EF model (not migrations, which are PostgreSQL-specific)
            await db.Database.EnsureCreatedAsync();

            // Seed development data
            try
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
                await seeder.SeedAsync();
            }
            catch
            {
                // If seeder fails (e.g., due to PostgreSQL RLS functions), that's OK for basic tests
            }
        }

        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory != null)
            await Factory.DisposeAsync();
        if (_keepAliveConnection != null)
        {
            await _keepAliveConnection.CloseAsync();
            await _keepAliveConnection.DisposeAsync();
        }
    }
}
