using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Domain;
using Franz.Common.Business.Repositories;
using Franz.Common.DependencyInjection;
using Franz.Common.Mediator.Messages;
using FranzTesting;
using Xunit;

namespace Franz.Testing.ArchitecturalReports.Layers
{
  /// <summary>
  /// ⚖️ Franz Tribunal — Contracts Layer Governance
  /// Enforces naming, immutability, and lifetime conventions for Franz.Contracts.
  /// </summary>
  public class ContractsLayerComplianceAudit : ArchitecturalAuditBase
  {
    [Trait("Category", "ArchitecturalReport")]

    public void Contracts_Governance()
    {
      ExecuteTribunal( "Contacts Layer Compliance Audit", (sb, markViolation) =>
      {
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine("               CONTRACTS LAYER COMPLIANCE AUDIT                ");
        sb.AppendLine("---------------------------------------------------------------");

        // RULE 1 — Assembly presence
        ExecuteRule("Assembly Presence", "Contracts assembly must be present.", () =>
        {
          Assert.NotNull(ContractsLayer);
          sb.AppendLine("✅ Verified: Contracts assembly detected.");
        }, sb, markViolation);

        // RULE 2 — Query Naming Convention
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

          sb.AppendLine($"✅ Verified {queries.Count} Query class(es) follow CQRS naming conventions.");
        }, sb, markViolation);

        // RULE 3 — Command Naming Convention
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
              .AreAssignableTo(typeof(ICommand<>))
              .Or()
              .AreAssignableTo(typeof(ICommand))
              .Should()
              .HaveNameEndingWith("Command")
              .Because("All command types should follow the 'SomethingCommand' naming pattern.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Verified {commands.Count} Command class(es) follow CQRS naming conventions.");
        }, sb, markViolation);

        // RULE 4 — DTO Naming & Namespace
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
              .Because("All DTOs must be suffixed with 'Dto' and reside in Franz.Contracts.DTOs.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Verified {dtos.Count} DTO class(es) follow naming and namespace conventions.");
        }, sb, markViolation);

        // RULE 5 — DTO Immutability / Record Enforcement
        ExecuteRule("DTO Immutability", "DTOs must be records or immutable types.", () =>
        {
          var dtoObjects = ContractsLayer.GetObjects(BaseArchitecture)
            .Where(t =>
              t.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) ||
              t.Namespace.FullName.Contains("DTOs", StringComparison.OrdinalIgnoreCase))
            .ToList();

          var offenders = new List<IType>();

          foreach (var dto in dtoObjects)
          {
            var type = GetReflectionType(dto);
            if (type == null) continue;

            if (IsRecord(type)) continue;

            bool hasWritableProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
              .Any(p =>
              {
                var setMethod = p.SetMethod;
                if (setMethod == null) return false;

                bool isInitOnly = setMethod.ReturnParameter
                  .GetRequiredCustomModifiers()
                  .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));

                return !isInitOnly;
              });

            if (hasWritableProps)
              offenders.Add(dto);
          }

          if (offenders.Any())
          {
            markViolation();
            sb.AppendLine("🚨 Mutable or non-record DTOs detected:");
            offenders.ForEach(o => sb.AppendLine($" - {o.FullName}"));
          }
          else sb.AppendLine("✅ All DTOs are immutable records — compliance confirmed.");
        }, sb, markViolation);

        // RULE 6 — Infrastructure Interfaces Lifetime Enforcement
        ExecuteRule("Infrastructure Interfaces", "Infrastructure interfaces must define lifetimes.", () =>
        {
          var infraInterfaces = ContractsLayer.GetObjects(BaseArchitecture)
              .Where(t =>
                  t.Name.StartsWith("I") &&
                  !t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase) &&
                  !t.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase) &&
                  !t.Name.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!infraInterfaces.Any())
          {
            sb.AppendLine("🟡 No infrastructure interfaces found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(infraInterfaces)
              .Should()
              .BeAssignableTo(typeof(IScopedDependency))
              .OrShould()
              .BeAssignableTo(typeof(ISingletonDependency))
              .Because("Infrastructure interfaces must declare explicit lifetime scope.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Verified {infraInterfaces.Count} infrastructure interface(s) define lifetime dependencies.");
        }, sb, markViolation);

        // RULE 7 — Custom Repositories Must Be Scoped & Independent
        ExecuteRule("Custom Repositories", "Custom repositories must declare scoped lifetime and remain independent.", () =>
        {
          var customRepos = ContractsLayer.GetObjects(BaseArchitecture)
              .Where(t =>
                  t.Name.StartsWith("I") &&
                  t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase) &&
                  !t.Name.Contains("Aggregate", StringComparison.OrdinalIgnoreCase) &&
                  !t.Name.Contains("Read", StringComparison.OrdinalIgnoreCase) &&
                  !t.Name.Contains("Entity", StringComparison.OrdinalIgnoreCase) &&
                  t.Namespace.FullName.Contains("Persistence", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!customRepos.Any())
          {
            sb.AppendLine("🟡 No custom repositories found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(customRepos)
              .Should()
              .BeAssignableTo(typeof(IScopedDependency))
              .AndShould()
              .NotBeAssignableTo(typeof(IReadRepository<>))
              .AndShould()
              .NotBeAssignableTo(typeof(IAggregateRepository<,>))
              .Because("Custom repositories must declare scoped lifetime and remain independent of framework abstractions.")
              .WithoutRequiringPositiveResults()
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Verified {customRepos.Count} repository interface(s) comply with scoped-lifetime independence.");
        }, sb, markViolation);

        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine(" CONTRACTS LAYER COMPLIANCE: COMPLETED SUCCESSFULLY");
        sb.AppendLine("---------------------------------------------------------------");
      });
    }

    // 🧠 Helpers
    private static bool IsRecord(Type type) =>
      type.GetMethod("<Clone>$", BindingFlags.NonPublic | BindingFlags.Instance) != null ||
      type.GetMethod("PrintMembers", BindingFlags.NonPublic | BindingFlags.Instance) != null;

    private static Type? GetReflectionType(IType archType)
    {
      var maybeTypeProp = archType.GetType().GetProperty("Type", BindingFlags.Public | BindingFlags.Instance);
      if (maybeTypeProp?.GetValue(archType) is Type systemType)
        return systemType;

      return Type.GetType(archType.FullName, throwOnError: false);
    }
  }
}
