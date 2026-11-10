using Franz.Common.Business.Domain;
using Franz.Common.DependencyInjection;
using Franz.Common.Messaging;
using Franz.Common.Messaging.Messages;

namespace Franz.Consumer.Processing
{
  public interface IGenericMessageProcessor<T> where T : IEntity
  {
    void Process(Message message);
  }
}
