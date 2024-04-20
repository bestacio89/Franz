using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Franz.Testing.ArchitectureTests;
public class PersistenceArchitectureTests : BaseArchitectureTest
{
  private Assembly[] LoadAssembliesWithPattern(string assemblyPattern)
  {
    var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
    var matchingAssemblies = loadedAssemblies
        .Where(assembly => Regex.IsMatch(assembly.FullName, assemblyPattern))
        .ToArray();

    return matchingAssemblies;
  }

  [Fact]
  public void PersistenceAssemblyExistence()
  {
    Assert.NotNull(PersistenceAssembly);
  }

  [Fact]
  public void PersistenceAssemblyDependencies()
  {
    var persistenceTypes = LoadAssembliesWithPattern(@".*\.Persistence\..*")
        .SelectMany(assembly => assembly.GetTypes());

    foreach (var type in persistenceTypes)
    {
      var dependencies = type.GetTypeInfo().ImplementedInterfaces;
      Assert.True(dependencies.Any(dep => dep.FullName.EndsWith("Domain.dll") || dep.FullName.EndsWith("Contracts.dll")));
    }
  }

  [Fact]
  public void DependencyCheck_IsCorrect()
  {

    ArchRuleDefinition
      .Types()
      .That()
      .Are(PersistenceLayer)
      .And()
      .DoNotResideInAssembly("Franz.Domain.dll")
      .Should()
      .ResideInNamespace("Franz.Common.Business.Domain")
      .OrShould()
      .ResideInNamespace("Franz.Common.Business.Events")
      .OrShould()
      .ResideInNamespace("Franz.Common.EntityFramework")
      .OrShould()
      .ResideInNamespace("Franz.Common.EntityFramework.Repositories")
      .OrShould()
      .ResideInNamespace("Franz.Common.EntityFramework.Extensions")
      .OrShould()
      .ResideInNamespace("Franz.Common.EntityFramework.Configuration")
      .OrShould()
      .ResideInNamespace("Franz.Common.MongoDB.config")
      .OrShould()
      .ResideInNamespace("Franz.Common.EntityFramework.Behaviors")
      .OrShould()
      .ResideInNamespace("Franz.Common.EntityFramework.Properties")
      .OrShould()
      .ResideInNamespace("Microsoft.Extensions.DependencyInjection")
      .OrShould()
      .ResideInNamespace("")
      .OrShould()
      .ResideInNamespace("MediatR")
      .OrShould()
      .ResideInNamespace("System")
      .Check(BaseArchitecture);

  }




  [Fact]
  public void RepositoryImplementationsExist()
  {
    var persistenceTypes = LoadAssembliesWithPattern(@".*\.Persistence\..*")
        .SelectMany(assembly => assembly.GetTypes());

    foreach (var type in persistenceTypes)
    {
      if (type.Name.EndsWith("Repository") && type.IsClass)
      {
        // Check for implementation of a repository interface
        var repositoryInterface = type.GetInterfaces()
            .FirstOrDefault(interf => interf.Name.EndsWith("Repository"));

        Assert.NotNull(repositoryInterface);
      }
    }
  }
}
