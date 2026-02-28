using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data;

public class OctocareDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public OctocareDbContext(DbContextOptions<OctocareDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserOrgMembership> UserOrgMemberships => Set<UserOrgMembership>();
    public DbSet<Participant> Participants => Set<Participant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OctocareDbContext).Assembly);

        // Global query filters for tenant isolation
        modelBuilder.Entity<Organisation>()
            .HasQueryFilter(o => _tenantContext.TenantId == null || o.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<UserOrgMembership>()
            .HasQueryFilter(m => _tenantContext.TenantId == null || m.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Participant>()
            .HasQueryFilter(p => _tenantContext.TenantId == null || p.TenantId == _tenantContext.TenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Sets the PostgreSQL session variable for RLS tenant isolation.
    /// Call this after opening a connection and before executing queries.
    /// Skips gracefully on non-PostgreSQL providers (e.g., SQLite in tests).
    /// </summary>
    public virtual async Task SetTenantAsync(CancellationToken cancellationToken = default)
    {
        if (_tenantContext.TenantId is not { } tenantId)
            return;

        // SET session variables are PostgreSQL-specific; skip for other providers (e.g., SQLite in tests)
        if (!Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ?? true)
            return;

        var connection = Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SET app.current_tenant = '{tenantId}'";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        var now = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.Entity is Organisation org)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(org.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(org.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(user.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(user.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is UserOrgMembership membership)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(membership.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(membership.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is Participant participant)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(participant.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(participant.UpdatedAt)).CurrentValue = now;
            }
        }
    }
}
