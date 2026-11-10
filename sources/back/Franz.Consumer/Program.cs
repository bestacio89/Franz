using Franz.Application;
using Franz.Common.Business.Extensions;
using Franz.Common.Http.EntityFramework.Extensions;
using Franz.Common.Messaging.Bootstrap.Extenstions;
using Franz.Common.Messaging.Hosting.Kafka;
using Franz.Common.Messaging.Kafka.Extensions;
using Franz.Common.Messaging.MultiTenancy.Extensions;
using Franz.Domain.Entities;
using Franz.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Franz.Consumer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var environment = builder.Environment;
        var configuration = builder.Configuration;

        // Register the domain and the same source-generated application composition
        // used by the API host. Do not mix V1 reflection dispatch with V2 registration.
        builder.Services.AddBusiness(typeof(Book).Assembly);
        builder.Services.RegisterApplicationServices();

        // Provider-neutral database wiring; select the engine and isolation mode through configuration.
        builder.Services.AddRelationalDatabase<ApplicationDbContext>(environment, configuration);
        builder.Services.RegisterPersistenceServices(configuration);

        // Register messaging contracts, Kafka transport, and host-owned listeners.
        builder.Services.AddMessagingArchitecture(environment, configuration);
        builder.Services.AddMessagingMultitenancyContext(options =>
            configuration.GetSection("Messaging:Tenant").Bind(options));
        builder.Services.AddKafkaMessaging(configuration);
        builder.Services.AddKafkaHostedListener(
            options => configuration.GetSection("Messaging:Kafka").Bind(options));
        builder.Services.AddOutboxHostedListener(
            options => configuration.GetSection("Messaging:Outbox").Bind(options));

        using var host = builder.Build();
        await host.RunAsync();
    }
}
