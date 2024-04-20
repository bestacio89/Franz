using System;
using System.Linq;
using System.Reflection;

namespace Franz.API.Helpers
{
  public class ApiAssemblyLoader
  {
    public Assembly LoadApplicationAssembly()
    {
      try
      {
        // Assuming your application assembly follows a naming convention like "YourProject.Application".
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Find the assembly whose name ends with ".Application".
        Assembly applicationAssembly = assemblies.FirstOrDefault(a => a.FullName.EndsWith(".Application"));

        return applicationAssembly;
      }
      catch (Exception ex)
      {
        // Handle any exceptions that may occur during assembly loading.
        // You can log the exception details or take appropriate action.
        return null; // Return null if the assembly couldn't be loaded.
      }
    }

    public Assembly LoadPersistenceAssembly()
    {
      try
      {
        // Assuming your persistence assembly follows a naming convention like "YourProject.Persistence".
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Find the assembly whose name ends with ".Persistence".
        Assembly persistenceAssembly = assemblies.FirstOrDefault(a => a.FullName.EndsWith(".Persistence"));

        return persistenceAssembly;
      }
      catch (Exception ex)
      {
        // Handle any exceptions that may occur during assembly loading.
        // You can log the exception details or take appropriate action.
        return null; // Return null if the assembly couldn't be loaded.
      }
    }
  }
}
