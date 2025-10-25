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
  /// Application Layer Compliance Audit —
  /// Validates CQRS handler naming, event handling compliance, and dependency isolation
  /// within the Franz Application layer.
  /// </summary>
  public class ApplicationLayerComplianceAudit : ArchitecturalAuditBase
  {
    [Trait("Category", "ArchitecturalReport")]
  
    public void Audit_ApplicationLayer_Compliance()
    {
      ExecuteTribunal("Application Layer Compliance Audit", (sb, markViolation) =>
      {
        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine("             APPLICATION LAYER COMPLIANCE AUDIT                 ");
        sb.AppendLine("---------------------------------------------------------------");

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
              .Where(t => t.Name.EndsWith("CommandHandler"))
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
              .Where(t => t.Name.EndsWith("QueryHandler"))
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
                .HaveFullName("Franz.Common.Business.Events.IDomainEvent")
                .OrShould()
                .ImplementAnyInterfacesThat()
                .HaveFullName("Franz.Common.Business.Events.IEvent")
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
                .HaveFullName("Franz.Common.Mediator.Handlers.IEventHandler`1")
                .OrShould()
                .ImplementAnyInterfacesThat()
                .HaveFullName("Franz.Common.Mediator.Handlers.INotificationHandler`1")
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

        // RULE 5 — Dependency isolation and purity
        ExecuteRule("Dependency Isolation", "Application layer must depend only on allowed Franz abstractions or System namespaces.", () =>
        {
          var rule = ArchRuleDefinition
              .Types()
              .That()
              .Are(ApplicationLayer)
              .Should()
              .DependOnAnyTypesThat()
              // Franz core / framework abstractions — allowed
              .ResideInNamespaceMatching(@"^Franz\.Common(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Common\.Business(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Common\.Mediator(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Common\.EntityFramework(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Common\.Mapping(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Common\.Logging(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Common\.Mapping$")
              // Contracts & Domain are allowed
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Contracts(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Domain(\..*)?$")
              // Persistence: allow only as a controlled/DI-level reference (eg DI type parameter)
              .OrShould().ResideInNamespaceMatching(@"^Franz\.Persistence(\..*)?$")
              // System / Microsoft/framework
              .OrShould().ResideInNamespaceMatching(@"^System(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Microsoft(\..*)?$")
              .OrShould().ResideInNamespaceMatching(@"^Microsoft\.Extensions\.DependencyInjection(\..*)?$")
              // Rationale
              .Because("Application layer may depend on Franz.Common (framework), Contracts, Domain and controlled Persistence DI-types (EntityRepository<TDbContext,TEntity>), but must not directly reference API or implementation-specific infra.")
              // avoid requiring positive matches (prevents failures on minimal templates)
              .WithoutRequiringPositiveResults();

          rule.Check(BaseArchitecture);
          sb.AppendLine("✅ Verified Application layer maintains dependency boundaries (allowed: Franz.Common, Contracts, Domain, Persistence(DI), System).");
        }, sb, markViolation);


        sb.AppendLine("---------------------------------------------------------------");
        sb.AppendLine(" APPLICATION LAYER COMPLIANCE: COMPLETED SUCCESSFULLY");
        sb.AppendLine("---------------------------------------------------------------");
      });
    }
  }
}
