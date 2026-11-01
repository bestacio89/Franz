using Franz.API.MultiTenancy;
using Franz.Common.Http.MultiTenancy;
using Franz.Common.Http.MultiTenancy.Configuration;
using Franz.Common.Http.MultiTenancy.Extensions;
using Franz.Common.Http.MultiTenancy.Resolvers;
using Franz.Common.MultiTenancy;
using Franz.Common.MultiTenancy.TenantResolution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Franz.API.Extensions;

/// <summary>Registers API-owned tenant registry, resolvers, and authorization.</summary>
public static class MultiTenancyServiceCollectionExtensions
{
    public static IServiceCollection AddApiMultiTenancy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection("MultiTenancy");
        var header = section["Header:Name"] ?? "X-Tenant-Id";
        var claim = section["JwtClaim:Name"] ?? "tenant_id";
        var membershipClaim = section["Authorization:MembershipClaim"] ?? "tenant_ids";

        services.AddFranzMultiTenancy(options =>
        {
            options.RequireTenant = section.GetValue("RequireTenant", true);
        });

        services.TryAddSingleton<ITenantStore, ConfigurationTenantStore>();
        services.TryAddScoped<ITenantRequestAuthorizer>(_ =>
            new ClaimsTenantRequestAuthorizer(membershipClaim));

        if (section.GetValue("Resolution:Host:Enabled", true))
            services.AddScoped<ITenantResolver, ApiHostTenantResolver>();

        if (section.GetValue("Resolution:Header:Enabled", true))
            services.AddScoped<ITenantResolver>(provider =>
                new HeaderTenantResolver(provider.GetRequiredService<ITenantStore>(), header));

        if (section.GetValue("Resolution:JwtClaim:Enabled", true))
            services.AddScoped<ITenantResolver>(provider =>
                new JwtClaimTenantResolver(provider.GetRequiredService<ITenantStore>(), claim));

        // The API pipeline rejects malformed supplied identifiers and contradictory
        // successful resolutions instead of silently accepting the first match.
        services.Replace(ServiceDescriptor.Scoped<ITenantResolutionPipeline, ApiTenantResolutionPipeline>());
        return services;
    }
}
