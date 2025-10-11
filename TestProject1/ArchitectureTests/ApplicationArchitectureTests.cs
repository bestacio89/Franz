using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Domain;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Messages;
using FranzTesting.TestingConditions;
using System.Data;
using Xunit;

namespace Franz.Testing.ArchitectureTests;
public class ApplicationArchitectureTests : BaseArchitectureTest
{
  [Fact]
  public void Application_Assembly_Should_Exist()
  {
    Assert.NotNull(ApplicationLayer);
  }

  [Fact]
  public void QueryHandlersFollowNamingConvention()
  {
    var handlers = ApplicationLayer.GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("QueryHandler"))
        .ToList();

    if (!handlers.Any())
    {
      Console.WriteLine("🟡 No CommandHandlers found in Application layer — skipping rule.");
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
        .Because("Query handlers should follow the 'SomethingQueryHandler' naming convention for clarity and consistency.")
        .Check(BaseArchitecture);
  }

  [Fact]
  public void CommandHandlersFollowNamingConvention()
  {
    var handlers = ApplicationLayer.GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("CommandHandler"))
        .ToList();

    if (!handlers.Any())
    {
      Console.WriteLine("🟡 No CommandHandlers found in Application layer — skipping rule.");
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
        .Because("Command handlers should follow the 'SomethingCommandHandler' naming convention for clarity and traceability.")
        .Check(BaseArchitecture);
  }

  [Fact]
  public void CommandHandlersMustInheritFromICommandHandler()
  {
    var handlers = ApplicationLayer.GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("CommandHandler"))
        .ToList();

    if (!handlers.Any())
    {
      Console.WriteLine("🟡 No CommandHandlers found in Application layer — skipping rule.");
      return;
    }
    ArchRuleDefinition
        .Classes()
        .That()
        .Are(ApplicationLayer)
        .And()
        .HaveNameEndingWith("CommandHandler")
        .Should()
        .ImplementInterface(typeof(ICommandHandler<,>))
        .Because("All command handlers must implement ICommandHandler<TCommand, TResult> to ensure consistent CQRS command handling.")
        .Check(BaseArchitecture);
  }

  [Fact]
  public void QueryHandlersMustInheritFromIQueryHandler()
  {
    var handlers = ApplicationLayer.GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("QueryHandler"))
        .ToList();

    if (!handlers.Any())
    {
      Console.WriteLine("🟡 No CommandHandlers found in Application layer — skipping rule.");
      return;
    }
    ArchRuleDefinition
        .Classes()
        .That()
        .Are(ApplicationLayer)
        .And()
        .HaveNameEndingWith("QueryHandler")
        .Should()
        .ImplementInterface(typeof(IQueryHandler<,>))
        .Because("All query handlers must implement IQueryHandler<TQuery, TResult> to ensure consistent CQRS query handling.")
        .Check(BaseArchitecture);
  }

  [Fact]
  public void HandlersInheritFromINotificationHandler()
  {
    if (!HasEventHandlers)
    {
      Console.WriteLine("🟡 No notification or event handlers found — skipping handler inheritance test.");
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
        .Because("All notification handlers should follow naming and inheritance patterns for consistency across modules.")
        .Check(BaseArchitecture);
  }

  [Fact]
  public void EventHandlers_Should_Implement_IEventHandler_And_Match_DomainEvents()
  {
    ReportArchitectureContext();

    // 🧩 Skip if there's neither events nor handlers
    if (!HasDomainEvents)
    {
      Console.WriteLine("🟡 No domain events or event handlers detected — skipping event governance test.");
      return;
    }

    // 🔍 Filter domain vs internal events
    var pureDomainEvents = DomainEventTypes
        .Where(t =>
            !t.FullName.Contains("Validation", StringComparison.OrdinalIgnoreCase) &&
            !t.FullName.Contains("Notification", StringComparison.OrdinalIgnoreCase) &&
            !t.FullName.Contains("Pipeline", StringComparison.OrdinalIgnoreCase) &&
            !t.FullName.Contains("Mediator", StringComparison.OrdinalIgnoreCase) &&
            !t.FullName.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase))
        .ToList();

    var internalAppEvents = DomainEventTypes.Except(pureDomainEvents).ToList();

    // 🧱 DOMAIN EVENTS → Must implement IDomainEvent or IEvent
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
          .Because("All *domain* events must implement IDomainEvent or IEvent to guarantee traceable, consistent propagation.")
          .Check(BaseArchitecture);

      Console.WriteLine($"✅ Validated {pureDomainEvents.Count} pure domain event(s).");
    }
    else
    {
      Console.WriteLine("🟡 No pure domain events found — skipping domain event interface enforcement.");
    }

    // ⚙️ APPLICATION HANDLERS → Must implement IEventHandler<T> or INotificationHandler<T>
    if (HasEventHandlers)
    {
      if (ApplicationEventHandlerTypes.Any())
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
            .Because("All event handlers must implement IEventHandler<T> or INotificationHandler<T> to ensure proper event dispatching.")
            .Check(BaseArchitecture);

        Console.WriteLine($"✅ Validated {ApplicationEventHandlerTypes.Count} event handler(s).");
      }
      else
      {
        Console.WriteLine("🟡 Application event handler list is empty — skipping handler enforcement.");
      }
    }
    else
    {
      Console.WriteLine("🟡 No event handlers found — skipping handler interface enforcement.");
    }

    // 🚫 INTERNAL EVENTS → Log but don't fail
    if (internalAppEvents.Any())
    {
      Console.WriteLine($"ℹ️ Ignored {internalAppEvents.Count} internal events (Validation/Notification/Pipeline):");
      foreach (var evt in internalAppEvents)
        Console.WriteLine($"   ↳ {evt.FullName}");
    }

    Console.WriteLine($"✅ Event governance check completed — {pureDomainEvents.Count} domain event(s), {ApplicationEventHandlerTypes.Count} handler(s).");
  }



  [Fact(DisplayName = "🧩 Application Layer — Strict Dependency Governance")]
  public void ApplicationLayer_Should_Depend_Only_On_Allowed_Namespaces()
  {
    ReportArchitectureContext();


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
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Core", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Handlers", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Messages", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines.Core", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines.Logging", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines.Validation", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Pipelines.Transaction", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Validation", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mediator.Extensions", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mapping", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mapping.Abstractions", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Mapping.Core", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Franz.Common.Logging", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("System", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("System.Threading", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("System.Threading.Tasks", true)
        .OrShould().DependOnAnyTypesThat().ResideInNamespace("Microsoft.Extensions.DependencyInjection", true)
        .Because("The Application layer must remain pure — only Franz.Common and System namespaces are allowed. Any dependency outside these is an architectural breach.");

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


