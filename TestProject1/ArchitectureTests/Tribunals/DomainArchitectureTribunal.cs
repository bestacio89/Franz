using System;
using System.Linq;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Franz.Common.Business.Domain;
using Franz.Common.Business.Events;
using Franz.Common.Mediator;
using Franz.Common.Mediator.Results;
using Franz.Common.Mediator.Errors;
using FranzTesting;
using Xunit;
using ArchUnitNET.Domain.Extensions;

namespace Franz.Testing.ArchitectureTests.Tribunals
{
  /// <summary>
  /// ⚖️ Franz Tribunal — Domain Layer Governance
  /// Ensures all entities, aggregates, events, and dependencies in the Domain layer comply with Franz architecture laws.
  /// </summary>
  public class DomainArchitectureTribunal : TribunalBase
  {
    [Fact(DisplayName = "⚖️ Domain Tribunal — Entity, Aggregate & Event Governance")]
    public void Domain_Governance()
    {
      ExecuteTribunal("Domain Tribunal", (sb, markViolation) =>
      {
        // RULE 1 — Entity inheritance
        ExecuteRule("Entities", "All Domain Entities must inherit Entity or Entity<T>.", () =>
        {
          var domainEntities = DomainLayer
              .GetObjects(BaseArchitecture)
              .Where(t =>
                  !t.ResidesInNamespace("Franz.Domain.ValueObjects", true) &&
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
            sb.AppendLine("🟡 No domain entities found — skipping Entity<> enforcement.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(domainEntities)
              .Should()
              .BeAssignableTo(typeof(Entity<>))
              .OrShould()
              .BeAssignableTo(typeof(IEntity))
              .Because("All domain entities must inherit Entity or Entity<TId> for consistent identity and lifecycle management.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Verified {domainEntities.Count} domain entity type(s) inherit Entity or Entity<TId>.");
        }, sb, markViolation);

        // RULE 2 — Aggregate roots
        ExecuteRule("Aggregates", "Aggregate roots must inherit AggregateRoot<> and implement IAggregateRoot<T>.", () =>
        {
          var aggregateRootInterface = BaseArchitecture.Interfaces
              .FirstOrDefault(i => i.FullName != null &&
                  i.FullName.StartsWith(typeof(IAggregateRoot<>).FullName!, StringComparison.OrdinalIgnoreCase));

          if (aggregateRootInterface == null)
          {
            sb.AppendLine("🟡 IAggregateRoot<T> interface not found — skipping aggregate validation.");
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
            sb.AppendLine("🟡 No aggregate roots implementing IAggregateRoot<T> found — skipping rule.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(aggregateRoots)
              .Should()
              .BeAssignableTo(typeof(AggregateRoot<>))
              .AndShould()
              .HaveNameEndingWith("Aggregate")
              .Because("Aggregate roots should implement IAggregateRoot<T> and inherit AggregateRoot<> base class.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Validated {aggregateRoots.Count} aggregate root(s) successfully.");

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

            sb.AppendLine("✅ Verified aggregate roots depend on domain events.");
          }
        }, sb, markViolation);

        // RULE 3 — Domain events
        ExecuteRule("Events", "Domain events must implement IDomainEvent or IIntegrationEvent and end with 'Event'.", () =>
        {
          if (!HasDomainEvents)
          {
            sb.AppendLine("🟡 No domain events found — skipping rule.");
            return;
          }

          var domainEventInterface = BaseArchitecture.Interfaces
              .FirstOrDefault(i => i.FullName == typeof(IDomainEvent).FullName);

          var integrationEventInterface = BaseArchitecture.Interfaces
              .FirstOrDefault(i => i.FullName == typeof(IIntegrationEvent).FullName);

          var validEvents = DomainEventTypes
              .Where(t =>
                  !t.FullName.Contains("Validation", StringComparison.OrdinalIgnoreCase) &&
                  !t.FullName.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase))
              .ToList();

          if (!validEvents.Any())
          {
            sb.AppendLine("🟡 No valid domain events found after filtering internal types.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(validEvents)
              .Should()
              .ImplementInterface(domainEventInterface)
              .OrShould()
              .ImplementInterface(integrationEventInterface)
              .AndShould()
              .HaveNameEndingWith("Event")
              .Because("All events should implement IDomainEvent or IIntegrationEvent and follow the 'SomethingHappenedEvent' naming convention.")
              .Check(BaseArchitecture);

          sb.AppendLine($"✅ Validated {validEvents.Count} domain event(s) successfully.");
        }, sb, markViolation);

        // RULE 4 — Domain dependency isolation
        ExecuteRule("Dependencies", "Domain layer must depend only on itself, Franz abstractions, or System libraries.", () =>
        {
          ArchRuleDefinition
              .Classes()
              .That()
              .ResideInAssembly(DomainAssembly)
              .Should()
              .OnlyDependOnTypesThat()
              .ResideInNamespaceMatching("*.Domain")
              .OrShould().ResideInAssembly(typeof(Entity<>).Assembly.GetName().Name)
              .OrShould().ResideInAssembly(typeof(ValueObject).Assembly.GetName().Name)
              .OrShould().ResideInAssembly(typeof(Result).Assembly.GetName().Name)
              .OrShould().ResideInAssembly(typeof(Error).Assembly.GetName().Name)
              .OrShould().ResideInNamespace("System", true)
              .Because("The Domain layer may depend only on itself, Franz domain abstractions, and System libraries.")
              .WithoutRequiringPositiveResults()
              .Check(BaseArchitecture);

          sb.AppendLine("✅ Verified domain dependency isolation (self + Franz + System).");
        }, sb, markViolation);

        // RULE 5 — Domain events must not depend on infrastructure
        ExecuteRule("Purity", "Domain events should not depend on infrastructure namespaces.", () =>
        {
          if (!HasDomainEvents)
          {
            sb.AppendLine("🟡 No domain events found — skipping purity check.");
            return;
          }

          ArchRuleDefinition
              .Classes()
              .That()
              .Are(DomainEventTypes)
              .Should()
              .NotDependOnAnyTypesThat()
              .ResideInNamespace("BookManagement.Infrastructure")
              .Because("Domain events must remain pure and not depend on persistence or infrastructure.")
              .Check(BaseArchitecture);

          sb.AppendLine("✅ Confirmed domain events do not depend on infrastructure.");
        }, sb, markViolation);
      });
    }
  }
}
