using Franz.Common.Caching.Extensions;
using Franz.Common.Data;
using Franz.Contracts.Persistence;
using Franz.Persistence.Persistence.Repositories;
using Franz.Persistence.Repositories;
using Franz.Persistence.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Franz.Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection RegisterPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFranzRedisCaching(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("Redis");
        });

        services.AddTenantScopedCacheKeys();
        services.AddFranzMediatorCaching(configuration);

        // Explicit registrations are deterministic and Native AOT-friendly.
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ISeeder, BookSeeder>();
        services.AddScoped<ISeeder, MemberSeeder>();

        return services;
    }
}
