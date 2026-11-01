using Franz.Common.MultiTenancy;
using Franz.Common.MultiTenancy.TenantResolution;

namespace Franz.API.MultiTenancy;

/// <summary>
/// Executes every enabled resolver, fails closed on an invalid supplied
/// identity, and rejects contradictory tenant identities.
/// </summary>
public sealed class ApiTenantResolutionPipeline(IEnumerable<ITenantResolver> resolvers)
    : ITenantResolutionPipeline
{
    private readonly IReadOnlyList<ITenantResolver> _resolvers =
        (resolvers ?? throw new ArgumentNullException(nameof(resolvers)))
        .OrderBy(resolver => resolver.Order)
        .ToArray();

    public async Task<TenantResolutionResult?> ResolveAsync(object? context = null)
    {
        TenantResolutionResult? resolved = null;

        foreach (var resolver in _resolvers)
        {
            var candidate = await resolver.ResolveTenantAsync(context).ConfigureAwait(false);
            if (candidate is null)
                continue;

            if (!candidate.Succeeded || candidate.Tenant is null)
                return candidate;

            if (resolved is null)
            {
                resolved = candidate;
                continue;
            }

            if (resolved.Tenant!.Id != candidate.Tenant.Id)
            {
                return TenantResolutionResult.FailedResult(
                    candidate.Source,
                    $"Conflicting tenant identities were supplied by {resolved.Source} and {candidate.Source}.");
            }
        }

        return resolved;
    }
}
