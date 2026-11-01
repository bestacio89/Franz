using Franz.Common.MultiTenancy;
using Franz.Common.MultiTenancy.Models;
using Franz.Common.MultiTenancy.TenantResolution;
using Microsoft.AspNetCore.Http;

namespace Franz.API.MultiTenancy;

/// <summary>Resolves only explicitly registered hosts; unrelated hosts remain non-applicable.</summary>
public sealed class ApiHostTenantResolver(ITenantStore store) : ITenantResolver
{
    public int Order => 50;

    public async Task<TenantResolutionResult?> ResolveTenantAsync(object? context)
    {
        if (context is not HttpContext httpContext)
            return null;

        var host = httpContext.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host))
            return null;

        string normalizedHost;
        try
        {
            normalizedHost = TenantDomain.NormalizeHost(host);
        }
        catch (ArgumentException)
        {
            return TenantResolutionResult.FailedResult(TenantResolutionSource.Host, "The request host is invalid.");
        }

        var tenant = await store.FindByHostAsync(normalizedHost).ConfigureAwait(false);
        if (tenant is null)
            return null;

        var verified = tenant.Domains.Any(domain =>
            domain.IsVerified && domain.Host == normalizedHost &&
            (domain.TenantId == Guid.Empty || domain.TenantId == tenant.Id));

        return verified
            ? TenantResolutionResult.Success(tenant, TenantResolutionSource.Host)
            : TenantResolutionResult.FailedResult(TenantResolutionSource.Host, "The host mapping is not verified.");
    }
}
