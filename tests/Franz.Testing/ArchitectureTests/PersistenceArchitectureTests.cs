using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Franz.Testing.ArchitectureTests;

public sealed class PersistenceArchitectureTests : BaseArchitectureTest
{
    private static Assembly[] LoadAssembliesWithPattern(string assemblyPattern)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => Regex.IsMatch(assembly.FullName ?? string.Empty, assemblyPattern))
            .ToArray();
    }

    [Fact]
    public void PersistenceAssemblyExistence() => Assert.NotNull(PersistenceAssembly);

    [Fact]
    public void PersistenceAssemblyDependencies()
    {
        var persistenceTypes = LoadAssembliesWithPattern(@".*\.Persistence\..*")
            .SelectMany(assembly => assembly.GetTypes());

        foreach (var type in persistenceTypes)
        {
            var dependencies = type.GetTypeInfo().ImplementedInterfaces;
            Assert.True(dependencies.Any(dependency =>
                dependency.FullName?.EndsWith("Domain.dll", StringComparison.Ordinal) == true ||
                dependency.FullName?.EndsWith("Contracts.dll", StringComparison.Ordinal) == true));
        }
    }

    [Fact]
    public void PersistenceLayerDependencies_AreCorrect()
    {
        ReportArchitectureContext();

        ArchRuleDefinition
            .Classes()
            .That()
            .ResideInAssembly(PersistenceAssembly)
            // Franz.Common 3.0 emits this composition-only source-generated type
            // into the consuming assembly. It is registration glue, not persistence logic.
            .And()
            .DoNotHaveNameEndingWith("FranzMediatorGeneratedRegistration")
            .Should()
            .OnlyDependOnTypesThat()
            .ResideInNamespaceMatching("Franz\\.Common\\..*")
            .OrShould().ResideInNamespaceMatching("Microsoft\\..*")
            .OrShould().ResideInNamespaceMatching("Franz\\.Contracts\\..*")
            .OrShould().ResideInNamespaceMatching("Franz\\.Domain\\..*")
            .OrShould().ResideInNamespaceMatching("Franz\\.Persistence(\\..*)?")
            .OrShould().ResideInNamespaceMatching("System(\\..*)?")
            .Because("Persistence must depend only on Franz framework abstractions, domain/contracts, its own implementation, and platform libraries.")
            .WithoutRequiringPositiveResults()
            .Check(BaseArchitecture);
    }

    [Fact]
    public void RepositoryImplementationsExist()
    {
        var persistenceTypes = LoadAssembliesWithPattern(@".*\.Persistence\..*")
            .SelectMany(assembly => assembly.GetTypes());

        foreach (var type in persistenceTypes.Where(type =>
                     type.Name.EndsWith("Repository", StringComparison.Ordinal) &&
                     type.IsClass && !type.IsAbstract))
        {
            var repositoryInterface = type.GetInterfaces()
                .FirstOrDefault(interf => interf.Name.EndsWith("Repository", StringComparison.Ordinal));

            Assert.NotNull(repositoryInterface);
        }
    }
}
