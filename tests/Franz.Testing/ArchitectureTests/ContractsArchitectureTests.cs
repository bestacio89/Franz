using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;

using ArchUnitNET.xUnit;

using Franz.Common.Business.Domain;

using Franz.Common.DependencyInjection;
using Franz.Common.Mediator.Messages;
using Franz.Common.Business.Repositories;
using System.Reflection;
using Interface = System.Reflection.TypeInfo;
using Xunit;

namespace Franz.Testing.ArchitectureTests
{
  public class ContractsArchitecture : BaseArchitectureTest
  {

    [Fact]
    public void Contracts_Assembly_Should_Exist()
    {
      Assert.NotNull(ContractsLayer); // Leverages the provider from BaseArchitectureTest
    }

   
   
    [Fact]
    public void QueryNameConventionIsCorrect()
    {
      var commandobjects = ContractsLayer.GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        .ToList();

      if (!commandobjects.Any())
      {
        Console.WriteLine("🟡 No Queries found in Contract layer — skipping rule.");
        return;
      }
      ArchRuleDefinition.
      Classes().That().AreAssignableTo(typeof(IQuery<>))
      .Should()
      .HaveNameEndingWith("Query")
      .Check(BaseArchitecture);
    }

    [Fact]
    public void CommandNameConventionIsCorrect()

    {
      var commandobjects = ContractsLayer.GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
        .ToList();

      if (!commandobjects.Any())
      {
        Console.WriteLine("🟡 No Commands found in Contract layer — skipping rule.");
        return;
      }
      ArchRuleDefinition.
      Classes().That().AreAssignableTo(typeof(ICommand<>))
       .Should()
       .HaveNameEndingWith("Command")
       .Check(BaseArchitecture);

    }

    [Fact]
    public void DtosNameConvetionIsCorrect()
    {
     
      var contractObjects = ContractsLayer
          .GetObjects(BaseArchitecture)
          .Where(t =>
             
              !t.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase)
              )
          .ToList();

      if (!contractObjects.Any())
      {
        Console.WriteLine("🟡 No Dtos found — skipping persistence contract enforcement (virgin template).");
        return;
      }
      ArchRuleDefinition.
           Classes().That()
          .ResideInNamespace("Franz.Contracts.DTOS")
          .Should()
          .HaveNameEndingWith("Dto")
          .Because("Dtos should be properly named as such")
          .Check(BaseArchitecture);
    }

    [Fact]
    public void Infraestructure_Interfaces_Follow_Rules()
    {
      var contractObjects = ContractsLayer
          .GetObjects(BaseArchitecture)
          .Where(t =>
              t.Name.StartsWith("I") &&
              !t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase))
           .ToList();

