using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using MediatR;
using Franz.Common.Business.Commands;
using Franz.Common.Business.Domain;
using Franz.Common.Business.Queries;
using FranzTesting.TestingConditions;
using System.Data;
using Xunit;

namespace Franz.Testing.ArchitectureTests;
public class ApplicationArchitectureTests : BaseArchitectureTest
{

  
  [Fact]
  public void Application_Assembly_Should_Exist()
  {
    Assert.NotNull(ApplicationLayer); // Leverages the provider from BaseArchitectureTest
  }
  [Fact]
  public void QueryHandlersFollowNamingConvention()
  {
    ArchRuleDefinition
        .Classes()
        .That()
        .ImplementInterface(typeof(IQueryHandler<,>))
        .And()
        .Are(ApplicationLayer)
        .Should()
        .HaveNameEndingWith("QueryHandler")
        .Check(BaseArchitecture);
  }

  [Fact]
  public void CommandHandlersFollowNamingConvention()
  {
    ArchRuleDefinition
        .Classes()
        .That()
        .ImplementInterface(typeof(ICommandHandler<,>))
        .And()
        .Are(ApplicationLayer)
        .Should()
        .HaveNameEndingWith("CommandHandler")
        .Check(BaseArchitecture);
  }
  [Fact]
  
  public void CommandHandlersMustInheritFromICommandHandler()
  {
    ArchRuleDefinition
        .Classes().That().Are(ApplicationLayer)
        .And().HaveNameEndingWith("CommandHandler")
        .Should().ImplementInterface(typeof(ICommandHandler<,>))
        .Because("All command handlers should implement ICommandHandler to ensure they conform to the defined handling strategy and maintain consistency across the application.")

        .Check(BaseArchitecture);
  }

  [Fact]
  public void QueryHandlersMustInheritFromIQueryHandler()
  {
    ArchRuleDefinition
        .Classes().That().Are(ApplicationLayer)
        .And().HaveNameEndingWith("QueryHandler")
        .Should().ImplementInterface(typeof(IQueryHandler<,>))
        .Because("All query handlers should implement IQueryHandler to ensure they conform to the defined handling strategy and maintain consistency across the application.")

        .Check(BaseArchitecture);
  }


  [Fact]
  public void HandlersInheritFromIRequestHandler()
  {
    ArchRuleDefinition
        .Classes()
        .That()
        .ImplementInterface(typeof(IRequestHandler<,>))
        .And()
        .Are(ApplicationLayer)
        .Should()
        .HaveNameEndingWith("Handler")
        .Check(BaseArchitecture);
  }



  [Fact]
  public void DependencyOnFranzCommonBusiness()
  {
    var rule = ArchRuleDefinition
        .Types().That().Are(ApplicationLayer)
        
        .Should()
        .BeAssignableTo(typeof(ICommandBaseRequest))
        .OrShould()
        .BeAssignableTo(typeof(IEntity))
        .OrShould()
        .BeAssignableTo(typeof(IAggregateRoot))
        .OrShould()
        .DependOnAnyTypesThat()
        .ResideInNamespace("Franz.Common.Business.Events")
        .OrShould().DependOnAnyTypesThat()
        .ResideInNamespace("Franz.Common.Business.Domain")
        .OrShould().DependOnAnyTypesThat()
        .ResideInNamespace("Franz.Common.Business.Commands")
        .OrShould().DependOnAnyTypesThat()
        .ResideInNamespace("Franz.Common.Business.Queries")
        .OrShould().DependOnAnyTypesThat()
        .ResideInNamespace("MediatR")
        .OrShould().DependOnAnyTypesThat()
        .ResideInNamespace("System")
        .OrShould().DependOnAnyTypesThat()
        .ResideInNamespace("AutoMapper")

        .Because("Application layer should utilize shared business logic components from the Franz.Common.Business namespace, ensuring consistent business logic implementation across applications.");
    
    rule.Check(BaseArchitecture);
  }


}
