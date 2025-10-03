using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Franz.Common.Messaging.Kafka;
using Franz.Consumer.Services;
using Franz.Common.Messaging.Kafka.Extensions;

namespace Franz.Consumer.Extensions
{
  public static class KafkaSetupExtensions
  {
    public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
    {
      // Register Kafka-related services
      services.AddMessaging(configuration);
      services.AddHostedService<KafkaConsumerService>();
      return services;
    }
  }
}
