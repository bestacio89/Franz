param (
    [string]$SourceProjectName = "Franz",
    [string]$TargetProjectName = "Something",
    [string]$TargetProjectRootOutputDir = "",
    [string]$RelativePathToAssemblyInfo = "",
    [switch]$DryRun
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
$SourceSolutionFullPath = "$SourceProjectFullPath$SourceProjectName.sln"

if ($TargetProjectRootOutputDir.Trim() -eq "") {
    $TargetProjectFullPath = "..\"
} else {
    $TargetProjectFullPath = "$TargetProjectRootOutputDir$TargetProjectName\"
}

$TargetSolutionFullPath = "$TargetProjectFullPath$TargetProjectName.sln"

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
    }
    else {
        $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllLines($path, $content, $utf8NoBom)
    }
}

# ================================
# COPY BASE SOLUTION
# ================================

function Copy-Solution {
    Write-Step "Copying solution..."

    if ($SourceProjectFullPath -ne $TargetProjectFullPath) {
        if (!$DryRun) {
            New-Item $TargetProjectFullPath -ItemType Directory -Force | Out-Null
            Copy-Item "$SourceProjectFullPath*" $TargetProjectFullPath -Recurse -Force -Exclude @(".git", "scripts")
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

            $newName = $_.Name -replace "^$SourceProjectName\b", $TargetProjectName

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
    param (
        [string]$filePath
    )

    $content = Get-Content $filePath -Raw

    # Replace base project name
    $updated = $content -replace "\b$SourceProjectName\b", $TargetProjectName

    # Restore protected namespaces if accidentally modified
    foreach ($ns in $ProtectedNamespaces) {
        $updated = $updated -replace "\b$TargetProjectName\.$($ns.Split('.')[-1])\b", $ns
    }

    Apply-Change $filePath $updated
}

# ================================
# PROCESS FILE TYPES
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

function Process-SolutionFile {
    Write-Step "Processing solution file..."

    if (!(Test-Path $TargetSolutionFullPath)) {
        Write-Host "Solution file not found, skipping."
        return
    }

    $content = Get-Content $TargetSolutionFullPath -Raw

    $updated = $content -replace "\b$SourceProjectName\b", $TargetProjectName

    Apply-Change $TargetSolutionFullPath $updated
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
    $updated = $content -replace "\b$SourceProjectName\b", $TargetProjectName

    Apply-Change $path $updated
}

# ================================
# EXECUTION
# ================================

Write-Host "===== SAFE TEMPLATE CLONING STARTED ====="

Copy-Solution
Rename-FilesAndFolders
Process-SolutionFile
Process-ProjectFiles
Process-CodeFiles
Process-AssemblyInfo

Write-Host "===== COMPLETED SUCCESSFULLY ====="