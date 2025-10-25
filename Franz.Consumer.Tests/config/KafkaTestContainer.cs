using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Franz.Consumer.Tests.config
{
  public class KafkaTestContainer : IAsyncDisposable
  {
    private readonly IContainer _kafkaContainer;

    public string BootstrapServers => "localhost:9093"; // Map Kafka's internal port to the host.

    public KafkaTestContainer()
    {
      // Correct usage of TestcontainersBuilder<T>
      _kafkaContainer = new ContainerBuilder()
          .WithImage("confluentinc/cp-kafka:latest") // Use the Kafka Docker image
          .WithName($"kafka-test-{Guid.NewGuid()}") // Unique container name
          .WithEnvironment("KAFKA_BROKER_ID", "1") // Required Kafka settings
          .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "localhost:2181")
          .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://localhost:9093")
          .WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://0.0.0.0:9093")
          .WithPortBinding(9093, true) // Map Kafka's internal port to a random host port
          .WithWaitStrategy(Wait.ForUnixContainer()) // Wait until Kafka is ready
          .Build();
    }

    public async Task StartAsync()
    {
      await _kafkaContainer.StartAsync(); // Start the Kafka container
    }

    public async Task StopAsync()
    {
      await _kafkaContainer.StopAsync(); // Stop the Kafka container
    }

    public async ValueTask DisposeAsync()
    {
      await _kafkaContainer.DisposeAsync(); // Dispose of the container
    }
  }
}
