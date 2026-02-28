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
    public DbSet<StoredEvent> Events => Set<StoredEvent>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<TenantProviderRelationship> TenantProviderRelationships => Set<TenantProviderRelationship>();
    public DbSet<PriceGuideVersion> PriceGuideVersions => Set<PriceGuideVersion>();
    public DbSet<SupportItem> SupportItems => Set<SupportItem>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();
    public DbSet<ServiceAgreement> ServiceAgreements => Set<ServiceAgreement>();
    public DbSet<ServiceAgreementItem> ServiceAgreementItems => Set<ServiceAgreementItem>();
    public DbSet<ServiceBooking> ServiceBookings => Set<ServiceBooking>();

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

        modelBuilder.Entity<TenantProviderRelationship>()
            .HasQueryFilter(r => _tenantContext.TenantId == null || r.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Plan>()
            .HasQueryFilter(p => _tenantContext.TenantId == null || p.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ServiceAgreement>()
            .HasQueryFilter(sa => _tenantContext.TenantId == null || sa.TenantId == _tenantContext.TenantId);

        // No query filter on Provider, StoredEvent, BudgetCategory, ServiceAgreementItem, or ServiceBooking — they are filtered via parent relationship
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
            else if (entry.Entity is Provider provider)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(provider.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(provider.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is TenantProviderRelationship relationship)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(relationship.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(relationship.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is PriceGuideVersion version)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(version.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(version.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is SupportItem supportItem)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(supportItem.CreatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is Plan plan)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(plan.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(plan.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is BudgetCategory budgetCategory)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(budgetCategory.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(budgetCategory.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is ServiceAgreement agreement)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(agreement.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(agreement.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is ServiceAgreementItem agreementItem)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(agreementItem.CreatedAt)).CurrentValue = now;
            }
            else if (entry.Entity is ServiceBooking booking)
            {
                if (entry.State == EntityState.Added)
                    entry.Property(nameof(booking.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(booking.UpdatedAt)).CurrentValue = now;
            }
        }
    }
}
