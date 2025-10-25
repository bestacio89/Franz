using Franz.Common.EntityFramework.SQLServer.Extensions;
using Franz.Common.Messaging.Bootstrap.Extenstions;
using Franz.Common.Messaging.Delegating;

using Franz.Consumer.Extensions;
using Franz.Consumer.Handlers;

using Franz.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Franz.Common.Messaging.Kafka.Extensions;
namespace Franz.Consumer
{
  public static class Program
  {
    public static async Task Main(string[] args)
    {
      using IHost host = Host.CreateDefaultBuilder(args)
          .ConfigureAppConfiguration((context, config) =>
          {
            // Load base configuration
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables();
          })
          .ConfigureServices((context, services) =>
          {
            var configuration = context.Configuration;
            var environment = context.HostingEnvironment;

            // 🧩 Messaging architecture setup (Kafka + Delegating)
            services.AddMessagingArchitecture(environment, configuration);
            services.AddKafkaMessaging(configuration);

            // 🗄️ Database setup (SQL Server)
            services.AddSqlServerDatabase<ApplicationDbContext>(configuration);

            // 🧠 Business domain and consumer services
            services.AddScoped<IMessageHandler, KafkaMessageHandler>();
            services.AddKafkaMessagingServices(configuration); // from Franz.Consumer.Extensions
          })
          .Build();

      // ✅ Start host (graceful lifetime)
      await host.RunAsync();
    }
  }
}
