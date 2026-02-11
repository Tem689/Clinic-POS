using System.Security.Claims;
using Clinic.Backend.Services;

namespace Clinic.Backend.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant)
    {
        // Extract tenant_id from JWT claims
        // Claim name 'tenant_id' or standard 'tid'
        var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;

        if (!string.IsNullOrEmpty(tenantIdClaim) && int.TryParse(tenantIdClaim, out int tenantId))
        {
            // For this slice, we just set the ID. 
            // In a real app, we might look up the tenant name or verify it's active.
            currentTenant.SetTenant(tenantId, "Tenant " + tenantId);
        }

        await _next(context);
    }
}
