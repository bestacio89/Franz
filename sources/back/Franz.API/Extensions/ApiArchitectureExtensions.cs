using Franz.API.MultiTenancy;
using Franz.Common.Http;
using Franz.Common.Http.Documentation.Extensions;
using Franz.Common.Http.MultiTenancy.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Franz.API.Extensions;

/// <summary>
/// Composes the HTTP pipeline with tenant resolution between authentication
/// and authorization. This composition is owned by the API, not Franz.Common.
/// </summary>
public static class ApiArchitectureExtensions
{
    public static IApplicationBuilder UseFranzApiArchitecture(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler()
            .UseForwardedHeaders()
            .UseCertificateForwarding()
            .UseFrenchLocalization()
            .UseRouting()
            .UseDefaultCors()
            .UseAuthentication()
            .UseMiddleware<TenantResolutionFailureGuardMiddleware>()
            .UseFranzMultiTenancy()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();
                endpoints.MapHealthChecks("/health");
            })
            .UseDocumentation();

        return app;
    }
}
