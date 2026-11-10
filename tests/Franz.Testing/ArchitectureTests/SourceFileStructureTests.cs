using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Franz.Testing.ArchitectureTests;

public sealed class SourceFileStructureTests
{
    [Fact]
    public void SourceFiles_ShouldContainOneTopLevelTypeWithMatchingFilename()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceRoots = new[]
        {
            Path.Combine(repositoryRoot, "sources", "back"),
            Path.Combine(repositoryRoot, "tests")
        };

        var violations = sourceRoots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            .Where(path => !IsBuildOutput(path))
            .Select(InspectSourceFile)
            .Where(violation => violation is not null)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Source-file structure violations were found:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    private static string? InspectSourceFile(string path)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path);
        var topLevelTypes = GetTopLevelTypes(syntaxTree.GetCompilationUnitRoot()).ToArray();

        if (topLevelTypes.Length == 0)
            return null;

        var fileName = Path.GetFileNameWithoutExtension(path);
        var declaredNames = topLevelTypes
            .Select(GetTypeName)
            .ToArray();

        if (topLevelTypes.Length == 1 &&
            string.Equals(declaredNames[0], fileName, StringComparison.Ordinal))
            return null;

        return $"{Path.GetRelativePath(FindRepositoryRoot(), path)}: " +
               $"expected one top-level type named '{fileName}', found [{string.Join(", ", declaredNames)}].";
    }

    private static string GetTypeName(MemberDeclarationSyntax declaration)
        => declaration switch
        {
            BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
            DelegateDeclarationSyntax type => type.Identifier.ValueText,
            _ => throw new ArgumentException("Unsupported top-level type declaration.", nameof(declaration))
        };

    private static IEnumerable<MemberDeclarationSyntax> GetTopLevelTypes(CompilationUnitSyntax compilationUnit)
        => GetTopLevelTypes(compilationUnit.Members);

    private static IEnumerable<MemberDeclarationSyntax> GetTopLevelTypes(
        SyntaxList<MemberDeclarationSyntax> members)
    {
        foreach (var member in members)
        {
            if (member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
            {
                yield return member;
                continue;
            }

            if (member is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                foreach (var type in GetTopLevelTypes(namespaceDeclaration.Members))
                    yield return type;
                continue;
            }

            if (member is FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
            {
                foreach (var type in GetTopLevelTypes(fileScopedNamespace.Members))
                    yield return type;
            }
        }
    }

    private static bool IsBuildOutput(string path)
        => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment =>
                string.Equals(segment, "bin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(segment, "obj", StringComparison.OrdinalIgnoreCase));

    private static string FindRepositoryRoot()
    {
        var startingDirectories = new[]
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(AppContext.BaseDirectory)
        };

        foreach (var start in startingDirectories)
        {
            for (var directory = start; directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Franz.slnx")))
                    return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not locate the Franz repository root (Franz.slnx). Run the architecture tests from a repository checkout.");
    }
}
