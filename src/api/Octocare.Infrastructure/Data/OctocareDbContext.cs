using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;

namespace Octocare.Infrastructure.Data;

public class OctocareDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public OctocareDbContext(DbContextOptions<OctocareDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Sets the PostgreSQL session variable for RLS tenant isolation.
    /// Call this after opening a connection and before executing queries.
    /// </summary>
    public async Task SetTenantAsync(CancellationToken cancellationToken = default)
    {
        if (_tenantContext.TenantId is not { } tenantId)
            return;

        var connection = Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SET app.current_tenant = '{tenantId}'";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
