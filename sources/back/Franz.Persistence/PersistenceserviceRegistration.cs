using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Franz.Common.Caching.Extensions;
using Franz.Common.Caching.Options;
using Franz.Common.EntityFramework.Extensions;

namespace Franz.Persistence;

public static class PersistenceServiceRegistration
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
      {
        services.AddScoped(iface, impl);
      }
    }

    return services;
  }

  public static IServiceCollection RegisterPersistenceServices<TDbContext>(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddFranzRedisCaching(options =>
    {
      options.ConnectionString = configuration.GetConnectionString("Redis");
    });

    services.AddFranzMediatorCaching(configuration, opt =>
    {
      var type = typeof(MediatorCachingOptions);

      type.GetProperty(nameof(MediatorCachingOptions.DefaultTtl))?
          .SetValue(opt, TimeSpan.FromMinutes(5));

      type.GetProperty(nameof(MediatorCachingOptions.LogHitLevel))?
          .SetValue(opt, LogLevel.Debug);

      type.GetProperty(nameof(MediatorCachingOptions.LogMissLevel))?
          .SetValue(opt, LogLevel.Information);
    });

    services.AddCustomRepositoriesFromAssembly(typeof(ApplicationDbContext).Assembly);
    services.AddEntityRepositories<ApplicationDbContext>();

    return services;
  }
}