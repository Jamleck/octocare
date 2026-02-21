using Octocare.Application.Interfaces;

namespace Octocare.Api.Middleware;

/// <summary>
/// Resolves the current tenant from the authenticated user's JWT claims
/// or from the X-Tenant-Id header in development.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantId = ResolveTenantId(context);
        if (tenantId.HasValue)
        {
            tenantContext.SetTenant(tenantId.Value);
        }

        await _next(context);
    }

    private static Guid? ResolveTenantId(HttpContext context)
    {
        // Try JWT org_id claim first
        var orgIdClaim = context.User.FindFirst("org_id")?.Value;
        if (Guid.TryParse(orgIdClaim, out var claimTenantId))
            return claimTenantId;

        // Fall back to X-Tenant-Id header in development
        if (context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            var headerValue = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (Guid.TryParse(headerValue, out var headerTenantId))
                return headerTenantId;
        }

        return null;
    }
}
