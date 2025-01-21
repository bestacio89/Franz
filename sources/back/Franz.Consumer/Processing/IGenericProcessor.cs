using Franz.Common.Business.Domain;
using Franz.Common.DependencyInjection;
using Franz.Common.Messaging;

namespace Franz.Consumer.Processing
{
  public interface IGenericMessageProcessor<T> where T : IEntity
  {
    void Process(Message message);
  }
}
