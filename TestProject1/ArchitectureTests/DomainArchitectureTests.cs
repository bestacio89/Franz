using Xunit;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ArchUnitNET.Fluent;
using Franz.Common.Business.Domain;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Events;
using MediatR;
using Franz.Common.Mediator.Messages;

namespace Franz.Testing.ArchitectureTests;

public class DomainArchitectureTests : BaseArchitectureTest
{


  [Fact]
  public void Entities_InheritCorrectly()
  {
    ArchRuleDefinition
      .Classes()
      .That()
      .ResideInNamespace("Franz.Domain.Entities")
      .Should()
      .BeAssignableTo(typeof(Entity<>))
      .AndShould().NotBeValueTypes()
      .Check(BaseArchitecture);
  }

  [Fact]
  public void Events_AreSetupCorrectly ()
  {
    ArchRuleDefinition
      .Classes()
      .That()
      .ResideInNamespace("Franz.Domain.Events")
      .And()
      .AreAssignableTo(typeof(IEvent))
      .Or().AreAssignableTo(typeof(IDomainEvent))
      .Should()
      .HaveNameEndingWith("Event")
      .Check(BaseArchitecture);

  }


  [Fact]
  public void DomainAssemblyDependencies_AreCorrect()
  {
    ArchRuleDefinition
      .Classes()
      .That()
     .ResideInAssembly(DomainAssembly)
     .Should()
     .DependOnAnyTypesThat()
     .ResideInAssembly(typeof(Entity<>).Assembly.FullName)
     .OrShould()
     .DependOnAnyTypesThat()
     .ResideInNamespace("System")
     .Check (BaseArchitecture);

  }
}

