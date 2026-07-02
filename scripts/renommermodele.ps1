param (
    [string]$SourceProjectName = "Franz",
    [string]$TargetProjectName = "Something",
    [string]$TargetProjectRootOutputDir = "",
    [string]$RelativePathToAssemblyInfo = "",
    [switch]$DryRun,
    [switch]$SkipSolutionProcessing
)

# =========================================================
# ROSLYN LOADING
# =========================================================

Add-Type -AssemblyName "Microsoft.CodeAnalysis"
Add-Type -AssemblyName "Microsoft.CodeAnalysis.CSharp"

# =========================================================
# PATH SETUP
# =========================================================

$SourceProjectFullPath = "$(Resolve-Path "..")\"
$SourceSolutionFullPath = "$SourceProjectFullPath$SourceProjectName.slnx"

if ($TargetProjectRootOutputDir.Trim() -eq "") {
    $TargetProjectFullPath = "..\"
} else {
    $TargetProjectFullPath = "$TargetProjectRootOutputDir$TargetProjectName\"
}

$TargetSolutionFullPath = "$TargetProjectFullPath$TargetProjectName.slnx"

$EscapedSource = [regex]::Escape($SourceProjectName)

if (!(Test-Path $SourceSolutionFullPath)) {
    throw "Source solution not found: $SourceSolutionFullPath"
}

# =========================================================
# UTILITIES
# =========================================================

function Write-Step($msg) {
    Write-Host "---- $msg"
}

function Apply-Change($path, $content) {
    if ($DryRun) {
        Write-Host "[DRY RUN] $path"
        return
    }

    $utf8 = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($path, $content, $utf8)
}

function Is-FranzFrameworkFile {
    param([string]$path)

    # NEVER TOUCH FRAMEWORK LAYER
    return $path -match "Franz\.Common"
}

# =========================================================
# ROSLYN REWRITER (ONLY SAFE STRUCTURAL ELEMENTS)
# =========================================================

Add-Type -TypeDefinition @"
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class SafeNamespaceRewriter : CSharpSyntaxRewriter
{
    private readonly string _source;
    private readonly string _target;

    public SafeNamespaceRewriter(string source, string target)
    {
        _source = source;
        _target = target;
    }

    public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        var name = node.Name.ToString();

        if (name.StartsWith(_source))
        {
            return node.WithName(
                SyntaxFactory.ParseName(name.Replace(_source, _target))
            );
        }

        return base.VisitNamespaceDeclaration(node);
    }

    public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
    {
        if (node.Name == null)
            return base.VisitUsingDirective(node);

        var name = node.Name.ToString();

        if (name.StartsWith(_source))
        {
            return node.WithName(
                SyntaxFactory.ParseName(name.Replace(_source, _target))
            );
        }

        return base.VisitUsingDirective(node);
    }
}
"@ -Language CSharp

function Rewrite-CsFile {
    param([string]$filePath)

    if (Is-FranzFrameworkFile $filePath) {
        return
    }

    $code = [System.IO.File]::ReadAllText($filePath)

    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($code)
    $root = $tree.GetRoot()

    $rewriter = New-Object SafeNamespaceRewriter $SourceProjectName $TargetProjectName
    $newRoot = $rewriter.Visit($root)

    $result = $newRoot.NormalizeWhitespace().ToFullString()

    Apply-Change $filePath $result
}

# =========================================================
# COPY SOLUTION
# =========================================================

function Copy-Solution {
    Write-Step "Copying solution..."

    if (!(Test-Path $TargetProjectFullPath)) {
        New-Item $TargetProjectFullPath -ItemType Directory -Force | Out-Null
    }

    if ($DryRun) { return }

    Copy-Item "$SourceProjectFullPath*" $TargetProjectFullPath `
        -Recurse -Force `
        -Exclude @(".git", "bin", "obj")
}

# =========================================================
# RENAME FILES / FOLDERS
# =========================================================

function Rename-FilesAndFolders {
    Write-Step "Renaming files/folders..."

    Get-ChildItem $TargetProjectFullPath -Recurse -Force |
    Sort-Object FullName -Descending |
    ForEach-Object {

        if ($_.FullName -match "Franz\.Common") {
            return
        }

        if ($_.Name -like "$SourceProjectName*") {

            $newName = $_.Name -replace "^$EscapedSource", $TargetProjectName

            if ($DryRun) {
                Write-Host "[DRY RUN] $($_.FullName) -> $newName"
            }
            else {
                Rename-Item $_.FullName -NewName $newName
            }
        }
    }
}

# =========================================================
# SOLUTION FILE (.slnx)
# =========================================================

function Process-SolutionFile {
    Write-Step "Processing solution..."

    if ($SkipSolutionProcessing) {
        return
    }

    if (!(Test-Path $TargetSolutionFullPath)) {
        return
    }

    $content = Get-Content $TargetSolutionFullPath -Raw

    # ONLY project identity replacement
    $updated = $content -replace "$EscapedSource(?=\.csproj)", $TargetProjectName

    Apply-Change $TargetSolutionFullPath $updated
}

# =========================================================
# CSPROJ FILES
# =========================================================

function Process-ProjectFiles {
    Write-Step "Processing csproj..."

    Get-ChildItem $TargetProjectFullPath -Recurse -Include *.csproj |
    ForEach-Object {

        $content = Get-Content $_.FullName -Raw

        $content = [regex]::Replace($content, "<AssemblyName>.*?</AssemblyName>",
            "<AssemblyName>$TargetProjectName</AssemblyName>")

        $content = [regex]::Replace($content, "<RootNamespace>.*?</RootNamespace>",
            "<RootNamespace>$TargetProjectName</RootNamespace>")

        $content = [regex]::Replace($content,
            "$EscapedSource(?=\.csproj)",
            $TargetProjectName)

        Apply-Change $_.FullName $content
    }
}

# =========================================================
# C# CODE (SAFE ONLY)
# =========================================================

function Process-CodeFiles {
    Write-Step "Processing C# files (safe mode)..."

    Get-ChildItem $TargetProjectFullPath -Recurse -Include *.cs |
    ForEach-Object {
        Rewrite-CsFile $_.FullName
    }
}

# =========================================================
# ASSEMBLY INFO
# =========================================================

function Process-AssemblyInfo {
    if ([string]::IsNullOrWhiteSpace($RelativePathToAssemblyInfo)) {
        return
    }

    Write-Step "Processing AssemblyInfo..."

    $path = "$TargetProjectFullPath$RelativePathToAssemblyInfo"

    if (!(Test-Path $path)) {
        throw "AssemblyInfo not found: $path"
    }

    $content = Get-Content $path -Raw
    $content = $content -replace $EscapedSource, $TargetProjectName

    Apply-Change $path $content
}

# =========================================================
# EXECUTION
# =========================================================

Write-Host "===== API TEMPLATE CLONER (SAFE MODE) ====="

Copy-Solution
Rename-FilesAndFolders
Process-SolutionFile
Process-ProjectFiles
Process-CodeFiles
Process-AssemblyInfo

Write-Host "===== DONE ====="

Write-Host ""
Write-Host "Press any key to exit..."
[System.Console]::ReadKey($true) | Out-Null