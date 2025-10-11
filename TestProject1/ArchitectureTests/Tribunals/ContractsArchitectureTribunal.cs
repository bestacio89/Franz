using System;
using System.Linq;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Domain;
using Franz.Common.DependencyInjection;
using Franz.Common.Mediator.Messages;
using FranzTesting;
using Interface = System.Reflection.TypeInfo;
using Xunit;

namespace Franz.Testing.ArchitectureTests.Tribunals
{
  /// <summary>
  /// ⚖️ Franz Tribunal — Contracts Layer Governance
  /// Enforces naming conventions for Commands, Queries, DTOs and interface lifetimes.
  /// </summary>
  public class ContractsArchitectureTribunal : TribunalBase
  {
    [Fact(DisplayName = "⚖️ Contracts Tribunal — Naming & Lifetime Governance")]
    public void Contracts_Governance()
    {
      ExecuteTribunal("Contracts Tribunal", (sb, markViolation) =>
      {
        // RULE 1 — Assembly presence
        ExecuteRule("Assembly", "Contracts assembly not found.", () =>
        {
          Assert.NotNull(ContractsLayer);
        }, sb, markViolation);

        // RULE 2 — Query naming convention
        ExecuteRule("Queries", "Queries must end with 'Query' and implement IQuery<>.", () =>
        {
          var queries = ContractsLayer.GetObjects(BaseArchitecture)
              .Where(t => t.Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!queries.Any())
          {
            sb.AppendLine("🟡 No Queries found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .AreAssignableTo(typeof(IQuery<>))
              .Should()
              .HaveNameEndingWith("Query")
              .Because("All query types should follow the 'SomethingQuery' naming pattern.")
              .Check(BaseArchitecture);
        }, sb, markViolation);

        // RULE 3 — Command naming convention
        ExecuteRule("Commands", "Commands must end with 'Command' and implement ICommand.", () =>
        {
          var commands = ContractsLayer.GetObjects(BaseArchitecture)
              .Where(t => t.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!commands.Any())
          {
            sb.AppendLine("🟡 No Commands found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .AreAssignableTo(typeof(ICommand))
              .Should()
              .HaveNameEndingWith("Command")
              .Because("All command types should follow the 'SomethingCommand' naming pattern.")
              .Check(BaseArchitecture);
        }, sb, markViolation);

        // RULE 4 — DTO naming convention
        ExecuteRule("DTOs", "DTOs must end with 'Dto' and reside in Franz.Contracts.DTOs.", () =>
        {
          var dtos = ContractsLayer.GetObjects(BaseArchitecture)
              .Where(t => t.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!dtos.Any())
          {
            sb.AppendLine("🟡 No DTOs found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .ResideInNamespace("Franz.Contracts.DTOs", true)
              .Should()
              .HaveNameEndingWith("Dto")
              .Because("All data transfer objects must be suffixed with 'Dto' for clarity and discoverability.")
              .Check(BaseArchitecture);
        }, sb, markViolation);

        // RULE 5 — Infrastructure interfaces (lifetime enforcement)
        ExecuteRule("Infrastructure Interfaces", "Infrastructure interfaces must define explicit lifetimes.", () =>
        {
          var interfaces = ContractsLayer.GetObjects(BaseArchitecture)
              .Where(t =>
                  t.Name.StartsWith("I") &&
                  !t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase) &&
                  !t.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase) &&
                  !t.Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!interfaces.Any())
          {
            sb.AppendLine("🟡 No custom service interfaces found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(interfaces)
              .Should()
              .BeAssignableTo(typeof(IScopedDependency))
              .OrShould()
              .BeAssignableTo(typeof(ISingletonDependency))
              .Because("All infrastructure interfaces must explicitly declare lifetime via dependency interfaces.")
              .Check(BaseArchitecture);
        }, sb, markViolation);

        // RULE 6 — Repository interface enforcement
        ExecuteRule("Repositories", "Repository interfaces must implement Franz persistence abstractions.", () =>
        {
          var repos = ContractsLayer.GetObjects(BaseArchitecture)
              .Where(t => t.Name.StartsWith("I") && t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!repos.Any())
          {
            sb.AppendLine("🟡 No repository interfaces found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(repos)
              .Should()
              .BeAssignableTo(typeof(IReadRepository<>))
              .OrShould()
              .BeAssignableTo(typeof(IAggregateRepository<,>))
              .AndShould()
              .HaveNameEndingWith("Repository")
              .Because("All repository interfaces must follow Franz's persistence contract model.")
              .Check(BaseArchitecture);
        }, sb, markViolation);

        // RULE 7 — Custom repository lifetime enforcement
        ExecuteRule("Custom Repositories", "Custom repositories must implement IScopedDependency.", () =>
        {
          var customRepos = ContractsLayer.GetObjects(BaseArchitecture)
              .Where(t =>
                  t.Name.StartsWith("I") &&
                  t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!customRepos.Any())
          {
            sb.AppendLine("🟡 No custom repository interfaces found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(customRepos)
              .Should()
              .NotBeAssignableTo(typeof(IReadRepository<>))
              .AndShould()
              .NotBeAssignableTo(typeof(IAggregateRepository<,>))
              .AndShould()
              .BeAssignableTo(typeof(IScopedDependency))
              .Because("Custom repositories should explicitly declare scoped lifetime for correct DI behaviour.")
              .Check(BaseArchitecture);
        }, sb, markViolation);
      });
    }
  }
}
