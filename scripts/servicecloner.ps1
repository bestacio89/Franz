
param(
    [string]$SourceProjectName = "Franz",
    [string]$TargetProjectName = "Orbita",
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

# Protected identifiers are never renamed, regardless of
# whether they occur in paths, source code, project files,
# configuration, solutions or documentation.
$ProtectedIdentifiers = @(
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
    $TargetRoot = Join-Path `
        $TargetProjectRootOutputDir `
        $TargetProjectName
}

$TargetRoot = [System.IO.Path]::GetFullPath($TargetRoot)

$SourceRoot = $SourceRoot.TrimEnd('\', '/')
$TargetRoot = $TargetRoot.TrimEnd('\', '/')

if ($SourceRoot -eq $TargetRoot) {
    throw "Source and target paths cannot be identical."
}

if (!(Test-Path -LiteralPath $SourceRoot -PathType Container)) {
    throw "Source path does not exist: $SourceRoot"
}

if (Test-Path -LiteralPath $TargetRoot) {
    throw "Target already exists: $TargetRoot"
}

# ============================================================
# HELPERS
# ============================================================

function Write-Step(
    [string]$Message
) {
    Write-Host ""
    Write-Host "============================================================"
    Write-Host " $Message"
    Write-Host "============================================================"
}

function Is-IgnoredPath(
    [string]$Path
) {
    foreach ($directory in $IgnoredDirectories) {
        $pattern = "(^|[\\/])$([regex]::Escape($directory))([\\/]|$)"

        if ($Path -match $pattern) {
            return $true
        }
    }

    return $false
}

function Is-ProtectedPath(
    [string]$Path
) {
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

function Write-FileSafe(
    [string]$Path,
    [string]$Content
) {
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

function Replace-Names(
    [string]$Content
) {
    if ([string]::IsNullOrEmpty($Content)) {
        return $Content
    }

    # Protect framework identifiers before any replacement.
    $protectedValues = @{}
    $index = 0

    foreach ($identifier in $ProtectedIdentifiers) {
        $token = "__CLONER_PROTECTED_$index`__"

        $protectedValues[$token] = $identifier

        $Content = [regex]::Replace(
            $Content,
            [regex]::Escape($identifier),
            $token,
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
        )

        $index++
    }

    # Rename the service-template identity.
    $Content = $Content.Replace(
        $SourceProjectName,
        $TargetProjectName
    )

    # Restore protected identifiers.
    foreach ($token in $protectedValues.Keys) {
        $Content = $Content.Replace(
            $token,
            $protectedValues[$token]
        )
    }

    return $Content
}

function Get-SafeFiles {
    Get-ChildItem `
        -LiteralPath $TargetRoot `
        -Recurse `
        -File |
    Where-Object {
        !(Is-IgnoredPath $_.FullName)
    }
}

function Get-SafeDirectories {
    Get-ChildItem `
        -LiteralPath $TargetRoot `
        -Recurse `
        -Directory |
    Where-Object {
        !(Is-IgnoredPath $_.FullName)
    }
}

# ============================================================
# COPY
# ============================================================

function Copy-Template {
    Write-Step "Copying template"

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

    $items = Get-ChildItem `
        -LiteralPath $TargetRoot `
        -Recurse `
        -Force |
    Where-Object {
        !(Is-IgnoredPath $_.FullName)
    } |
    Sort-Object {
        $_.FullName.Length
    } -Descending

    foreach ($item in $items) {

        if (Is-ProtectedPath $item.FullName) {
            continue
        }

        if ($item.Name -notlike "*$SourceProjectName*") {
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
    Write-Step "Updating source references"

    foreach ($file in Get-SafeFiles) {

        if (Is-ProtectedPath $file.FullName) {
            continue
        }

        if ($TextExtensions -notcontains $file.Extension) {
            continue
        }

        $content = [System.IO.File]::ReadAllText(
            $file.FullName
        )

        $updated = Replace-Names $content

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
    Write-Step "Normalizing projects"

    Get-ChildItem `
        -LiteralPath $TargetRoot `
        -Recurse `
        -Filter "*.csproj" |
    Where-Object {
        !(Is-IgnoredPath $_.FullName)
    } |
    ForEach-Object {

        if (Is-ProtectedPath $_.FullName) {
            return
        }

        $content = [System.IO.File]::ReadAllText(
            $_.FullName
        )

        # Only normalize identity properties when they match
        # the source service identity.
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

        $updated = Replace-Names $content

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

        if (Is-ProtectedPath $_.FullName) {
            return
        }

        $content = [System.IO.File]::ReadAllText(
            $_.FullName
        )

        $updated = Replace-Names $content

        if ($updated -eq $content) {
            return
        }

        Write-FileSafe `
            $_.FullName `
            $updated
    }
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

Copy-Template

Rename-Tree

Process-TextFiles

Process-Projects

Process-Solutions

Write-Host ""
Write-Host "============================================================"
Write-Host " CLONING COMPLETED"
Write-Host "============================================================"

if ($DryRun) {
    Write-Host "Mode: DRY RUN - no file contents were modified."
}
else {
    Write-Host "Mode: EXECUTION"
}
