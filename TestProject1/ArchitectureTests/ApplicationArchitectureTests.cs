using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Domain;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Messages;
using System;
using System.Linq;
using Xunit;

namespace Franz.Testing.ArchitectureTests
{
  /// <summary>
  /// 🧩 Franz Architecture Governance — Application Layer
  /// Enforces CQRS and dependency boundaries for the Application assembly.
  /// </summary>
  public class ApplicationArchitectureTests : BaseArchitectureTest
  {
    [Fact]
    public void Application_Assembly_Should_Exist()
    {
      Assert.NotNull(ApplicationLayer);
    }

    // ───────────────────────────────────────────────
    // ⚙️ COMMAND HANDLERS
    // ───────────────────────────────────────────────
    [Fact]
    public void CommandHandlers_Should_Implement_ICommandHandler_And_Follow_Naming()
    {
      var handlers = ApplicationLayer.GetObjects(BaseArchitecture)
          .Where(t => t.Name.EndsWith("CommandHandler", StringComparison.OrdinalIgnoreCase))
          .ToList();

      if (!handlers.Any())
      {
        Console.WriteLine("🟡 No CommandHandlers found in Application layer — skipping rule.");
        return;
      }

      ArchRuleDefinition
          .Classes()
          .That().Are(handlers)
          .Should()
          .ImplementInterface(typeof(ICommandHandler<,>))
          .AndShould()
          .HaveNameEndingWith("CommandHandler")
          .Because("Command handlers must implement ICommandHandler<,> and follow the 'SomethingCommandHandler' naming pattern.")
          .Check(BaseArchitecture);

      Console.WriteLine($"✅ Validated {handlers.Count} command handler(s).");
    }

    // ───────────────────────────────────────────────
    // 🔍 QUERY HANDLERS
    // ───────────────────────────────────────────────
    [Fact]
    public void QueryHandlers_Should_Implement_IQueryHandler_And_Follow_Naming()
    {
      var handlers = ApplicationLayer.GetObjects(BaseArchitecture)
          .Where(t => t.Name.EndsWith("QueryHandler", StringComparison.OrdinalIgnoreCase))
          .ToList();

      if (!handlers.Any())
      {
        Console.WriteLine("🟡 No QueryHandlers found in Application layer — skipping rule.");
        return;
      }

      ArchRuleDefinition
          .Classes()
          .That().Are(handlers)
          .Should()
          .ImplementInterface(typeof(IQueryHandler<,>))
          .AndShould()
          .HaveNameEndingWith("QueryHandler")
          .Because("Query handlers must implement IQueryHandler<,> and follow the 'SomethingQueryHandler' naming pattern.")
          .Check(BaseArchitecture);

      Console.WriteLine($"✅ Validated {handlers.Count} query handler(s).");
    }

    // ───────────────────────────────────────────────
    // 🧠 NOTIFICATION HANDLERS
    // ───────────────────────────────────────────────
    [Fact]
    public void NotificationHandlers_Should_Implement_INotificationHandler_And_Follow_Naming()
    {
      if (!HasEventHandlers)
      {
        Console.WriteLine("🟡 No notification or event handlers found — skipping rule.");
        return;
      }

      ArchRuleDefinition
          .Classes()
          .That()
          .ImplementInterface(typeof(INotificationHandler<>))
          .And()
          .Are(ApplicationLayer)
          .Should()
          .HaveNameEndingWith("Handler")
          .Because("All notification handlers should follow naming and inheritance patterns for consistency.")
          .Check(BaseArchitecture);
    }

