using Franz.Common.Messaging;
using Franz.Common.Business.Domain;
using Newtonsoft.Json; // Example for JSON deserialization; use your preferred method.
using Microsoft.Extensions.Logging;
using Franz.Common.Messaging.Messages;

namespace Franz.Consumer.Processing
{
  public class GenericMessageProcessor<T> : IGenericMessageProcessor<T> where T : class, IEntity
  {
    private readonly ILogger<GenericMessageProcessor<T>> _logger;

    public GenericMessageProcessor(ILogger<GenericMessageProcessor<T>> logger)
    {
      _logger = logger;
    }

    public void Process(Message message)
    {
      try
      {
        _logger.LogInformation("Processing message for entity type {EntityType}.", typeof(T).Name);

        // Deserialize the message body into the target entity type
        var entity = JsonConvert.DeserializeObject<T>(message.Body);
        if (entity == null)
        {
          _logger.LogWarning("Message body could not be deserialized to type {EntityType}.", typeof(T).Name);
          return;
        }

        // Validate the entity (optional, based on your requirements)
        ValidateEntity(entity);

        // Process the entity
        ProcessEntity(entity);

        _logger.LogInformation("Successfully processed message for entity type {EntityType}.", typeof(T).Name);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while processing message for entity type {EntityType}.", typeof(T).Name);
        throw; // Optionally rethrow to trigger retry or DLQ.
      }
    }

    private void ValidateEntity(T entity)
    {
      // Add validation logic, if needed
      _logger.LogDebug("Validating entity: {@Entity}", entity);
    }

    private void ProcessEntity(T entity)
    {
      // Business logic for the entity
      _logger.LogInformation("Processing entity: {@Entity}", entity);

      // Example: Save entity to the database, send further messages, etc.
    }
  }
}
