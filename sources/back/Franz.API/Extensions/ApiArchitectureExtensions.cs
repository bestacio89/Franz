using Franz.Common.Http;
using Franz.Common.Http.Documentation.Extensions;
using Franz.Common.Http.MultiTenancy.Extensions;\nusing Franz.API.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Franz.API.Extensions;

/// <summary>
/// Composes the shared HTTP pipeline while placing tenant resolution between
/// authentication and authorization. This is intentionally API-owned so the
/// reusable Franz.Common packages remain unchanged.
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
            .UseFranzMultiTenancy()
            .UseAuthorization()
            .UseEndpoints(endpoints => endpoints.MapControllers().RequireAuthorization())
            .UseDocumentation()
            .UseHealthChecks("/health");

        return app;
    }
}
