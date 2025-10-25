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

namespace Franz.Testing.ArchitecturalReports.Layers
{
  /// <summary>
  /// API Layer Compliance Audit —
  /// Validates controller conventions, dependency isolation, and namespace discipline
  /// within the Franz API boundary.
  /// </summary>
  public class ApiLayerComplianceAudit : ArchitecturalAuditBase
  {
    [Trait("Category", "ArchitecturalReport")]

    public void Audit_ApiLayer_Compliance()
    {
      ExecuteTribunal("API Layer Compliance Audit", (sb, markViolation) =>
      {
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine("                API LAYER COMPLIANCE AUDIT                     ");
        sb.AppendLine("---------------------------------------------------------------");

        // RULE 1 — Assembly presence
        ExecuteRule("Assembly Presence", "API assembly must be present and accessible.", () =>
        {
          Assert.NotNull(ApiAssembly);
          sb.AppendLine("✅ Verified: API assembly detected.");
        }, sb, markViolation);

        // RULE 2 — Controller conventions
        ExecuteRule("Controller Conventions", "Controllers must end with 'Controller' and reside under Franz.API.Controllers.", () =>
        {
          var controllers = ApiLayer
              .GetObjects(BaseArchitecture)
              .Where(t => t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!controllers.Any())
          {
            sb.AppendLine("🟡 No controllers found — skipping enforcement (empty template).");
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
              .Because("Controllers must reside in Franz.API.Controllers and derive from ControllerBase.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Verified {controllers.Count} controller(s) comply with naming and inheritance conventions.");
        }, sb, markViolation);

        // RULE 3 — Dependency isolation
        ExecuteRule("Dependency Isolation", "Controllers may depend only on Contracts, Common abstractions, and framework namespaces.", () =>
        {
          var controllers = ApiLayer
              .GetObjects(BaseArchitecture)
              .Where(t => t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!controllers.Any())
          {
            sb.AppendLine("🟡 No controllers found — skipping dependency analysis (empty template).");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(controllers)
              .Should()
              .OnlyDependOnTypesThat()
              // Franz internal abstractions
              .ResideInNamespaceMatching(@"^Franz\.Contracts(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Common(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Common\.Mediator(\..*)?$")
              // Framework namespaces
              .OrShould().ResideInNamespaceMatching(@"^System(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Microsoft(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Microsoft\.AspNetCore(\..*)?$")
              // Local API namespace
              .OrShould().ResideInNamespaceMatching(@"^Franz\.API(\..*)?$")
              .Because("Controllers must not depend on Domain, Application, or Persistence layers.")
              .WithoutRequiringPositiveResults()
              .Check(BaseArchitecture);

          sb.AppendLine("✅ Verified API controllers maintain clean dependency boundaries.");
        }, sb, markViolation);

        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine(" API LAYER COMPLIANCE: COMPLETED SUCCESSFULLY");
        sb.AppendLine("---------------------------------------------------------------");
      });
    }
  }
}
