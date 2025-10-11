using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Domain;
using Franz.Common.Business.Events;
using Franz.Common.Mediator;
using Franz.Common.Mediator.Results;
using Franz.Common.Mediator.Errors;
namespace Franz.Testing.ArchitectureTests;

/// <summary>
/// Validates the structural integrity of the Domain layer in Franz-based projects.
/// </summary>
public class DomainArchitectureTests : BaseArchitectureTest
{
  // ─────────────────────────────────────────────
  // 🧱 ENTITY RULES
  // ─────────────────────────────────────────────
 
  [Fact]
  public void DomainEntities_ShouldInherit_FromEntityOrEntityOfT()
  {
    // 🧱 Identify all domain entities that are NOT value objects or infrastructure
    var domainEntities = DomainLayer
        .GetObjects(BaseArchitecture)
        .Where(t =>
           
            !t.ResidesInNamespace("Franz.Domain.ValueObjects", true) && // 🚫 exclude ValueObjects
            !t.FullName.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase) &&
            !t.FullName.Contains("Persistence", StringComparison.OrdinalIgnoreCase) &&
            !t.FullName.Contains("Mongo", StringComparison.OrdinalIgnoreCase) &&
            !t.Name.Contains("Repository", StringComparison.OrdinalIgnoreCase) &&
            !t.Name.Contains("Context", StringComparison.OrdinalIgnoreCase) &&
            !t.Name.Contains("Handler", StringComparison.OrdinalIgnoreCase) &&
            !t.Name.Contains("Validator", StringComparison.OrdinalIgnoreCase) &&
            !t.Name.Contains("Service", StringComparison.OrdinalIgnoreCase) &&
            t.Assembly.NameEquals(DomainAssembly.GetName().Name))
        .ToList();

    if (!domainEntities.Any())
    {
      Console.WriteLine("🟡 No domain entities found — skipping Entity<> enforcement.");
      return;
    }

    // ✅ Enforce that all domain entities inherit Entity or Entity<T>
    ArchRuleDefinition
        .Classes()
        .That()
        .Are(domainEntities)
        .Should()
        .BeAssignableTo(typeof(Entity<>))
        .OrShould()
        .BeAssignableTo(typeof(IEntity))
        .Because("All domain entities must inherit Entity or Entity<TId> for consistent identity, equality, and lifecycle management.")
        .Check(BaseArchitecture);

