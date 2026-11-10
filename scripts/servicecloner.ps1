
[CmdletBinding()]
param(
    [string]$SourceProjectName = "Franz",
    [string]$TargetProjectName = "Orbita",
    [string]$TargetProjectRootOutputDir = "",
    [switch]$DryRun,
    [switch]$SkipSolutionProcessing
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

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

# Explicit framework namespaces and identifiers.
# These are never renamed.
$ProtectedIdentifiers = @(
    "Franz.Common"
)

# Framework-owned identifiers beginning with these prefixes
# are preserved in source content.
#
# Examples:
# FranzMediator
# IFranzRepository
# AddFranzServices
# ConfigureFranz
# FranzEntity
#
# Template namespaces such as Franz.Domain are intentionally
# excluded and will be renamed.
$ProtectedIdentifierPrefixes = @(
    "Franz",
    "IFranz"
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

$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)

# ============================================================
# VALIDATION
# ============================================================

if ([string]::IsNullOrWhiteSpace($SourceProjectName)) {
    throw "SourceProjectName cannot be empty."
}

if ([string]::IsNullOrWhiteSpace($TargetProjectName)) {
    throw "TargetProjectName cannot be empty."
}

if ($SourceProjectName -eq $TargetProjectName) {
    throw "Source and target project names must be different."
}

if ($SourceProjectName -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
    throw "SourceProjectName must be a valid identifier."
}

if ($TargetProjectName -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
    throw "TargetProjectName must be a valid identifier."
}

# ============================================================
# PATH RESOLUTION
# ============================================================

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

$SourceRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $ScriptRoot "..")
)

$RepositoryRoot = Split-Path $SourceRoot -Parent

if ([string]::IsNullOrWhiteSpace($TargetProjectRootOutputDir)) {
    $TargetRoot = Join-Path $RepositoryRoot $TargetProjectName
}
else {
    $TargetRoot = Join-Path $TargetProjectRootOutputDir $TargetProjectName
}

$TargetRoot = [System.IO.Path]::GetFullPath($TargetRoot)

