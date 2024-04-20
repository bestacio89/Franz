using ArchUnitNET.Loader;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Franz.Common.Business.Commands;
using Franz.Common.Business.Domain;
using Franz.Common.EntityFramework;
using Assembly = System.Reflection.Assembly;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using Franz.Common.Business.Queries;

namespace Franz.Testing;


public abstract class BaseTests
{

  protected static readonly Assembly DomainAssembly = typeof(Entity).Assembly;
  protected static readonly Assembly ApplicationAssembly = typeof(ICommandHandler<,>).Assembly;
  protected static readonly Assembly PersistenceAssembly = typeof(DbContextBase).Assembly;
  protected static readonly Assembly ApiAssembly = typeof(Program).Assembly;
  protected static readonly Assembly ContractsAssembly = typeof(IQueryRequest<>).Assembly;
}
public abstract class BaseArchitectureTest : BaseTests

{
  protected static readonly Architecture BaseArchitecture = new ArchLoader()
  .LoadAssemblies(
    DomainAssembly,
    ApplicationAssembly,
    PersistenceAssembly,
    ApiAssembly,
    ContractsAssembly)
   .Build();

  protected static readonly IObjectProvider<IType> DomainLayer =
    ArchRuleDefinition.Types().That().ResideInAssembly(DomainAssembly).As("Domain Layer");
  
  protected static readonly IObjectProvider<IType> PersistenceLayer =
    ArchRuleDefinition.Types().That().ResideInAssembly(PersistenceAssembly).As("Persistence Layer");

  protected static readonly IObjectProvider<IType> APILayer =
    ArchRuleDefinition.Types().That().ResideInAssembly(ApiAssembly).As("API Layer");

  protected static readonly IObjectProvider<IType> ApplicationLayer =
    ArchRuleDefinition.Types().That().ResideInAssembly(ApplicationAssembly).As("Application Layer");


  protected static readonly IObjectProvider<IType> ContractsLayer =
    ArchRuleDefinition.Types().That().ResideInAssembly(ContractsAssembly).As("Contracts Layer");
}
