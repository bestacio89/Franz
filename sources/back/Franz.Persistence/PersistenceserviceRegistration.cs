using Franz.Common.Caching.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Franz.Persistence
{
  public static class ServiceCollectionExtensions
  {

    public static IServiceCollection AddCustomRepositoriesFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
      var repoTypes = assembly
          .GetTypes()
          .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase));

      foreach (var impl in repoTypes)
      {
        var iface = impl.GetInterfaces().FirstOrDefault(i => i.Name == "I" + impl.Name);
        if (iface != null)
          services.AddScoped(iface, impl);
      }

      return services;
    }
    public static IServiceCollection RegisterPersistenceServices<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
    {
      // ✅ Add Franz Redis Caching + Mediator Caching pipeline
     /* services
        .AddFranzRedisCaching(configuration.GetConnectionString("Redis"), database: 1)
        .AddFranzMediatorCaching(opt =>
        {
          opt.DefaultTtl = TimeSpan.FromMinutes(5);
          opt.ShouldCache = req => true; // Always cache by default
          opt.LogHitLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
          opt.LogMissLevel = Microsoft.Extensions.Logging.LogLevel.Information;
        });*/
      services.AddFranzCaching();
      services.AddCustomRepositoriesFromAssembly(typeof(ApplicationDbContext).Assembly);
      services.AddGenericRepositories<ApplicationDbContext>();
      services.AddEntityRepositories<ApplicationDbContext>();
      services.AddScoped(typeof(EntityRepository<,>), typeof(EntityRepository<,>));

      // ✅ Add persistence services with dynamically determined types (if needed)
      // Example: services.AddDatabase<TDbContext>(configuration);

      return services;
    }
  }
}
