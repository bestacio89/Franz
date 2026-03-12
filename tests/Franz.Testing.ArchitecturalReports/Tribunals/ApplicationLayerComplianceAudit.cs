using System;
using System.Linq;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Mediator.Handlers;
using FranzTesting;
using Xunit;

namespace Franz.Testing.ArchitecturalReports.Layers
{
  /// <summary>
  /// ⚖️ Franz Tribunal — Application Layer Compliance Audit
  /// Enforces CQRS patterns, Event Handling discipline, and strict Dependency Isolation.
  /// </summary>
  public sealed class ApplicationLayerComplianceAudit : ArchitecturalAuditBase
  {
    
    [Trait("Category", "ArchitecturalReport")]
    public void Audit_ApplicationLayer_Compliance()
    {
      ExecuteTribunal("Application Layer Compliance Audit", (sb, markViolation) =>
      {
        var prefix = SolutionPrefix;

        // RULE 1 — Assembly Presence (Manual Assert)
        ExecuteRule("Assembly Presence", "Application assembly must be present.", () =>
        {
          Assert.NotNull(ApplicationAssembly);
        }, sb, markViolation);

        // RULE 2 — Command Handler Conventions
        var commandHandlerRule = ArchRuleDefinition.Classes()
            .That().AreAssignableTo(typeof(ICommandHandler<,>))
            .And().Are(ApplicationLayer)
            .Should().HaveNameEndingWith("CommandHandler")
            .Because("Command handlers must follow CQRS naming conventions for clear intent and traceability.");

        ExecuteRule("Command Handlers", "Handlers must implement ICommandHandler and end with 'CommandHandler'.",
            commandHandlerRule, sb, markViolation);

        // RULE 3 — Query Handler Conventions
        var queryHandlerRule = ArchRuleDefinition.Classes()
            .That().AreAssignableTo(typeof(IQueryHandler<,>))
            .And().Are(ApplicationLayer)
            .Should().HaveNameEndingWith("QueryHandler")
            .Because("Query handlers must follow CQRS naming conventions for consistency.");

        ExecuteRule("Query Handlers", "Handlers must implement IQueryHandler and end with 'QueryHandler'.",
            queryHandlerRule, sb, markViolation);

        // RULE 4 — Event Handler Compliance (Mediator Abstractions)
        var eventHandlerRule = ArchRuleDefinition.Classes()
            .That().ResideInNamespaceMatching($"^{prefix}\\.Application\\.EventHandlers(\\..*)?$")
            .Should().ImplementAnyInterfacesThat().HaveFullNameContaining("IEventHandler")
            .OrShould().ImplementAnyInterfacesThat().HaveFullNameContaining("INotificationHandler")
            .AndShould().HaveNameEndingWith("Handler")
            .Because("Application event handlers must be properly registered via Mediator interfaces.")
            .WithoutRequiringPositiveResults();

        ExecuteRule("Event Handlers", "Handlers must implement proper mediator interfaces and naming.",
            eventHandlerRule, sb, markViolation);

        // RULE 5 — Dependency Isolation (The "Purity" Rule)
        var isolationRule = ArchRuleDefinition.Types()
            .That().Are(ApplicationLayer)
            // 🛡️ Filter out the compiler-generated 'state machines' for Async/Await
            .And().DoNotHaveNameMatching(".*<.*>.*")
            .Should().OnlyDependOnTypesThat()
            // 🏛️ Internal Allowed
            .ResideInNamespaceMatching($"^{prefix}\\.Common(\\..*)?$")
            .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Contracts(\\..*)?$")
            .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Domain(\\..*)?$")
            .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Application(\\..*)?$") // Can depend on its own types!

            // 🏗️ The "Plumbing" Permission (Required for LINQ, Tasks, and Mappings)
            .OrShould().ResideInNamespaceMatching(@"^System(\..*)?$")
            .OrShould().ResideInNamespaceMatching(@"^Microsoft(\..*)?$")
            .OrShould().ResideInNamespaceMatching(@"^AutoMapper(\..*)?$") // If IFranzMapper uses it under the hood

            .Because("The Application layer is the pure orchestrator. It uses System abstractions and Franz contracts.")
            .WithoutRequiringPositiveResults();


        ExecuteRule("Dependency Isolation", "Application must only depend on Domain, Contracts, or Framework abstractions.",
            isolationRule, sb, markViolation);

        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine($"🕊️  {prefix}.Application Governance check complete.");
      });
    }
  }
}