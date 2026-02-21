using Octocare.Application.Interfaces;

namespace Octocare.Infrastructure.Tenancy;

/// <summary>
/// Scoped implementation of ITenantContext.
/// Set once per request by TenantResolutionMiddleware.
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        if (TenantId is not null)
            throw new InvalidOperationException("Tenant has already been set for this scope.");

        TenantId = tenantId;
    }
}
