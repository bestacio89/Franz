using Franz.Common.Messaging;
using Franz.Common.Messaging.Delegating;
using Franz.Common.Messaging.Headers;
using Franz.Consumer.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Franz.Consumer.Handlers
{
  public class KafkaMessageHandler : IMessageHandler
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaMessageHandler> _logger;

    public KafkaMessageHandler(IServiceProvider serviceProvider, ILogger<KafkaMessageHandler> logger)
    {
      _serviceProvider = serviceProvider;
      _logger = logger;
    }

    public void Process(Message message)
    {
      try
      {
        _logger.LogInformation("Processing message with headers: {@Headers}", message.Headers);

        // Determine the target entity type (e.g., from headers)
        var entityType = GetEntityTypeFromHeaders(message.Headers);
        if (entityType == null)
        {
          _logger.LogWarning("Entity type could not be determined from message headers.");
          return;
        }

        // Resolve the appropriate generic processor for the entity type
        var processorType = typeof(IGenericMessageProcessor<>).MakeGenericType(entityType);
        dynamic processor = _serviceProvider.GetRequiredService(processorType);

        // Process the message
        processor.Process(message);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while processing the message.");
        throw;
      }
    }

    private Type GetEntityTypeFromHeaders(MessageHeaders headers)
    {
      // Extract entity type information from headers (custom logic)
      if (headers.TryGetValue("EntityType", out var entityTypeHeader))
      {
        return Type.GetType(entityTypeHeader.ToString());
      }

      return null;
    }
  }
}
