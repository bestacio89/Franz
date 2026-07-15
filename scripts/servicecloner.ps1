param(
    [string]$SourceProjectName = "Franz",
    [string]$TargetProjectName = "Nexus",
    [string]$TargetProjectRootOutputDir = "",
    [switch]$DryRun,
    [switch]$SkipSolutionProcessing
)

# ============================================================
# PROJECT TEMPLATE CLONER
# ============================================================

$ErrorActionPreference = "Stop"


# ============================================================
# CONFIGURATION
# ============================================================

$IgnoredDirectories = @(
    ".git",
    ".vs",
    "bin",
    "obj",
    "packages",
    "node_modules"
)

$ProtectedPaths = @(
    "Franz.Common"
)

$TextExtensions = @(
    ".cs",
    ".csproj",
    ".props",
    ".targets",
    ".sln",
    ".slnx",
    ".json",
    ".xml",
    ".yaml",
    ".yml",
    ".md",
    ".txt"
)


# ============================================================
# PATH RESOLUTION
# ============================================================

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

$SourceRoot = Resolve-Path (
    Join-Path $ScriptRoot ".."
)

$SourceRoot = [System.IO.Path]::GetFullPath($SourceRoot)

$RepositoryRoot = Split-Path $SourceRoot -Parent


if ([string]::IsNullOrWhiteSpace($TargetProjectRootOutputDir))
{
    $TargetRoot = Join-Path `
        $RepositoryRoot `
        $TargetProjectName
}
else
{
    $TargetRoot = Join-Path `
        $TargetProjectRootOutputDir `
        $TargetProjectName
}


$TargetRoot =
    [System.IO.Path]::GetFullPath($TargetRoot)



if ($SourceRoot.TrimEnd("\") -eq $TargetRoot.TrimEnd("\"))
{
    throw "Source and target paths cannot be identical."
}


if (!(Test-Path $SourceRoot))
{
    throw "Source path does not exist: $SourceRoot"
}



# ============================================================
# HELPERS
# ============================================================

function Write-Step(
    [string]$Message
)
{
    Write-Host ""
    Write-Host "---- $Message"
}


function Is-IgnoredPath(
    [string]$Path
)
{
    foreach ($dir in $IgnoredDirectories)
    {
        if ($Path -match "(\\|/)$([regex]::Escape($dir))(\\|/|$)")
        {
            return $true
        }
    }

    return $false
}


function Is-ProtectedPath(
    [string]$Path
)
{
    foreach ($protected in $ProtectedPaths)
    {
        if ($Path -match [regex]::Escape($protected))
        {
            return $true
        }
    }

    return $false
}


function Write-FileSafe(
    [string]$Path,
    [string]$Content
)
{
    if ($DryRun)
    {
        Write-Host "[DRY] Write $Path"
        return
    }


    $utf8 =
        New-Object System.Text.UTF8Encoding($false)


    [System.IO.File]::WriteAllText(
        $Path,
        $Content,
        $utf8
    )
}


function Replace-Names(
    [string]$Content
)
{
    return $Content.Replace(
        $SourceProjectName,
        $TargetProjectName
    )
}


function Get-SafeFiles()
{
    Get-ChildItem `
        -Path $TargetRoot `
        -Recurse `
        -File |
    Where-Object {

        !(Is-IgnoredPath $_.FullName)

    }
}


# ============================================================
# COPY
# ============================================================

function Copy-Template()
{
    Write-Step "Copying template"


    if (Test-Path $TargetRoot)
    {
        throw "Target already exists: $TargetRoot"
    }


    New-Item `
        -ItemType Directory `
        -Path $TargetRoot |
    Out-Null


    Get-ChildItem `
        -Path $SourceRoot `
        -Force |
    Where-Object {

        $_.Name -notin $IgnoredDirectories

    } |
    Copy-Item `
        -Destination $TargetRoot `
        -Recurse `
        -Force
}



# ============================================================
# RENAME TREE
# ============================================================

function Rename-Tree()
{
    Write-Step "Renaming directories and files"


    Get-ChildItem `
        -Path $TargetRoot `
        -Recurse `
        -Force |
    Where-Object {

        !(Is-IgnoredPath $_.FullName)

    } |
    Sort-Object {

        $_.FullName.Length

    } `
    -Descending |
    ForEach-Object {


        if (Is-ProtectedPath $_.FullName)
        {
            return
        }


        if ($_.Name -like "*$SourceProjectName*")
        {

            $newName =
                $_.Name.Replace(
                    $SourceProjectName,
                    $TargetProjectName
                )


            if ($DryRun)
            {
                Write-Host `
                    "[DRY] Rename $($_.FullName) -> $newName"
            }
            else
            {
                Rename-Item `
                    -Path $_.FullName `
                    -NewName $newName
            }
        }
    }
}



# ============================================================
# TEXT PROCESSING
# ============================================================

function Process-TextFiles()
{
    Write-Step "Updating source references"


    foreach ($file in Get-SafeFiles)
    {

        if (Is-ProtectedPath $file.FullName)
        {
            continue
        }


        if ($TextExtensions -notcontains $file.Extension)
        {
            continue
        }


        $content =
            Get-Content `
                -Path $file.FullName `
                -Raw


        $updated =
            Replace-Names $content


        if ($updated -ne $content)
        {
            Write-FileSafe `
                $file.FullName `
                $updated
        }
    }
}



# ============================================================
# CSPROJ NORMALIZATION
# ============================================================

function Process-Projects()
{
    Write-Step "Normalizing projects"


    Get-ChildItem `
        -Path $TargetRoot `
        -Recurse `
        -Filter "*.csproj" |
    ForEach-Object {


        if (Is-ProtectedPath $_.FullName)
        {
            return
        }


        $content =
            Get-Content `
                $_.FullName `
                -Raw


        $content =
            $content -replace
            "<AssemblyName>.*?</AssemblyName>",
            "<AssemblyName>$TargetProjectName</AssemblyName>"


        $content =
            $content -replace
            "<RootNamespace>.*?</RootNamespace>",
            "<RootNamespace>$TargetProjectName</RootNamespace>"


        $content =
            Replace-Names $content


        Write-FileSafe `
            $_.FullName `
            $content
    }
}



# ============================================================
# SOLUTION
# ============================================================

function Process-Solutions()
{
    if ($SkipSolutionProcessing)
    {
        return
    }


    Write-Step "Updating solutions"


    Get-ChildItem `
        -Path $TargetRoot `
        -Recurse `
        -Include "*.sln","*.slnx" |
    ForEach-Object {


        $content =
            Get-Content `
                $_.FullName `
                -Raw


        $content =
            Replace-Names $content


        Write-FileSafe `
            $_.FullName `
            $content
    }
}



# ============================================================
# EXECUTION
# ============================================================

Write-Host ""
Write-Host "=========================================="
Write-Host " TEMPLATE CLONER"
Write-Host "=========================================="
Write-Host "Source : $SourceRoot"
Write-Host "Target : $TargetRoot"


Copy-Template

Rename-Tree

Process-TextFiles

Process-Projects

Process-Solutions


Write-Host ""
Write-Host "=========================================="
Write-Host " COMPLETED"
Write-Host "=========================================="

if ($DryRun)
{
    Write-Host "Mode: DRY RUN"
}