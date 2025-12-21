using Franz.Common.Messaging.Kafka;
using Franz.Common.Messaging.Delegating;
using Franz.Common.Messaging.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Franz.Common.Messaging;
using System.Text;
using Franz.Common.Messaging.Messages;

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
      _topicName = TopicNamer.GetTopicName((Common.Reflection.IAssembly)typeof(KafkaConsumerService).Assembly);

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

            // Convert Kafka headers to Franz Message headers
            var headers = result.Message.Headers?
                .ToDictionary(
                    header => header.Key,
                    header => new Microsoft.Extensions.Primitives.StringValues(
                        Encoding.UTF8.GetString(header.GetValueBytes() ?? Array.Empty<byte>())
                    )
                ) ?? new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();

            // Create the Franz Message object
            Message franzMessage = new Message(result.Message.Value, (IDictionary<string, IReadOnlyCollection<string>>)headers);

            // Pass the message to the handler
            _messageHandler.Process(franzMessage);
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
