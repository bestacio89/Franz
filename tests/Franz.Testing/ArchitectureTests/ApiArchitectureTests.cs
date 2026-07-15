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
    // Use the ApiLayer provider which is already filtered by namespace in your base class
    var controllers = ApiLayer.GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (!controllers.Any())
    {
      Console.WriteLine("🟡 No Controllers found — skipping.");
      return;
    }

    // Apply the rule ONLY to the types retrieved from the API Layer
    ArchRuleDefinition
        .Classes()
        .That()
        .Are(controllers) // Explicitly restrict the search space
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
            t.Name.StartsWith("I", StringComparison.Ordinal) ||
            t.Name.EndsWith("Command", StringComparison.Ordinal) ||
            t.Name.EndsWith("Query", StringComparison.Ordinal))
        .ToList();

    if (!contractObjects.Any())
    {
      Console.WriteLine("🟡 No contract interfaces or message definitions found — skipping enforcement (template mode).");
      return;
    }

    var apiClasses = BaseArchitecture
        .Classes
        .Where(t => t.Assembly.Name.Equals("Franz.API", StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (!apiClasses.Any())
    {
      Console.WriteLine("🟡 No API layer types found — skipping dependency validation.");
      return;
    }

    // Build rule — pass allowed assemblies as separate params
    ArchRuleDefinition
      .Classes()
      .That()
      .ResideInAssembly("Franz.API")
      .And()
      .DoNotHaveNameEndingWith("Program")
      .And()
      .DoNotHaveNameEndingWith("Startup")
      .Should()
      .OnlyDependOnTypesThat()
      .ResideInAssembly(
          "Franz.API"       
      )
      .OrShould()
      .ResideInAssembly(
          "Franz.Contracts")
      .OrShould()
      .ResideInAssembly(
          "Franz.Common")
      .Because("API components (except composition root) should depend only on Contracts and Common abstractions.")
      .WithoutRequiringPositiveResults()
      .Check(BaseArchitecture);
  }




}
