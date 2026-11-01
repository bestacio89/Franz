using Franz.Common.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Franz.API.MultiTenancy;

/// <summary>Rejects malformed or contradictory supplied tenant identities even when tenancy is optional.</summary>
public sealed class TenantResolutionFailureGuardMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantResolutionPipeline pipeline)
    {
        var result = await pipeline.ResolveAsync(context).ConfigureAwait(false);
        if (result is { Succeeded: false })
        {
            context.Response.StatusCode = result.Message?.Contains("Conflicting tenant identities", StringComparison.Ordinal) == true
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
            return;
        }

        await next(context).ConfigureAwait(false);
    }
}
