using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using FranzTesting;
using Xunit;

namespace Franz.Testing.ArchitectureTests.Tribunals
{
  /// <summary>
  /// ⚖️ Franz Tribunal — Persistence Layer Governance
  /// Ensures repository structure, dependency isolation, and contract adherence in the persistence layer.
  /// </summary>
  public class PersistenceArchitectureTribunal : TribunalBase
  {
    private Assembly[] LoadAssembliesWithPattern(string assemblyPattern)
    {
      var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      return loadedAssemblies
          .Where(assembly => Regex.IsMatch(assembly.FullName, assemblyPattern, RegexOptions.IgnoreCase))
          .ToArray();
    }

    [Fact(DisplayName = "⚖️ Persistence Tribunal — Repository & Dependency Governance")]
    public void Persistence_Governance()
    {
      ExecuteTribunal("Persistence Tribunal", (sb, markViolation) =>
      {
        // RULE 1 — Assembly existence
        ExecuteRule("Assembly", "Persistence assembly not found.", () =>
        {
          Assert.NotNull(PersistenceAssembly);
        }, sb, markViolation);

        // RULE 2 — Repository existence and interface enforcement
        ExecuteRule("Repositories", "All repository classes must implement repository interfaces.", () =>
        {
          var persistenceAssemblies = LoadAssembliesWithPattern(@".*\.Persistence(\.|$)");
          var repositoryTypes = persistenceAssemblies
              .SelectMany(a => a.GetTypes())
              .Where(t => t.IsClass && t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!repositoryTypes.Any())
          {
            sb.AppendLine("🟡 No repositories detected — skipping rule (virgin template).");
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

          if (repositoryTypes.Any())
            sb.AppendLine($"✅ Verified {repositoryTypes.Count} repository implementation(s) exist and are properly interfaced.");
        }, sb, markViolation);

        // RULE 3 — Repository interface dependencies (optional dependency check)
        ExecuteRule("Assembly Dependencies", "Persistence must depend on Domain or Contracts assemblies.", () =>
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
              .Any(dep => dep.FullName?.Contains("Domain") == true || dep.FullName?.Contains("Contracts") == true);

          if (!anyValidDep)
          {
            sb.AppendLine("🚨 Persistence layer does not depend on Domain or Contracts abstractions.");
            markViolation();
          }
          else
          {
            sb.AppendLine("✅ Verified persistence layer has valid dependencies (Domain/Contracts).");
          }
        }, sb, markViolation);

        // RULE 4 — Dependency purity enforcement
        ExecuteRule("Dependencies", "Persistence layer depends on forbidden namespaces.", () =>
        {
          var rule = ArchRuleDefinition
              .Classes()
              .That()
              .ResideInAssembly(PersistenceAssembly)
              .Should()
              .OnlyDependOnTypesThat()
              // Franz Core & Business Layers
              .ResideInNamespaceMatching("Franz.Common.Business.Domain")
              .OrShould().ResideInNamespaceMatching("Franz.Common.Business.Events")
              // Franz Persistence Layer
              .OrShould().ResideInNamespaceMatching("Franz.Common.EntityFramework")
              .OrShould().ResideInNamespaceMatching("Franz.Common.EntityFramework.Repositories")
              .OrShould().ResideInNamespaceMatching("Franz.Common.EntityFramework.Extensions")
              .OrShould().ResideInNamespaceMatching("Franz.Common.EntityFramework.Configuration")
              .OrShould().ResideInNamespaceMatching("Franz.Common.EntityFramework.Behaviors")
              .OrShould().ResideInNamespaceMatching("Franz.Common.EntityFramework.Properties")
              .OrShould().ResideInNamespaceMatching("Franz.Common.MongoDB")
              .OrShould().ResideInNamespaceMatching("Franz.Common.MongoDB.Config")
              .OrShould().ResideInNamespaceMatching("Franz.Common.MongoDB.Repositories")
              .OrShould().ResideInNamespaceMatching("Franz.Common.Caching")
              // Franz Utilities
              .OrShould().ResideInNamespaceMatching("Franz.Common.Mediator")
              .OrShould().ResideInNamespaceMatching("Franz.Common.Errors")
              .OrShould().ResideInNamespaceMatching("Microsoft.Extensions.DependencyInjection")
              // System Namespaces
              .OrShould().ResideInNamespaceMatching("System")
              .OrShould().ResideInNamespaceMatching("System.Collections")
              .OrShould().ResideInNamespaceMatching("System.Collections.Generic")
              .OrShould().ResideInNamespaceMatching("System.Linq")
              .OrShould().ResideInNamespaceMatching("System.Threading")
              .OrShould().ResideInNamespaceMatching("System.Threading.Tasks")
              .OrShould().ResideInNamespaceMatching("System.Runtime.CompilerServices")
              // Self Namespace
              .OrShould().ResideInNamespaceMatching("BookManagement.Persistence")
              .Because("The Persistence layer should depend only on Franz persistence abstractions, domain concepts, and system libraries.")
              .WithoutRequiringPositiveResults();

          rule.Check(BaseArchitecture);
          sb.AppendLine("✅ Verified persistence dependency isolation (Franz + System + self).");
        }, sb, markViolation);

        // RULE 5 — Isolation from upper layers
        ExecuteRule("Isolation", "Persistence must not depend on API or Application layers.", () =>
        {
          ArchRuleDefinition
              .Classes()
              .That()
              .ResideInAssembly(PersistenceAssembly)
              .Should()
              .NotDependOnAnyTypesThat()
              .ResideInNamespace("BookManagement.API")
              .AndShould()
              .NotDependOnAnyTypesThat()
              .ResideInNamespace("BookManagement.Application")
              .Because("Persistence must remain isolated from API and Application layers.")
              .Check(BaseArchitecture);

          sb.AppendLine("✅ Confirmed persistence isolation from API and Application layers.");
        }, sb, markViolation);
      });
    }
  }
}
