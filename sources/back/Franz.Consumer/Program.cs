using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Franz.Common.Business.Domain;
using Franz.Common.Messaging.Kafka;
using Franz.Common.EntityFramework.SQLServer.Extensions;
using Franz.Consumer.Extensions;
using Franz.Consumer.Services;
using Franz.Persistence;
using Franz.Common.Messaging.Delegating;
using Franz.Consumer.Handlers;
using Franz.Common.Messaging.Bootstrap.Extenstions;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
      // Add configuration files
      config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
      config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
    })
    .ConfigureServices((context, services) =>
    {
      var configuration = context.Configuration;
      var environment = context.HostingEnvironment;

      // Messaging architecture (Kafka and related services)
      services.AddMessagingArchitecture(environment, configuration);
      services.AddKafkaMessaging(configuration);

      // Database setup (SQL Server)
      services.AddSqlServerDatabase<ApplicationDbContext>(configuration);

      // Add additional application services if needed
      services.AddScoped<IMessageHandler, KafkaMessageHandler>();
    })
    .Build();

// Run the host
await builder.RunAsync();
