using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;

using ArchUnitNET.xUnit;

using Franz.Common.Business.Domain;

using Franz.Common.DependencyInjection;
using Franz.Common.Mediator.Messages;
using FranzTesting;
using Interface = System.Reflection.TypeInfo;

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
          .NotBeAssignableTo(typeof(IReadRepository<>))
          .OrShould()
          .NotBeAssignableTo(typeof(IAggregateRepository<,>))
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
          .AreNot(typeof(IReadRepository<>)) // Exclude IReadRepository
          .And().AreNot(typeof(IAggregateRepository<,>)) // Exclude IAggregateRepository
          .And().HaveNameEndingWith("Repository") // Naming convention for custom repositories
          .Should()
          .ImplementAnyInterfacesThat()
          .HaveFullNameMatching(typeof(IScopedDependency).FullName!)
          // Enforce IScopedDependency
          .Because("Custom persistence interfaces should follow lifetime management using IScopedDependency")
          .Check(BaseArchitecture);
    }
  }
}

