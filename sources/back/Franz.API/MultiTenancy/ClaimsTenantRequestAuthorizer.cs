using System.Security.Claims;
using Franz.Common.Http.MultiTenancy;
using Franz.Common.MultiTenancy.Models;

namespace Franz.API.MultiTenancy;

/// <summary>
/// Requires an authenticated principal to carry an explicit tenant-membership
/// claim. Tenant resolution data alone never grants access.
/// </summary>
public sealed class ClaimsTenantRequestAuthorizer(string membershipClaimType) : ITenantRequestAuthorizer
{
    public Task<bool> IsAuthorizedAsync(
        ClaimsPrincipal principal,
        TenantInfo tenant,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(tenant);
        cancellationToken.ThrowIfCancellationRequested();

        var authorized = principal.FindAll(membershipClaimType)
            .SelectMany(claim => claim.Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Any(value => Guid.TryParse(value, out var tenantId) && tenantId == tenant.Id);

        return Task.FromResult(authorized);
    }
}
