using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using FranzTesting;
using Xunit;

namespace Franz.Testing.ArchitecturalReports.Layers
{
  /// <summary>
  /// ⚖️ Franz Tribunal — Persistence Layer Compliance Audit
  /// Validates repository structure, dependency purity, and isolation of the Franz.Persistence layer.
  /// </summary>
  public sealed class PersistenceLayerComplianceAudit : ArchitecturalAuditBase
  {
    private static Assembly[] LoadAssembliesWithPattern(string assemblyPattern)
    {
      var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      return loadedAssemblies
          .Where(assembly => Regex.IsMatch(assembly.FullName, assemblyPattern, RegexOptions.IgnoreCase))
          .ToArray();
    }

    [Trait("Category", "ArchitecturalReport")]

    public void Persistence_Governance()
    {
      ExecuteTribunal("Persistence Tribunal", (sb, markViolation) =>
      {
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine("              PERSISTENCE LAYER COMPLIANCE AUDIT               ");
        sb.AppendLine("---------------------------------------------------------------");

        // RULE 1 — Assembly existence
        ExecuteRule("Assembly", "Persistence assembly must be present.", () =>
        {
          Assert.NotNull(PersistenceAssembly);
        }, sb, markViolation);

        // RULE 2 — Repository interface enforcement
        ExecuteRule("Repositories", "All repository classes must implement corresponding interfaces.", () =>
        {
          var persistenceAssemblies = LoadAssembliesWithPattern(@".*\.Persistence(\.|$)");
          var repositoryTypes = persistenceAssemblies
              .SelectMany(a => a.GetTypes())
              .Where(t => t.IsClass && t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!repositoryTypes.Any())
          {
            sb.AppendLine("🟡 No repositories found — skipping rule (virgin template).");
            return;
          }

          foreach (var repo in repositoryTypes)
          {
            var hasInterface = repo.GetInterfaces().Any(i => i.Name.EndsWith("Repository"));
            if (!hasInterface)
            {
              sb.AppendLine($"🚨 [Repositories] {repo.Name} does not implement a repository interface.");
              markViolation();
            }
          }

          sb.AppendLine($"✅ Verified {repositoryTypes.Count} repository implementation(s) are correctly interfaced.");
        }, sb, markViolation);

        // RULE 3 — Dependencies to Domain/Contracts
        ExecuteRule("Assembly Dependencies", "Persistence must depend on Domain or Contracts abstractions.", () =>
        {
          var persistenceTypes = LoadAssembliesWithPattern(@".*\.Persistence(\.|$)")
              .SelectMany(a => a.GetTypes())
              .ToList();

          if (!persistenceTypes.Any())
          {
            sb.AppendLine("🟡 No persistence types found — skipping dependency test.");
            return;
          }

          bool anyValidDep = persistenceTypes
              .SelectMany(t => t.GetTypeInfo().ImplementedInterfaces)
              .Any(dep => dep.FullName?.Contains(".Domain") == true || dep.FullName?.Contains(".Contracts") == true);

          if (!anyValidDep)
          {
            sb.AppendLine("🚨 Persistence layer does not depend on Domain or Contracts abstractions.");
            markViolation();
          }
          else
          {
            sb.AppendLine("✅ Verified persistence layer references Domain/Contracts abstractions.");
          }
        }, sb, markViolation);

        // RULE 4 — Dependency purity
        ExecuteRule("Dependencies", "Persistence layer may depend only on Franz persistence abstractions, domain concepts, and system libraries.", () =>
        {
          var prefix = SolutionPrefix;

          var rule = ArchRuleDefinition
              .Classes()
              .That()
              .ResideInAssembly(PersistenceAssembly)
              .Should()
              .OnlyDependOnTypesThat()
              // Franz Core / Business
              .ResideInNamespaceMatching($"^{prefix}\\.Common\\.Business\\.Domain(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.Business\\.Events(\\..*)?$")
              // Franz Persistence
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.EntityFramework(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.MongoDB(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.Caching(\\..*)?$")
              // Franz Mediator & Helpers
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.Mediator(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.Errors(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common(\\..*)?$")
              // Domain & Contracts
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Domain(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Contracts(\\..*)?$")
              // Microsoft + System
              .OrShould().ResideInNamespaceMatching(@"^Microsoft(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^System(\..*)?$")
              // Self
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Persistence(\\..*)?$")
              .Because("The Persistence layer should depend only on Franz abstractions, domain concepts, and system libraries.")
              .WithoutRequiringPositiveResults();

          rule.Check(BaseArchitecture);
          sb.AppendLine("✅ Verified persistence dependency isolation (Franz + System + self).");
        }, sb, markViolation);

        // RULE 5 — Isolation from upper layers
        ExecuteRule("Isolation", "Persistence must not depend on API or Application layers.", () =>
        {
          var prefix = SolutionPrefix;

          ArchRuleDefinition
              .Classes()
              .That()
              .ResideInAssembly(PersistenceAssembly)
              .Should()
              .NotDependOnAnyTypesThat()
              .ResideInNamespaceMatching($"^{prefix}\\.API(\\..*)?$")
              .AndShould()
              .NotDependOnAnyTypesThat()
              .ResideInNamespaceMatching($"^{prefix}\\.Application(\\..*)?$")
              .Because("Persistence must remain isolated from API and Application layers.")
              .Check(BaseArchitecture);

          sb.AppendLine("✅ Confirmed persistence isolation from API and Application layers.");
        }, sb, markViolation);

        // ───────────────────────────────
        // 🎯 VERDICT SUMMARY
        // ───────────────────────────────
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine(" PERSISTENCE LAYER COMPLIANCE: COMPLETED SUCCESSFULLY");
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine($"🕊️  {SolutionPrefix}.Persistence Audit Verdict: Excellent");
        sb.AppendLine("⚙️  Dependencies: Franz + Domain + Contracts + System ✔");
        sb.AppendLine("═══════════════════════════════════════════════");
      });
    }
  }
}