      if (!contractObjects.Any())
      {
        Console.WriteLine("🟡 No Custom Service Intefaces found — skipping persistence contract enforcement (virgin template).");
        return;
      }
      ArchRuleDefinition.
        Classes()
        .That().ResideInNamespace("Franz.Contracts.Infrastructure.*")
        .Should()
        .BeAssignableTo(typeof(IScopedDependency))
        .OrShould().BeAssignableTo(typeof(ISingletonDependency))
        .Because("Every infrastructureInterface requires aproper  lifetime")
        .WithoutRequiringPositiveResults()
        .Check(BaseArchitecture)       ;

    }

    [Fact]
    public void Persistence_Interfaces_Follow_Rules()
    {
      // 🎯 Identify repository interfaces defined in the Domain (contracts or persistence abstractions)
      var contractObjects = ContractsLayer
          .GetObjects(BaseArchitecture)
          .Where(t =>
              t.Name.StartsWith("I") &&
              t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase))
          .ToList();

      if (!contractObjects.Any())
      {
        Console.WriteLine("🟡 No repository interfaces found — skipping persistence contract enforcement (virgin template).");
        return;
      }

      // ✅ Enforce rule: all repository interfaces should conform to Franz repository contracts
      ArchRuleDefinition
          .Classes()
          .That()
          .Are(contractObjects)
          .Should()
          .BeAssignableTo(typeof(IScopedDependency))
          .AndShould()
          .NotBeAssignableTo(typeof(IEntityRepository<,>))
          .OrShould()
          .NotBeAssignableTo(typeof(IAggregateRootRepository<,,>))
          .AndShould()
          .HaveNameEndingWith("Repository")
                 
          .Because("Because repositories are scoped and Custom repos should not implement Generic Framework provisioned implementations")
          .WithoutRequiringPositiveResults()
          .Check(BaseArchitecture);

      Console.WriteLine($"✅ Verified {contractObjects.Count} repository interface(s) follow the Franz persistence contract model.");
    }


    [Fact]
    public void CustomPersistenceInterfaces_Follow_Rules()
    {
      // 🎯 Identify repository interfaces defined in the Domain (contracts or persistence abstractions)
      var contractObjects = ContractsLayer
          .GetObjects(BaseArchitecture)
          .Where(t =>
              t.Name.StartsWith("I") &&
              t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase) &&
              !t.Name.Contains("Aggregate") &&
              !t.Name.Contains("Read") &&
              !t.NameContains("Entity"))

          .ToList();

      if (!contractObjects.Any())
      {
        Console.WriteLine("🟡 No custom repository interfaces found — skipping persistence contract enforcement (virgin template).");
        return;
      }
      // ✅ Enforce rule: all custom repository interfaces should inherit IScopedDependency
      ArchRuleDefinition.Classes()
          .That()
          .AreNot(typeof(IEntityRepository<,>)) // Exclude IReadRepository
          .And().AreNot(typeof(IAggregateRepository<,>)) // Exclude IAggregateRepository
          .And().HaveNameEndingWith("Repository") // Naming convention for custom repositories
          .Should()
          .ImplementAnyInterfacesThat()
          .HaveFullNameMatching(typeof(IScopedDependency).FullName!)
          // Enforce IScopedDependency
          .Because("Custom persistence interfaces should follow lifetime management using IScopedDependency")
          .Check(BaseArchitecture);
    }


    [Fact]
    public void Dtos_Must_Be_Immutable_Or_Records()
    {
      var dtoObjects = ContractsLayer
          .GetObjects(BaseArchitecture)
          .Where(t =>
              t.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) &&
              t.Namespace.FullName.Contains("DTOs", StringComparison.OrdinalIgnoreCase))
          .Where(t =>
              !t.Name.StartsWith("Create", StringComparison.OrdinalIgnoreCase) &&
              !t.Name.StartsWith("Update", StringComparison.OrdinalIgnoreCase) &&
              !t.Name.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) &&
              !t.Name.StartsWith("Configure", StringComparison.OrdinalIgnoreCase))
          .ToList();

      if (!dtoObjects.Any())
      {
        Console.WriteLine("🟡 No immutable DTO contracts found — skipping DTO immutability rule.");
        return;
      }

      var offenders = new List<IType>();

      foreach (var dto in dtoObjects)
      {
        var type = GetReflectionType(dto);

        if (type == null)
          continue;

        // ✅ Records are always compliant
        if (IsRecord(type))
          continue;

        // 🧠 Validate properties:
        // public setters are forbidden unless they are init-only
        bool hasWritableProps = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(property =>
            {
              var setter = property.SetMethod;

              if (setter == null)
                return false;

              bool isInitOnly = setter.ReturnParameter
                  .GetRequiredCustomModifiers()
                  .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));

              return !isInitOnly;
            });

        if (hasWritableProps)
        {
          offenders.Add(dto);
        }
      }

      if (!offenders.Any())
      {
        Console.WriteLine(
            "✅ Immutable DTO contracts confirmed. " +
            "Records and init-only DTOs comply.");
        return;
      }

      Console.WriteLine("🚨 Mutable DTO contracts detected:");

      foreach (var offender in offenders)
      {
        Console.WriteLine($" - {offender.FullName}");
      }

      var rule = ArchRuleDefinition
          .Classes()
          .That()
          .Are(offenders)
          .Should()
          .NotExist()
          .Because(
              "DTO contracts must be immutable records or contain only init-only properties. " +
              "Authoring DTOs must explicitly use Create, Update, Edit, or Configure prefixes.");

      rule.Check(BaseArchitecture);
    }


    private static bool IsRecord(Type type)
    {
      return type.GetMethod(
                 "<Clone>$",
                 BindingFlags.NonPublic | BindingFlags.Instance) != null
          ||
          type.GetMethod(
                 "PrintMembers",
                 BindingFlags.NonPublic | BindingFlags.Instance) != null;
    }


    private static Type? GetReflectionType(IType archType)
    {
      // Dynamic reflection avoids compile-time assumptions
      var typeProperty = archType
          .GetType()
          .GetProperty(
              "Type",
              BindingFlags.Public | BindingFlags.Instance);

      if (typeProperty?.GetValue(archType) is Type systemType)
        return systemType;

      // Fallback resolution
      return Type.GetType(
          archType.FullName,
          throwOnError: false);
    }


  }
}