    Console.WriteLine($"✅ Verified {domainEntities.Count} domain entity type(s) inherit Entity or Entity<TId>.");
  }

  // ─────────────────────────────────────────────
  // ⚙️ AGGREGATE ROOT RULES
  // ─────────────────────────────────────────────
  [Fact]
  public void AggregateRoots_AreSetupCorrectly()
  {
    ReportArchitectureContext();

    var aggregateRootInterface = BaseArchitecture.Interfaces
        .FirstOrDefault(i => i.FullName != null &&
                             i.FullName.StartsWith(typeof(IAggregateRoot<>).FullName!,
                             StringComparison.OrdinalIgnoreCase));

    if (aggregateRootInterface == null)
    {
      Console.WriteLine("🟡 IAggregateRoot<TEvent> interface not found — skipping aggregate root validation.");
      return;
    }

    var domainEventInterface = BaseArchitecture.Interfaces
        .FirstOrDefault(i => i.FullName == typeof(IDomainEvent).FullName);

    var aggregateRoots = DomainLayer
        .GetObjects(BaseArchitecture)
        .Where(t => t.ImplementsInterface(aggregateRootInterface))
        .ToList();

    if (!aggregateRoots.Any())
    {
      Console.WriteLine("🟡 No aggregate roots implementing IAggregateRoot<TEvent> found — skipping aggregate rule.");
      return;
    }

    // ✅ Structural rule
    ArchRuleDefinition
        .Classes()
        .That()
        .Are(aggregateRoots)
        .Should()
        .BeAssignableTo(typeof(AggregateRoot<>))
        .AndShould()
        .HaveNameEndingWith("Aggregate")
        .Because("Aggregate roots should implement IAggregateRoot<TEvent> and inherit from AggregateRoot<> base class.")
        .Check(BaseArchitecture);

    Console.WriteLine($"✅ Validated {aggregateRoots.Count} aggregate root(s) successfully.");

    // ✅ Optional: Ensure aggregates depend on domain events
    if (domainEventInterface != null)
    {
      ArchRuleDefinition
          .Classes()
          .That()
          .Are(aggregateRoots)
          .Should()
          .DependOnAnyTypesThat()
          .ImplementInterface(domainEventInterface)
          .Because("Aggregate roots should be capable of raising domain events.")
          .Check(BaseArchitecture);

      Console.WriteLine("✅ Verified aggregate roots depend on domain events.");
    }
    else
    {
      Console.WriteLine("🟡 IDomainEvent interface not found — skipping dependency validation.");
    }
  }

  // ─────────────────────────────────────────────
  // 📢 DOMAIN EVENT RULES
  // ─────────────────────────────────────────────
  [Fact]
  public void Events_AreSetupCorrectly()
  {
    ReportArchitectureContext();

    var aggregateRootInterface = BaseArchitecture.Interfaces
        .FirstOrDefault(i => i.FullName != null &&
                             i.FullName.StartsWith(typeof(IAggregateRoot<>).FullName!,
                             StringComparison.OrdinalIgnoreCase));

    var hasAggregates = DomainLayer
        .GetObjects(BaseArchitecture)
        .Any(t => aggregateRootInterface != null && t.ImplementsInterface(aggregateRootInterface));

    if (!hasAggregates)
    {
      Console.WriteLine("🟡 No aggregates found — skipping event validation test.");
      return;
    }

    if (!HasDomainEvents)
    {
      Console.WriteLine("🟡 No domain events found — skipping event validation test.");
      return;
    }

    var domainEventInterface = BaseArchitecture.Interfaces
        .FirstOrDefault(i => i.FullName == typeof(IDomainEvent).FullName);

    var integrationEventInterface = BaseArchitecture.Interfaces
        .FirstOrDefault(i => i.FullName == typeof(IIntegrationEvent).FullName);

    var validDomainEvents = DomainEventTypes
        .Where(t => !t.FullName.Contains("Validation", StringComparison.OrdinalIgnoreCase)
                 && !t.FullName.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (!validDomainEvents.Any())
    {
      Console.WriteLine("🟡 No valid domain events found after filtering internal types — skipping.");
      return;
    }

    ArchRuleDefinition
        .Classes()
        .That()
        .Are(validDomainEvents)
        .Should()
        .ImplementInterface(domainEventInterface)
        .OrShould()
        .ImplementInterface(integrationEventInterface)
        .AndShould()
        .HaveNameEndingWith("Event")
        .Because("All events should implement IDomainEvent or IIntegrationEvent and follow the 'SomethingHappenedEvent' naming convention.")
        .Check(BaseArchitecture);

    Console.WriteLine($"✅ Validated {validDomainEvents.Count} domain event(s) successfully.");
  }

  // ─────────────────────────────────────────────
  // 🧭 DOMAIN DEPENDENCY RULES
  // ─────────────────────────────────────────────
  [Fact]
  public void DomainAssemblyDependencies_AreCorrect()
  {
    var domainobjects = DomainLayer.GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("Event") || t.Name.EndsWith("AggregateRoot") || t.GetType().Namespace == "*.Domain.Entities")
        .ToList();

    if (!domainobjects.Any())
    {
      Console.WriteLine("🟡 No CommandHandlers found in Application layer — skipping rule.");
      return;
    }
    ArchRuleDefinition
        .Classes()
        .That()
        .ResideInAssembly(DomainAssembly)
        .Should()
        .OnlyDependOnTypesThat()
        // Self references (other domain types)
        .ResideInNamespaceMatching("*.Domain")

        // Franz base abstractions
        .OrShould().ResideInAssembly(typeof(Entity<>).Assembly.GetName().Name)          // Franz.Common.Business
        .OrShould().ResideInAssembly(typeof(ValueObject).Assembly.GetName().Name)       // Franz.Common.Business.Domain
        .OrShould().ResideInAssembly(typeof(Result).Assembly.GetName().Name)            // Franz.Common.Mediator
        .OrShould().ResideInAssembly(typeof(Error).Assembly.GetName().Name)             // Franz.Common.Errors

        // System namespaces
        .OrShould().ResideInNamespace("System")
        .OrShould().ResideInNamespace("System.Collections")
        .OrShould().ResideInNamespace("System.Collections.Generic")
        .OrShould().ResideInNamespace("System.Linq")
        .OrShould().ResideInNamespace("System.Runtime.CompilerServices")

        .Because("The Domain layer may depend on itself, Franz domain abstractions, and system libraries only.")
        .WithoutRequiringPositiveResults()
        .Check(BaseArchitecture);

    Console.WriteLine("✅ Verified domain dependency isolation (self + Franz + System).");
  }


  [Fact]
  public void DomainEvents_ShouldNotDependOnInfrastructure()
  {
    if (!HasDomainEvents)
    {
      Console.WriteLine("🟡 No domain events found — skipping infrastructure dependency rule.");
      return;
    }

    ArchRuleDefinition
        .Classes()
        .That()
        .Are(DomainEventTypes)
        .Should()
        .NotDependOnAnyTypesThat()
        .ResideInNamespace("BookManagement.Infrastructure")
        .Because("Domain events must be pure domain concepts without infrastructure dependencies.")
        .Check(BaseArchitecture);

    Console.WriteLine("✅ Confirmed domain events do not depend on infrastructure.");
  }
}

