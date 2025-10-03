using FranzTesting;
using ArchUnitNET;
using Xunit;
using ArchUnitNET.Fluent;

using ArchUnitNET.xUnit;
using Microsoft.AspNetCore.Mvc;
using ControllerBase =  Microsoft.AspNetCore.Mvc.ControllerBase;
using Franz.Common.Mediator.Messages;

namespace Franz.Testing.ArchitectureTests;
public class ApiArchitectureTests : BaseArchitectureTest
{
  [Fact]
  public void Api_Assembly_Should_Exist()
  {
    Assert.NotNull(APILayer); // Leverages the provider from BaseArchitectureTest
  }

  [Fact]
  public void Controllers_AreLocatedCorrectly()
  {
    ArchRuleDefinition // Get types from the provider
        .Classes()
        .That()
        .HaveNameEndingWith("Controller")
        .Should()
        .BeAssignableTo(typeof(ControllerBase))
        .AndShould()
        .ResideInNamespace("Franz.API.Controllers")
        .Check(BaseArchitecture);

  
  }

  [Fact]
  public void DependencyToContractsExists()
  {
    ArchRuleDefinition
    .Classes()
    .That()
    .AreAssignableTo(typeof(ICommand<>))
    .Should()
    .ResideInAssembly("Franz.Contracts")
    .Check(BaseArchitecture);
  }



}
