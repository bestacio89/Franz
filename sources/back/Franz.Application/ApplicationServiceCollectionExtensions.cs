using Franz.Common.Mapping.Extensions;
using Franz.Common.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace Franz.Application;
public static class ApplicationServiceCollectionExtensions
{
  public static IServiceCollection RegisterApplicationServices(this IServiceCollection collection)
  {


   collection.AddFranzMapping(Assembly.GetExecutingAssembly());
   collection.AddFranzMediatorV2Default(null, Assembly.GetExecutingAssembly());

    return collection;
  }

}
