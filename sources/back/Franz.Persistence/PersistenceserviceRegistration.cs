using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Franz.Common.Caching.Extensions;

namespace Franz.Persistence
{
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection RegisterPersistenceServices<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
    {
      // ✅ Add Franz Redis Caching + Mediator Caching pipeline
      services
        .AddFranzRedisCaching(configuration.GetConnectionString("Redis"), database: 1)
        .AddFranzMediatorCaching(opt =>
        {
          opt.DefaultTtl = TimeSpan.FromMinutes(5);
          opt.ShouldCache = req => true; // Always cache by default
          opt.LogHitLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
          opt.LogMissLevel = Microsoft.Extensions.Logging.LogLevel.Information;
        });

      // ✅ Add persistence services with dynamically determined types (if needed)
      // Example: services.AddDatabase<TDbContext>(configuration);

      return services;
    }
  }
}
