param (
    [string]$SourceProjectName = "Franz",
    [string]$TargetProjectName = "Something",
    [string]$TargetProjectRootOutputDir = "",
    [string]$RelativePathToAssemblyInfo = "",
    [switch]$DryRun,
    [switch]$SkipSolutionProcessing
)

# ================================
# CONFIGURATION
# ================================

$ProtectedNamespaces = @(
    "Franz.Common"
)

# ================================
# PATH SETUP
# ================================

$SourceProjectFullPath = "$(Resolve-Path "..")\"
$SourceSolutionFullPath = "$SourceProjectFullPath$SourceProjectName.slnx"

if ($TargetProjectRootOutputDir.Trim() -eq "") {
    $TargetProjectFullPath = "..\"
} else {
    $TargetProjectFullPath = "$TargetProjectRootOutputDir$TargetProjectName\"
}

$TargetSolutionFullPath = "$TargetProjectFullPath$TargetProjectName.slnx"

if (!(Test-Path $SourceSolutionFullPath)) {
    throw "Source solution not found: $SourceSolutionFullPath"
}

# ================================
# UTILITIES
# ================================

function Write-Step($msg) {
    Write-Host "---- $msg"
}

function Apply-Change($path, $content) {
    if ($DryRun) {
        Write-Host "[DRY RUN] Would update: $path"
        return
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllLines($path, $content, $utf8NoBom)
}

function Safe-Replace([string]$content, [string]$source, [string]$target) {
    $pattern = [regex]::Escape($source)
    return ($content -replace $pattern, $target)
}

# ================================
# COPY BASE SOLUTION
# ================================

function Copy-Solution {
    Write-Step "Copying solution..."

    if ($SourceProjectFullPath -ne $TargetProjectFullPath) {
        if (!$DryRun) {
            New-Item $TargetProjectFullPath -ItemType Directory -Force | Out-Null

            Copy-Item "$SourceProjectFullPath*" $TargetProjectFullPath `
                -Recurse -Force `
                -Exclude @(".git", "bin", "obj", "scripts")
        }
    }
}

# ================================
# RENAME FILES & FOLDERS
# ================================

function Rename-FilesAndFolders {
    Write-Step "Renaming files and folders..."

    Get-ChildItem -Path $TargetProjectFullPath -Recurse |
    Sort-Object FullName -Descending |
    ForEach-Object {

        foreach ($ns in $ProtectedNamespaces) {
            if ($_.FullName -like "*$ns*") {
                return
            }
        }

        if ($_.Name -like "$SourceProjectName*") {

            $newName = $_.Name -replace "^$SourceProjectName", $TargetProjectName

            if ($DryRun) {
                Write-Host "[DRY RUN] Rename $($_.FullName) -> $newName"
            }
            else {
                Rename-Item $_.FullName -NewName $newName
            }
        }
    }
}

# ================================
# SAFE CONTENT REPLACEMENT
# ================================

function Replace-Content {
    param ([string]$filePath)

    $content = Get-Content $filePath -Raw

    $updated = Safe-Replace $content $SourceProjectName $TargetProjectName

    # Restore protected namespaces
    foreach ($ns in $ProtectedNamespaces) {
        $leaf = $ns.Split('.')[-1]
        $updated = $updated -replace "\b$TargetProjectName\.$leaf\b", $ns
    }

    Apply-Change $filePath $updated
}

# ================================
# FILE PROCESSING
# ================================

function Process-CodeFiles {
    Write-Step "Processing .cs files..."

    Get-ChildItem $TargetProjectFullPath -Recurse -Include *.cs |
    ForEach-Object {
        Replace-Content $_.FullName
    }
}

function Process-ProjectFiles {
    Write-Step "Processing .csproj files..."

    Get-ChildItem $TargetProjectFullPath -Recurse -Include *.csproj |
    ForEach-Object {
        Replace-Content $_.FullName
    }
}

# ================================
# SLNX PROCESSING (SAFE MODE)
# ================================

function Process-SolutionFile {
    Write-Step "Processing .slnx solution file..."

    if ($SkipSolutionProcessing) {
        Write-Host "Skipping solution processing (flag enabled)."
        return
    }

    if (!(Test-Path $TargetSolutionFullPath)) {
        Write-Host "Solution file not found, skipping."
        return
    }

    # SAFETY: treat .slnx as structured artifact, not regex text
    try {
        $content = Get-Content $TargetSolutionFullPath -Raw

        $updated = Safe-Replace $content $SourceProjectName $TargetProjectName

        Apply-Change $TargetSolutionFullPath $updated
    }
    catch {
        throw "Failed processing .slnx safely: $($_.Exception.Message)"
    }
}

# ================================
# ASSEMBLY INFO (OPTIONAL)
# ================================

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
    $updated = Safe-Replace $content $SourceProjectName $TargetProjectName

    Apply-Change $path $updated
}

# ================================
# EXECUTION
# ================================

Write-Host "===== SAFE .SLNX TEMPLATE CLONING STARTED ====="

Copy-Solution
Rename-FilesAndFolders
Process-SolutionFile
Process-ProjectFiles
Process-CodeFiles
Process-AssemblyInfo

Write-Host "===== COMPLETED SUCCESSFULLY ====="