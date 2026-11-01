using Franz.Common.MultiTenancy;
using Franz.Common.MultiTenancy.TenantResolution;
using Microsoft.AspNetCore.Http;

namespace Franz.API.MultiTenancy;

/// <summary>
/// Executes every enabled resolver, fails closed on an invalid supplied
/// identity, and rejects contradictory tenant identities.
/// </summary>
public sealed class ApiTenantResolutionPipeline(IEnumerable<ITenantResolver> resolvers)
    : ITenantResolutionPipeline
{
    internal const string HttpContextCacheKey = "__Franz.Api.TenantResolution";

    private readonly IReadOnlyList<ITenantResolver> _resolvers =
        (resolvers ?? throw new ArgumentNullException(nameof(resolvers)))
        .OrderBy(resolver => resolver.Order)
        .ToArray();

    public async Task<TenantResolutionResult?> ResolveAsync(object? context = null)
    {
        if (context is HttpContext httpContext &&
            httpContext.Items.TryGetValue(HttpContextCacheKey, out var cached) &&
            cached is TenantResolutionResult cachedResult)
            return cachedResult;

        TenantResolutionResult? resolved = null;

        foreach (var resolver in _resolvers)
        {
            var candidate = await resolver.ResolveTenantAsync(context).ConfigureAwait(false);
            if (candidate is null)
                continue;

            if (!candidate.Succeeded || candidate.Tenant is null)
                return Cache(context, candidate);

            if (resolved is null)
            {
                resolved = candidate;
                continue;
            }

            if (resolved.Tenant!.Id != candidate.Tenant.Id)
            {
                return Cache(context, TenantResolutionResult.FailedResult(
                    candidate.Source,
                    $"Conflicting tenant identities were supplied by {resolved.Source} and {candidate.Source}."));
            }
        }

        return Cache(context, resolved);
    }

    private static TenantResolutionResult? Cache(object? context, TenantResolutionResult? result)
    {
        if (context is HttpContext httpContext && result is not null)
            httpContext.Items[HttpContextCacheKey] = result;
        return result;
    }
}
