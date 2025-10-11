using System;
using System.Linq;
using System.Text;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Mediator.Messages;
using FranzTesting;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ControllerBase = Microsoft.AspNetCore.Mvc.ControllerBase;

namespace Franz.Testing.ArchitectureTests.Tribunals
{
  /// <summary>
  /// ⚖️ Franz Tribunal — API Layer Governance
  /// Enforces controller conventions and ensures contracts are properly referenced.
  /// </summary>
  public class ApiArchitectureTribunal : TribunalBase
  {
    [Fact(DisplayName = "⚖️ API Tribunal — Controller Governance & Namespace Discipline")]
    public void Api_Governance()
    {
      ExecuteTribunal("API Tribunal", (sb, markViolation) =>
      {
        // RULE 1 — Ensure API assembly exists
        ExecuteRule("API", "API assembly not found.", () =>
        {
          Assert.NotNull(ApiLayer);
        }, sb, markViolation);

        // RULE 2 — Controller naming & location
        ExecuteRule("Controllers", "Controllers must end with 'Controller' and reside in Franz.API.Controllers.", () =>
        {
          var apiObjects = ApiLayer
              .GetObjects(BaseArchitecture)
              .Where(t => t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!apiObjects.Any())
          {
            sb.AppendLine("🟡 No Controllers found — skipping enforcement (virgin template).");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .HaveNameEndingWith("Controller")
              .Should()
              .BeAssignableTo(typeof(ControllerBase))
              .AndShould()
              .ResideInNamespace("Franz.API.Controllers", true)
              .Because("Controllers must live within the API Controllers namespace and inherit ControllerBase.")
              .Check(BaseArchitecture);
        }, sb, markViolation);

        // RULE 3 — Dependency to Contracts layer
        ExecuteRule("Contracts", "Controllers should depend only on Commands/Queries from Contracts layer.", () =>
        {
          var contractObjects = ContractsLayer
              .GetObjects(BaseArchitecture)
              .Where(t =>
                  t.Name.StartsWith("I") ||
                  t.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase) ||
                  t.Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!contractObjects.Any())
          {
            sb.AppendLine("🟡 No contracts (commands/queries) found — skipping enforcement (virgin template).");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(ApiLayer)
              .And()
              .HaveNameEndingWith("Controller")
              .Should()
              .OnlyDependOnTypesThat()
              .ResideInNamespace("Franz.Contracts", true)
              .OrShould()
              .ResideInNamespace("System", true)
              .OrShould()
              .ResideInNamespace("Microsoft", true)
              .Because("API should depend only on contracts, not internal application logic.")
              .Check(BaseArchitecture);
        }, sb, markViolation);
      });
    }
  }
}
