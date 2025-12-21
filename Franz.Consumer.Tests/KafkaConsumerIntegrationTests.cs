using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Franz.Common.Messaging;
using Franz.Common.Messaging.Configuration;
using Franz.Common.Messaging.Delegating;
using Franz.Common.Messaging.Messages;
using Franz.Consumer.Services;
using Franz.Consumer.Tests.config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Franz.Consumer.Tests
{
  public class KafkaConsumerIntegrationTests : IAsyncLifetime
  {
    private readonly KafkaTestContainer _kafkaContainer;
    private readonly string _topicName;

    public KafkaConsumerIntegrationTests()
    {
      _kafkaContainer = new KafkaTestContainer();
      _topicName = $"test-topic-{Guid.NewGuid()}";
    }

    public async Task InitializeAsync()
    {
      await _kafkaContainer.StartAsync();
      await CreateTopicAsync(_topicName);
    }

    public async Task DisposeAsync()
    {
      await _kafkaContainer.DisposeAsync();
    }

    [Fact(Skip = "Skipped in CI environment")]
    [Trait("Category", "Integration")]   // 👈 Mark this as Integration
    public async Task KafkaConsumer_ShouldProcessMessage()
    {
      // Arrange
      var services = new ServiceCollection();
      var messageProcessedEvent = new ManualResetEvent(false);

      services.AddLogging();
      services.AddSingleton<IMessageHandler>(provider =>
          new TestMessageHandler(messageProcessedEvent));
      services.AddSingleton<IHostedService, KafkaConsumerService>();
      services.Configure<MessagingOptions>(options =>
      {
        options.BootStrapServers = _kafkaContainer.BootstrapServers;
        options.GroupID = "test-consumer-group";
      });

      var serviceProvider = services.BuildServiceProvider();

      // Act
      var host = serviceProvider.GetRequiredService<IHostedService>() as KafkaConsumerService;

      var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
      await host!.StartAsync(cancellationTokenSource.Token);

      await ProduceMessageAsync(_topicName, "test-key", "{\"Id\":123,\"Name\":\"Test\"}");

      Assert.True(messageProcessedEvent.WaitOne(TimeSpan.FromSeconds(5)),
          "Message was not processed in time");

      await host.StopAsync(cancellationTokenSource.Token);
    }

    private async Task CreateTopicAsync(string topicName)
    {
      var adminConfig = new AdminClientConfig { BootstrapServers = _kafkaContainer.BootstrapServers };
      using var adminClient = new AdminClientBuilder(adminConfig).Build();
      await adminClient.CreateTopicsAsync(new[]
      {
        new TopicSpecification { Name = topicName, NumPartitions = 1, ReplicationFactor = 1 }
      });
    }

    private async Task ProduceMessageAsync(string topicName, string key, string value)
    {
      var producerConfig = new ProducerConfig { BootstrapServers = _kafkaContainer.BootstrapServers };
      using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
      await producer.ProduceAsync(topicName,
          new Message<string, string> { Key = key, Value = value });
    }

    private class TestMessageHandler : IMessageHandler
    {
      private readonly ManualResetEvent _messageProcessedEvent;

      public TestMessageHandler(ManualResetEvent messageProcessedEvent)
      {
        _messageProcessedEvent = messageProcessedEvent;
      }

      public void Process(Message message)
      {
        Assert.Equal("{\"Id\":123,\"Name\":\"Test\"}", message.Body);
        _messageProcessedEvent.Set();
      }
    }
  }
}
