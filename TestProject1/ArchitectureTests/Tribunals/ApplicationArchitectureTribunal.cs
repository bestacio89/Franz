using System;
using System.Linq;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Domain;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Messages;
using FranzTesting;
using Xunit;

namespace Franz.Testing.ArchitectureTests.Tribunals
{
  /// <summary>
  /// ⚖️ Franz Tribunal — Application Layer Governance
  /// Enforces CQRS handler naming, inheritance, event handling, and dependency purity.
  /// </summary>
  public class ApplicationArchitectureTribunal : TribunalBase
  {
    [Fact(DisplayName = "⚖️ Application Tribunal — CQRS Governance & Dependency Purity")]
    public void Application_Governance()
    {
      ExecuteTribunal("Application Tribunal", (sb, markViolation) =>
      {
        // RULE 1 — Assembly existence
        ExecuteRule("Assembly", "Application assembly not found.", () =>
        {
          Assert.NotNull(ApplicationLayer);
        }, sb, markViolation);

        // RULE 2 — CommandHandler naming convention
        ExecuteRule("Command Handlers", "CommandHandler naming convention violated.", () =>
        {
          var handlers = ApplicationLayer.GetObjects(BaseArchitecture)
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
              .Because("Command handlers must end with 'CommandHandler' for clarity and traceability.")
              .Check(BaseArchitecture);
        }, sb, markViolation);

        // RULE 3 — QueryHandler naming convention
        ExecuteRule("Query Handlers", "QueryHandler naming convention violated.", () =>
        {
          var handlers = ApplicationLayer.GetObjects(BaseArchitecture)
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
              .Because("Query handlers must end with 'QueryHandler' for clarity and consistency.")
              .Check(BaseArchitecture);
        }, sb, markViolation);

        // RULE 4 — Event & Notification handler enforcement
        ExecuteRule("Event Handlers", "EventHandler or NotificationHandler implementation missing or misnamed.", () =>
        {
          if (!HasDomainEvents)
          {
            sb.AppendLine("🟡 No domain events detected — skipping event governance rule.");
            return;
          }

          var pureDomainEvents = DomainEventTypes
              .Where(t =>
                  !t.FullName.Contains("Validation", StringComparison.OrdinalIgnoreCase) &&
                  !t.FullName.Contains("Notification", StringComparison.OrdinalIgnoreCase) &&
                  !t.FullName.Contains("Pipeline", StringComparison.OrdinalIgnoreCase) &&
                  !t.FullName.Contains("Mediator", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (pureDomainEvents.Any())
          {
            ArchRuleDefinition
                .Classes()
                .That()
                .Are(pureDomainEvents)
                .Should()
                .ImplementAnyInterfacesThat()
                .HaveFullName("Franz.Common.Business.Events.IDomainEvent")
                .OrShould()
                .ImplementAnyInterfacesThat()
                .HaveFullName("Franz.Common.Business.Events.IEvent")
                .AndShould()
                .HaveNameEndingWith("Event")
                .Because("Domain events must implement IDomainEvent or IEvent for traceable propagation.")
                .Check(BaseArchitecture);
          }

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
                .Because("Application handlers must implement IEventHandler<T> or INotificationHandler<T>.")
                .Check(BaseArchitecture);
          }
          else
          {
            sb.AppendLine("🟡 No event handlers detected — skipping handler enforcement.");
          }
        }, sb, markViolation);

        // RULE 5 — Strict dependency governance
        ExecuteRule("Dependencies", "Application layer depends on forbidden namespaces.", () =>
        {
          var rule = ArchRuleDefinition
              .Types()
              .That()
              .Are(ApplicationLayer)
              .Should()
              .DependOnAnyTypesThat()
              .ResideInNamespace("Franz.Common.Business.Domain", true)
              .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Business.Entities", true)
              .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Business.Events", true)
              .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator", true)
              .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mapping", true)
              .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Logging", true)
              .OrShould().DependOnAnyTypesThat().ResideInNamespace("System", true)
              .OrShould().DependOnAnyTypesThat().ResideInNamespace("Microsoft.Extensions.DependencyInjection", true)
              .Because("Application layer must remain pure — only Franz.Common and System namespaces are allowed.");

          rule.Check(BaseArchitecture);
        }, sb, markViolation);
      });
    }
  }
}
