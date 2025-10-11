using ArchUnitNET;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Mediator.Messages;
using FranzTesting;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ControllerBase =  Microsoft.AspNetCore.Mvc.ControllerBase;

namespace Franz.Testing.ArchitectureTests;
public class ApiArchitectureTests : BaseArchitectureTest
{
  [Fact]
  public void Api_Assembly_Should_Exist()
  {
    Assert.NotNull(ApiLayer); // Leverages the provider from BaseArchitectureTest
  }

  [Fact]
  public void Controllers_AreLocatedCorrectly()
  {
    var apiObjects = ApiLayer
         .GetObjects(BaseArchitecture)
         .Where(t =>
             t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
         .ToList();

    if (!apiObjects.Any())
    {
      Console.WriteLine("🟡 No  Service Intefaces found — skipping contract interface enforcement (virgin template).");
      return;
    }
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
    var contractObjects = ContractsLayer
         .GetObjects(BaseArchitecture)
         .Where(t =>
             t.Name.StartsWith("I") 
             || !t.NameEndsWith("Command") || !t.NameEndsWith("Query"))
         .ToList();

    if (!contractObjects.Any())
    {
      Console.WriteLine("🟡 No  Service Intefaces found — skipping contract interface enforcement (virgin template).");
      return;
    }
    ArchRuleDefinition
    .Classes()
    .That()
    .AreAssignableTo(typeof(ICommand<>))
    .Should()
    .ResideInAssembly("Franz.Contracts")
    .Check(BaseArchitecture);
  }



}
