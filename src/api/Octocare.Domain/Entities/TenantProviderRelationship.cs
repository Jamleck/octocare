namespace Octocare.Domain.Entities;

/// <summary>
/// Junction table linking a tenant (Organisation) to a Provider.
/// This enables multi-tenancy: providers are shared across tenants,
/// but each tenant only sees the providers linked to them.
/// </summary>
public class TenantProviderRelationship
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ProviderId { get; private set; }
    public string Status { get; private set; } = "active";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private TenantProviderRelationship() { }

    public static TenantProviderRelationship Create(Guid tenantId, Guid providerId, string status = "active")
    {
        return new TenantProviderRelationship
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderId = providerId,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
