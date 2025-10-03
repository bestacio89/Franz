using ArchUnitNET.Domain;
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
      ArchRuleDefinition.
      Classes().That().AreAssignableTo(typeof(IQuery<>))
      .Should()
      .HaveNameEndingWith("Query")
      .Check(BaseArchitecture);
    }

    [Fact]
    public void CommandNameConventionIsCorrect()

    {
      ArchRuleDefinition.
      Classes().That().AreAssignableTo(typeof(ICommand))
       .Should()
       .HaveNameEndingWith("Command")
       .Check(BaseArchitecture);

    }

    [Fact]
    public void DtosNameConvetionIsCorrect()
    {
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
      ArchRuleDefinition.
        Classes()
        .That().ResideInNamespace("Franz.Contracts.Infrastructure.*")
        .Should()
        .BeAssignableTo(typeof(Interface))
        .AndShould()
        .BeAssignableTo(typeof(IScopedDependency))
        .OrShould().BeAssignableTo(typeof(ISingletonDependency))
        .Because("Every infrastructureInterface requires aproper  lifetime")
        .Check(BaseArchitecture);

    }

    [Fact]
    public void Persistence_Interfaces_Follow_Rules()
    {
      ArchRuleDefinition.
        Classes()
        .That().ResideInNamespace("Franz.Contracts.Persistence.*")
        .Should()
        .BeAssignableTo(typeof(IReadRepository<>))
        .OrShould()
        .BeAssignableTo(typeof(IAggregateRepository<,>))
        .AndShould()
        .HaveNameEndingWith("Repository")
        .Because("Generic Microservice Persistence logic is handled in the Read/Aggregate Repositories")
        .Check(BaseArchitecture);
      

    }

    [Fact]
    public void CustomPersistenceInterfaces_Follow_Rules()
    {
      ArchRuleDefinition.Classes()
          .That().Are(typeof(Interface))
          .And().AreNot(typeof(IReadRepository<>)) // Exclude IReadRepository
          .And().AreNot(typeof(IAggregateRepository<,>)) // Exclude IAggregateRepository
          .And().HaveNameEndingWith("Repository") // Naming convention for custom repositories
          .Should()
          .BeAssignableTo(typeof(IScopedDependency)) // Enforce IScopedDependency
          .Because("Custom persistence interfaces should follow lifetime management using IScopedDependency")
          .Check(BaseArchitecture);
    }
  }
}

