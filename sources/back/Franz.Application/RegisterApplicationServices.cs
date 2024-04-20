using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Franz.Application;
public static class RegisterApplicationServices
{
  public static IServiceCollection RegisterServices(this IServiceCollection collection)
  {

    collection.AddMediator(Assembly.GetAssembly(typeof(RegisterApplicationServices)));
    return collection;
  }

}