$SourceRoot = $SourceRoot.TrimEnd('\', '/')
$TargetRoot = $TargetRoot.TrimEnd('\', '/')

if ([string]::Equals(
    $SourceRoot,
    $TargetRoot,
    [System.StringComparison]::OrdinalIgnoreCase
)) {
    throw "Source and target paths cannot be identical."
}

if (!(Test-Path -LiteralPath $SourceRoot -PathType Container)) {
    throw "Source path does not exist: $SourceRoot"
}

if (Test-Path -LiteralPath $TargetRoot) {
    throw "Target already exists: $TargetRoot"
}

$SourcePrefix = $SourceRoot + [System.IO.Path]::DirectorySeparatorChar

if ($TargetRoot.StartsWith(
    $SourcePrefix,
    [System.StringComparison]::OrdinalIgnoreCase
)) {
    throw "Target cannot be created inside the source directory."
}

# ============================================================
# HELPERS
# ============================================================

function Write-Step {
    param([string]$Message)

    Write-Host ""
    Write-Host "============================================================"
    Write-Host " $Message"
    Write-Host "============================================================"
}

function Is-IgnoredPath {
    param([string]$Path)

    foreach ($directory in $IgnoredDirectories) {
        $pattern = "(^|[\\/])$([regex]::Escape($directory))([\\/]|$)"

        if ($Path -match $pattern) {
            return $true
        }
    }

    return $false
}

function Is-ProtectedPath {
    param([string]$Path)

    foreach ($identifier in $ProtectedIdentifiers) {
        if ($Path.IndexOf(
            $identifier,
            [System.StringComparison]::OrdinalIgnoreCase
        ) -ge 0) {
            return $true
        }
    }

    return $false
}

function Write-FileSafe {
    param(
        [string]$Path,
        [string]$Content
    )

    if ($DryRun) {
        Write-Host "[DRY] Write $Path"
        return
    }

    [System.IO.File]::WriteAllText(
        $Path,
        $Content,
        $Utf8NoBom
    )
}

# ============================================================
# FRAMEWORK IDENTIFIER PROTECTION
# ============================================================

function Protect-FrameworkIdentifiers {
    param([string]$Content)

    $protectedValues = @{}
    $index = 0

    # Protect explicitly declared framework namespaces first.
    foreach ($identifier in $ProtectedIdentifiers) {

        $pattern = [regex]::Escape($identifier)

        $Content = [regex]::Replace(
            $Content,
            $pattern,
            {
                param($match)

                $token = "__CLONER_PROTECTED_$index`__"
                $index++

                $protectedValues[$token] = $match.Value

                return $token
            },
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
        )
    }

    # Protect framework-owned identifiers beginning with Franz
    # or IFranz, but not template namespaces such as Franz.Domain.
    #
    # The negative lookahead excludes known template namespace
    # segments, allowing them to be renamed.
    #
    # Identifier examples:
    # FranzMediator
    # IFranzRepository
    # AddFranzServices
    # ConfigureFranz
    # FranzEntity
    #
    # Namespace examples:
    # Franz.Common.Mediator (protected above)
    # Franz.Domain (not protected)
    # Franz.Application (not protected)

    $excludedSegments = @(
        "Domain",
        "Application",
        "Infrastructure",
        "Api",
        "API",
        "Contracts",
        "Presentation",
        "Persistence",
        "Tests",
        "Web",
        "Worker",
        "Host",
        "Abstractions"
    )

    $excludedPattern = ($excludedSegments | ForEach-Object {
        [regex]::Escape($_)
    }) -join "|"

    $identifierPattern = (
        '(?<![A-Za-z0-9_])' +
        '(?:I?Franz[A-Za-z0-9_]*)' +
        '(?!\.(?:' + $excludedPattern + ')(?![A-Za-z0-9_]))' +
        '(?![A-Za-z0-9_])'
    )

    $Content = [regex]::Replace(
        $Content,
        $identifierPattern,
        {
            param($match)

            $token = "__CLONER_PROTECTED_$index`__"
            $index++

            $protectedValues[$token] = $match.Value

            return $token
        }
    )

    return @{
        Content = $Content
        Tokens  = $protectedValues
    }
}

function Restore-FrameworkIdentifiers {
    param(
        [string]$Content,
        [hashtable]$Tokens
    )

    foreach ($token in $Tokens.Keys) {
        $Content = $Content.Replace(
            $token,
            $Tokens[$token]
        )
    }

    return $Content
}

# ============================================================
# PROJECT IDENTITY REPLACEMENT
# ============================================================

function Replace-ProjectIdentity {
    param([string]$Content)

    if ([string]::IsNullOrEmpty($Content)) {
        return $Content
    }

    $protected = Protect-FrameworkIdentifiers $Content

    $Content = $protected.Content

    # Rename template identity.
    $Content = $Content.Replace(
        $SourceProjectName,
        $TargetProjectName
    )

    # Restore framework identifiers exactly.
    $Content = Restore-FrameworkIdentifiers `
        $Content `
        $protected.Tokens

    return $Content
}

# ============================================================
# SAFE FILE ENUMERATION
# ============================================================

function Get-SafeFiles {

    Get-ChildItem `
        -LiteralPath $TargetRoot `
        -Recurse `
        -File `
        -Force |

    Where-Object {
        !(Is-IgnoredPath $_.FullName)
    }
}

function Get-SafeDirectories {

    Get-ChildItem `
        -LiteralPath $TargetRoot `
        -Recurse `
        -Directory `
        -Force |

    Where-Object {
        !(Is-IgnoredPath $_.FullName)
    }
}

# ============================================================
# COPY
# ============================================================

function Copy-Template {

    Write-Step "Copying template"

    if ($DryRun) {

        Write-Host "[DRY] Create target directory: $TargetRoot"

        Get-ChildItem `
            -LiteralPath $SourceRoot `
            -Force |

        Where-Object {
            $_.Name -notin $IgnoredDirectories
        } |

        ForEach-Object {
            Write-Host "[DRY] Copy $($_.FullName)"
        }

        return
    }

    New-Item `
        -ItemType Directory `
        -Path $TargetRoot |
    Out-Null

    Get-ChildItem `
        -LiteralPath $SourceRoot `
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

function Rename-Tree {

    Write-Step "Renaming directories and files"

    $items = @(
        Get-ChildItem `
            -LiteralPath $TargetRoot `
            -Recurse `
            -Force |

        Where-Object {
            !(Is-IgnoredPath $_.FullName)
        } |

        Sort-Object {
            $_.FullName.Length
        } -Descending
    )

    foreach ($item in $items) {

        if (Is-ProtectedPath $item.Name) {
            continue
        }

        if ($item.Name.IndexOf(
            $SourceProjectName,
            [System.StringComparison]::Ordinal
        ) -lt 0) {
            continue
        }

        $newName = $item.Name.Replace(
            $SourceProjectName,
            $TargetProjectName
        )

        if ($DryRun) {
            Write-Host "[DRY] Rename $($item.FullName) -> $newName"
            continue
        }

        Rename-Item `
            -LiteralPath $item.FullName `
            -NewName $newName
    }
}

# ============================================================
# TEXT PROCESSING
# ============================================================

function Process-TextFiles {

    Write-Step "Updating project references"

    foreach ($file in Get-SafeFiles) {

        if (Is-ProtectedPath $file.Name) {
            continue
        }

        if ($TextExtensions -notcontains $file.Extension) {
            continue
        }

        $content = [System.IO.File]::ReadAllText(
            $file.FullName
        )

        $updated = Replace-ProjectIdentity $content

        if ($updated -eq $content) {
            continue
        }

        Write-FileSafe `
            $file.FullName `
            $updated
    }
}

# ============================================================
# CSPROJ NORMALIZATION
# ============================================================

function Process-Projects {

    Write-Step "Normalizing project identities"

    Get-ChildItem `
        -LiteralPath $TargetRoot `
        -Recurse `
        -Filter "*.csproj" `
        -File |

    Where-Object {
        !(Is-IgnoredPath $_.FullName)
    } |

    ForEach-Object {

        if (Is-ProtectedPath $_.Name) {
            return
        }

        $content = [System.IO.File]::ReadAllText(
            $_.FullName
        )

        $content = [regex]::Replace(
            $content,
            "(<AssemblyName>)$([regex]::Escape($SourceProjectName))(?=</AssemblyName>)",
            "`$1$TargetProjectName"
        )

        $content = [regex]::Replace(
            $content,
            "(<RootNamespace>)$([regex]::Escape($SourceProjectName))(?=</RootNamespace>)",
            "`$1$TargetProjectName"
        )

        $updated = Replace-ProjectIdentity $content

        if ($updated -eq $content) {
            return
        }

        Write-FileSafe `
            $_.FullName `
            $updated
    }
}

# ============================================================
# SOLUTION PROCESSING
# ============================================================

function Process-Solutions {

    if ($SkipSolutionProcessing) {
        Write-Host "Solution processing skipped."
        return
    }

    Write-Step "Updating solutions"

    Get-ChildItem `
        -LiteralPath $TargetRoot `
        -Recurse `
        -File |

    Where-Object {
        $_.Extension -in @(".sln", ".slnx") -and
        !(Is-IgnoredPath $_.FullName)
    } |

    ForEach-Object {

        if (Is-ProtectedPath $_.Name) {
            return
        }

        $content = [System.IO.File]::ReadAllText(
            $_.FullName
        )

        $updated = Replace-ProjectIdentity $content

        if ($updated -eq $content) {
            return
        }

        Write-FileSafe `
            $_.FullName `
            $updated
    }
}

# ============================================================
# POST-PROCESS VALIDATION
# ============================================================

function Validate-Clone {

    Write-Step "Validating cloned project"

    $remaining = @()

    foreach ($file in Get-SafeFiles) {

        if ($TextExtensions -notcontains $file.Extension) {
            continue
        }

        if (Is-ProtectedPath $file.Name) {
            continue
        }

        $content = [System.IO.File]::ReadAllText(
            $file.FullName
        )

        if ($content.Contains($SourceProjectName)) {
            $remaining += $file.FullName
        }
    }

    if ($remaining.Count -gt 0) {

        Write-Warning "Source identity references remain:"

        foreach ($path in $remaining) {
            Write-Warning $path
        }
    }
    else {
        Write-Host "No remaining source identity references found."
    }

    $frameworkReferences = @(
        Get-SafeFiles |
        Where-Object {
            $_.Extension -in $TextExtensions
        } |
        Select-String -Pattern "Franz\.Common" -List
    )

    Write-Host "Files retaining Franz.Common: $($frameworkReferences.Count)"

    Write-Host "Validation completed."
}

# ============================================================
# EXECUTION
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " SERVICE TEMPLATE CLONER"
Write-Host "============================================================"

Write-Host "Source       : $SourceRoot"
Write-Host "Target       : $TargetRoot"
Write-Host "Source name  : $SourceProjectName"
Write-Host "Target name  : $TargetProjectName"
Write-Host "Dry run      : $DryRun"
Write-Host "Protected    : $($ProtectedIdentifiers -join ', ')"
Write-Host "Prefixes     : $($ProtectedIdentifierPrefixes -join ', ')"

Copy-Template

if (!$DryRun) {

    Rename-Tree

    Process-TextFiles

    Process-Projects

    Process-Solutions

    Validate-Clone
}
else {

    Write-Step "Dry run completed"

    Write-Host "No files or directories were modified."
}

Write-Host ""
Write-Host "============================================================"
Write-Host " CLONING COMPLETED"
Write-Host "============================================================"

if ($DryRun) {
    Write-Host "Mode: DRY RUN"
}
else {
    Write-Host "Mode: EXECUTION"
}
