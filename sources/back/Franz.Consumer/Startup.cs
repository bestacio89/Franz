using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Franz.Persistence;
using Franz.Common.Business.Domain;
using Franz.Common.EntityFramework;
using Franz.Common.EntityFramework.SQLServer.Extensions;
using Franz.Common.Messaging.Bootstrap.Extenstions;
using Franz.Common.Messaging.Kafka.Extensions;

namespace Franz.Consumer
{
  internal class Startup<TDbContext>
      where TDbContext : DbContextBase
  {
    private readonly IHostEnvironment hostEnvironment;
    private readonly IConfiguration configuration;

    public Startup(IHostEnvironment hostEnvironment, IConfiguration configuration)
    {
      this.hostEnvironment = hostEnvironment;
      this.configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services
          .AddMessagingArchitecture(hostEnvironment, configuration)
          .AddMessaging(configuration)
          .AddSqlServerDatabase<TDbContext>(configuration);
    }
  }
}
