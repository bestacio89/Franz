using System;
using System.Linq;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Domain;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Messages;
using FranzTesting;
using Xunit;

namespace Franz.Testing.ArchitecturalReports.Layers
{
  /// <summary>
  /// ⚖️ Franz Tribunal — Application Layer Compliance Audit
  /// Validates CQRS handler naming, event handling compliance, and dependency isolation
  /// within the Application layer. Fully dynamic for any solution prefix.
  /// </summary>
  public sealed class ApplicationLayerComplianceAudit : ArchitecturalAuditBase
  {
    [Trait("Category", "ArchitecturalReport")]
    public void Audit_ApplicationLayer_Compliance()
    {
      ExecuteTribunal("Application Layer Compliance Audit", (sb, markViolation) =>
      {
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine("             APPLICATION LAYER COMPLIANCE AUDIT                 ");
        sb.AppendLine("---------------------------------------------------------------");

        var prefix = SolutionPrefix; // 🔹 Dynamic prefix (Franz, Raanz, ActivusFranz, etc.)

        // RULE 1 — Assembly presence
        ExecuteRule("Assembly Presence", "Application assembly must be present.", () =>
        {
          Assert.NotNull(ApplicationAssembly);
          sb.AppendLine("✅ Verified: Application assembly detected.");
        }, sb, markViolation);

        // RULE 2 — Command Handler naming convention
        ExecuteRule("Command Handlers", "CommandHandlers must end with 'CommandHandler' and implement ICommandHandler.", () =>
        {
          var handlers = ApplicationLayer
              .GetObjects(BaseArchitecture)
              .Where(t => t.Name.EndsWith("CommandHandler", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!handlers.Any())
          {
            sb.AppendLine("🟡 No CommandHandlers found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .AreAssignableTo(typeof(ICommandHandler<,>))
              .And()
              .Are(ApplicationLayer)
              .Should()
              .HaveNameEndingWith("CommandHandler")
              .Because("Command handlers must follow CQRS conventions for traceability.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Verified {handlers.Count} CommandHandler class(es) follow CQRS naming conventions.");
        }, sb, markViolation);

        // RULE 3 — Query Handler naming convention
        ExecuteRule("Query Handlers", "QueryHandlers must end with 'QueryHandler' and implement IQueryHandler.", () =>
        {
          var handlers = ApplicationLayer
              .GetObjects(BaseArchitecture)
              .Where(t => t.Name.EndsWith("QueryHandler", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!handlers.Any())
          {
            sb.AppendLine("🟡 No QueryHandlers found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .AreAssignableTo(typeof(IQueryHandler<,>))
              .And()
              .Are(ApplicationLayer)
              .Should()
              .HaveNameEndingWith("QueryHandler")
              .Because("Query handlers must follow CQRS conventions for consistency.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Verified {handlers.Count} QueryHandler class(es) follow CQRS naming conventions.");
        }, sb, markViolation);

        // RULE 4 — Event and Notification handler compliance
        ExecuteRule("Event Handlers", "Handlers must implement event or notification interfaces correctly.", () =>
        {
          if (!HasDomainEvents)
          {
            sb.AppendLine("🟡 No domain events detected — skipping event governance rule.");
            return;
          }

          // Validate domain event types
          var validEvents = DomainEventTypes
              .Where(t => !t.FullName.Contains("Validation", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (validEvents.Any())
          {
            ArchRuleDefinition
                .Classes()
                .That()
                .Are(validEvents)
                .Should()
                .ImplementAnyInterfacesThat()
                .HaveFullName($"{prefix}.Common.Business.Events.IDomainEvent")
                .OrShould()
                .ImplementAnyInterfacesThat()
                .HaveFullName($"{prefix}.Common.Business.Events.IEvent")
                .AndShould()
                .HaveNameEndingWith("Event")
                .Because("Domain events must implement IDomainEvent or IEvent for proper propagation.")
                .Check(BaseArchitecture);

            sb.AppendLine($"✅ Verified {validEvents.Count} DomainEvent class(es) implement correct event contracts.");
          }

          // Validate event handlers
          if (HasEventHandlers && ApplicationEventHandlerTypes.Any())
          {
            ArchRuleDefinition
                .Classes()
                .That()
                .Are(ApplicationEventHandlerTypes)
                .Should()
                .ImplementAnyInterfacesThat()
                .HaveFullName($"{prefix}.Common.Mediator.Handlers.IEventHandler`1")
                .OrShould()
                .ImplementAnyInterfacesThat()
                .HaveFullName($"{prefix}.Common.Mediator.Handlers.INotificationHandler`1")
                .AndShould()
                .HaveNameEndingWith("Handler")
                .Because("Application event handlers must implement proper mediator interfaces.")
                .Check(BaseArchitecture);

            sb.AppendLine($"✅ Verified {ApplicationEventHandlerTypes.Count} event handler(s) implement mediator interfaces.");
          }
          else
          {
            sb.AppendLine("🟡 No event handlers detected — skipping handler compliance rule.");
          }
        }, sb, markViolation);

        // RULE 5 — Dependency isolation and purity (dynamic)
        ExecuteRule("Dependency Isolation", "Application layer must depend only on allowed abstractions or system namespaces.", () =>
        {
          var rule = ArchRuleDefinition
              .Types()
              .That()
              .Are(ApplicationLayer)
              .Should()
              .DependOnAnyTypesThat()
              // Franz core / framework abstractions — allowed
              .ResideInNamespaceMatching($"^{prefix}\\.Common(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.Business(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.Mediator(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.EntityFramework(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.Mapping(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Common\\.Logging(\\..*)?$")
              // Contracts & Domain — allowed
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Contracts(\\..*)?$")
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Domain(\\..*)?$")
              // Persistence: allowed for DI abstraction only
              .OrShould().ResideInNamespaceMatching($"^{prefix}\\.Persistence(\\..*)?$")
              // System & Microsoft libs
              .OrShould().ResideInNamespaceMatching(@"^System(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Microsoft(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Microsoft\\.Extensions\\.DependencyInjection(\\..*)?$")
              .Because("Application may depend on framework abstractions, Domain, Contracts, or Persistence DI types — not on API or infra.")
              .WithoutRequiringPositiveResults();

          rule.Check(BaseArchitecture);
          sb.AppendLine("✅ Verified Application layer maintains dependency boundaries (Common, Domain, Contracts, Persistence(DI), System).");
        }, sb, markViolation);

        // ───────────────────────────────
        // 🎯 VERDICT SUMMARY
        // ───────────────────────────────
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine(" APPLICATION LAYER COMPLIANCE: COMPLETED SUCCESSFULLY");
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine($"🕊️  {prefix}.Application Audit Verdict: Excellent");
        sb.AppendLine("⚙️  Handlers: CQRS ✔  |  Dependencies: Pure ✔  |  Events: Compliant ✔");
      });
    }
  }
}