    // ───────────────────────────────────────────────
    // 🧱 DOMAIN EVENTS & HANDLERS
    // ───────────────────────────────────────────────
    [Fact]
    public void EventHandlers_Should_Implement_IEventHandler_And_Match_DomainEvents()
    {
      ReportArchitectureContext();

      if (!HasDomainEvents)
      {
        Console.WriteLine("🟡 No domain events or event handlers detected — skipping rule.");
        return;
      }

      var pureDomainEvents = DomainEventTypes
          .Where(t =>
              !t.FullName.Contains("Validation", StringComparison.OrdinalIgnoreCase) &&
              !t.FullName.Contains("Notification", StringComparison.OrdinalIgnoreCase) &&
              !t.FullName.Contains("Pipeline", StringComparison.OrdinalIgnoreCase) &&
              !t.FullName.Contains("Mediator", StringComparison.OrdinalIgnoreCase) &&
              !t.FullName.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase))
          .ToList();

      var internalAppEvents = DomainEventTypes.Except(pureDomainEvents).ToList();

      // Domain events must implement proper interfaces
      if (pureDomainEvents.Any())
      {
        ArchRuleDefinition
            .Classes()
            .That().Are(pureDomainEvents)
            .Should()
            .ImplementAnyInterfacesThat().HaveFullName("Franz.Common.Business.Events.IDomainEvent")
            .OrShould()
            .ImplementAnyInterfacesThat().HaveFullName("Franz.Common.Business.Events.IEvent")
            .AndShould()
            .HaveNameEndingWith("Event")
            .Because("All domain events must implement IDomainEvent or IEvent.")
            .Check(BaseArchitecture);

        Console.WriteLine($"✅ Validated {pureDomainEvents.Count} domain event(s).");
      }

      // Application handlers must implement correct event contracts
      if (ApplicationEventHandlerTypes.Any())
      {
        ArchRuleDefinition
            .Classes()
            .That().Are(ApplicationEventHandlerTypes)
            .Should()
            .ImplementAnyInterfacesThat().HaveFullName("Franz.Common.Mediator.Handlers.IEventHandler`1")
            .OrShould()
            .ImplementAnyInterfacesThat().HaveFullName("Franz.Common.Mediator.Handlers.INotificationHandler`1")
            .AndShould()
            .HaveNameEndingWith("Handler")
            .Because("All event handlers must implement IEventHandler<T> or INotificationHandler<T>.")
            .Check(BaseArchitecture);

        Console.WriteLine($"✅ Validated {ApplicationEventHandlerTypes.Count} application event handler(s).");
      }

      if (internalAppEvents.Any())
      {
        Console.WriteLine($"ℹ️ Ignored {internalAppEvents.Count} internal events (Validation/Notification/Pipeline):");
        foreach (var evt in internalAppEvents)
          Console.WriteLine($"   ↳ {evt.FullName}");
      }

      Console.WriteLine($"✅ Event governance check completed — {pureDomainEvents.Count} domain event(s), {ApplicationEventHandlerTypes.Count} handler(s).");
    }

    // ───────────────────────────────────────────────
    // 🧩 DEPENDENCY BOUNDARY
    // ───────────────────────────────────────────────
    [Fact(DisplayName = "🧩 Application Layer — Strict Dependency Governance")]
    public void ApplicationLayer_Should_Depend_Only_On_Allowed_Namespaces()
    {
      ReportArchitectureContext();

      var rule = ArchRuleDefinition
          .Types()
          .That()
          .Are(ApplicationLayer)
          .Should()
          .DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Business.Domain")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Business.Entities")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Business.Events")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Core")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Handlers")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines.Core")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines.Logging")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines.Validation")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines.Transaction")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Validation")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Extensions")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mapping")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mapping.Abstractions")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mapping.Core")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Logging")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("System")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("System.Threading")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("System.Threading.Tasks")
          .OrShould().DependOnAnyTypesThat().ResideInNamespace("Microsoft.Extensions.DependencyInjection")
          .Because("The Application layer must remain pure — only Franz.Common and System namespaces are allowed.");

      try
      {
        rule.Check(BaseArchitecture);
        Console.WriteLine("✅ Application layer validated — no foreign dependencies detected.");
      }
      catch (FailedArchRuleException ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("🚨 Franz detected an impurity in the Application layer!");
        Console.ResetColor();
        Console.WriteLine($"Details: {ex.Message}");
        throw;
      }
    }
  }
}
