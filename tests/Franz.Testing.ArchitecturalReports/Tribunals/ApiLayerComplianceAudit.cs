using System;
using System.Linq;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using FranzTesting;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ControllerBase = Microsoft.AspNetCore.Mvc.ControllerBase;

namespace Franz.Testing.ArchitecturalReports.Layers
{
  /// <summary>
  /// ⚖️ Franz Tribunal — API Layer Compliance Audit
  /// Industrial-strength validation of controller conventions and dependency isolation.
  /// </summary>
  public sealed class ApiLayerComplianceAudit : ArchitecturalAuditBase
  {

    [Trait("Category", "ArchitecturalReport")]
    public void Audit_ApiLayer_Compliance()
    {
      ExecuteTribunal("API Layer Compliance Audit", (sb, markViolation) =>
      {
        var prefix = SolutionPrefix;

        // RULE 1 — Assembly presence (Manual Check)
        ExecuteRule("Assembly Presence", "API assembly must be detected.", () =>
        {
          Assert.NotNull(ApiAssembly);
        }, sb, markViolation);

        // RULE 2 — Controller conventions
        var controllerNamingRule = ArchRuleDefinition
            .Classes()
            .That().ResideInNamespaceMatching($"^{prefix}\\.API(\\..*)?$")
            .And().HaveNameEndingWith("Controller")
            .Should().BeAssignableTo(typeof(ControllerBase))
            .AndShould().ResideInNamespaceMatching($"^{prefix}\\.API\\.Controllers(\\..*)?$")
            .Because("Controllers must reside in .API.Controllers and derive from ControllerBase for consistent routing.");

        ExecuteRule("Controller Conventions", "Controllers must follow naming and inheritance standards.",
            controllerNamingRule, sb, markViolation);

        // RULE 3 — Dependency isolation (The "Fker" Hunter)
        var isolationRule = ArchRuleDefinition
            .Classes()
            .That().HaveNameEndingWith("Controller")
            .Should().OnlyDependOnTypesThat()
            .ResideInNamespaceMatching($"^{prefix}\\.Contracts(\\..*)?$")
            .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common(\\..*)?$")
            .OrShould().ResideInNamespaceMatching(@"^System(\..*)?$")
            .OrShould().ResideInNamespaceMatching(@"^Microsoft(\..*)?$")
            .OrShould().ResideInNamespaceMatching($"^{prefix}\\.API(\\..*)?$")
            .Because("Controllers are entry points; they must not touch Domain, Application, or Persistence layers directly.");

        ExecuteRule("Dependency Isolation", "Controllers must not bypass the Application layer.",
            isolationRule, sb, markViolation);

        // 🎯 VERDICT SUMMARY logic is now handled by the Base Class ExecuteTribunal
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine($"🕊️  {prefix}.API Audit Final Processing: Complete");
      });
    }
  }
}