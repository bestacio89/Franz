using Franz.Common.Messaging.Kafka;
using Franz.Common.Messaging.Delegating;
using Franz.Common.Messaging.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Franz.Common.Messaging;

namespace Franz.Consumer.Services
{
  public class KafkaConsumerService : IHostedService
  {
    private readonly Confluent.Kafka.IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IMessageHandler _messageHandler;
    private readonly string _topicName;

    public KafkaConsumerService(
        IOptions<MessagingOptions> messagingOptions,
        IMessageHandler messageHandler,
        ILogger<KafkaConsumerService> logger)
    {
      _logger = logger;
      _messageHandler = messageHandler;
      _topicName = TopicNamer.GetTopicName(typeof(KafkaConsumerService).Assembly);

      var consumerConfig = new ConsumerConfig
      {
        BootstrapServers = messagingOptions.Value.BootStrapServers,
        GroupId = messagingOptions.Value.GroupID,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
      };

      _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("Starting Kafka Consumer Service...");
      _consumer.Subscribe(_topicName);

      Task.Run(() =>
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          try
          {
            var result = _consumer.Consume(cancellationToken);
            _logger.LogInformation($"Message consumed from topic {_topicName}: {result.Message.Value}");

            // Pass the message to the handler
            _messageHandler.Process(new Message(result.Message.Key, result.Message.Value));
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error occurred while consuming messages.");
          }
        }
      }, cancellationToken);

      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("Stopping Kafka Consumer Service...");
      _consumer.Close();
      return Task.CompletedTask;
    }
  }
}
