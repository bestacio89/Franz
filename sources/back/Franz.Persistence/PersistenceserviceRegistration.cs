using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Franz.Common.Business.Domain;
using Franz.Common.Business.Events;
using Franz.Common.EntityFramework;
using Franz.Common.EntityFramework.SQLServer.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Franz.Persistence
{

  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection RegisterPersistenceServices<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)     
      where TDbContext : DbContextBase
    {
      // Read TIdType from appsettings.json
      var tidTypeName = configuration.GetValue<string>("TIdType");
      if (string.IsNullOrWhiteSpace(tidTypeName))
      {
        throw new ApplicationException("TIdType is not defined in appsettings.json.");
      }

      var tidType = Type.GetType(tidTypeName);
      if (tidType == null)
      {
        throw new ApplicationException($"Type '{tidTypeName}' specified in appsettings.json for TIdType was not found.");
      }

      // Register persistence services with dynamically determined types
      services.AddSqlServerDatabase<ApplicationDbContext>(configuration);

      return services;
    }

  }
}
