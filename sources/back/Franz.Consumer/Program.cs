using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Franz.Persistence;
using Franz.Common.Business.Events;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Franz.Consumer
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      // Initialize configuration
      var configuration = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json") // Adjust the path to your app settings JSON
          .Build();

      // Create the host builder
      var hostBuilder = CreateHostBuilder(args);

      // Build and run the host
      hostBuilder.Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      // Create the host builder and configure services
      var hostBuilder = Host.CreateDefaultBuilder(args)
          .ConfigureServices((hostContext, services) =>
          {
            // Add application services, configurations, etc.
            services.AddDbContext<ApplicationDbContext>(); // Replace with your actual DbContext

            // Retrieve the specific configuration section required by AddMessaging
            var messagingConfig = hostContext.Configuration.GetSection("Messaging");
            services.AddMessaging(messagingConfig); // Pass the configuration section to AddMessaging
          });

      return hostBuilder;
    }
  }
}
