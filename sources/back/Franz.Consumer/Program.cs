using Franz.Common.Http.EntityFramework.Extensions;
using Franz.Common.Messaging.Bootstrap.Extensions;
using Franz.Common.Messaging.Hosting.Kafka.Extensions;
using Franz.Common.Messaging.Kafka.Extensions;
using Franz.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Franz.Consumer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var environment = builder.Environment;
        var configuration = builder.Configuration;

        // Provider-neutral database wiring; select the engine through configuration.
        builder.Services.AddRelationalDatabase<ApplicationDbContext>(environment, configuration);
        builder.Services.RegisterPersistenceServices<ApplicationDbContext>(configuration);

        // Register the shared messaging contracts, Kafka transport, and host-owned listeners.
        builder.Services.AddMessagingArchitecture(environment, configuration);
        builder.Services.AddKafkaMessaging(configuration);
        builder.Services.AddKafkaHostedListener(configuration);
        builder.Services.AddOutboxHostedListener(configuration);

        using var host = builder.Build();
        await host.RunAsync();
    }
}
