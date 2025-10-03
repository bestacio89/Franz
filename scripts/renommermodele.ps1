param (
    [Alias('sp')]
    [parameter(
        HelpMessage = "Name of the existing project (Franz by default).",
        Mandatory = $false,
        ValueFromPipeline = $false)]
    [ValidateScript({            
            if (Test-Path "..\..\Franz\$_.sln".Trim().Trim('"')) { 
                return $true 
            } 
            else { 
                throw "The project solution template does not exist or has been renamed: \Something\$_.sln." 
            }
        })]
    [string]$SourceProjectName = "Franz",
    
    [Alias('tp')]
    [parameter(
        HelpMessage = "Name of the new project.",
        Mandatory = $false,
        ValueFromPipeline = $false)]
 
    [string]$TargetProjectName = "Something" ,
    
    [Alias('odtp')]
    [parameter(
        HelpMessage = "Output directory for the new project (default: ../).",
        Mandatory = $false,
        ValueFromPipeline = $false)]
    [string]$TargetProjectRootOutputDir = "",

    [Alias('ai')]
    [parameter(
        HelpMessage = "Relative path to the AssemblyInfo.cs file, e.g. "".\Properties\AssemblyInfo.cs"".`nSpecify empty string to skip AssemblyInfo processing.",
        Mandatory = $false,
        ValueFromPipeline = $false)]
    [AllowEmptyString()]
    [string]$RelativePathToAssemblyInfo = ""
)

$SourceProjectName = $SourceProjectName.Trim().Trim("")
$SourceProjectFullPath = "$(Resolve-Path "..")\"
$SourceSolutionFullPath = "$SourceProjectFullPath$SourceProjectName.sln"
$TargetProjectName = $TargetProjectName.Trim().Trim("")

if ($isSameSourceAndTargetProject = ($TargetProjectRootOutputDir.Trim() -eq "") ) {    
    $TargetProjectFullPath = "..\"
}
else {
    $TargetProjectFullPath = "$TargetProjectRootOutputDir$TargetProjectName\"
}

$TargetSolutionFullPath = "$TargetProjectFullPath$TargetProjectName.sln"

if ( (Test-Path $SourceSolutionFullPath -PathType Leaf) -ne $true) {
    throw "The source solution file was not found: $SourceSolutionFullPath"
}

function Start-ProceedOrExit {
    [OutputType([System.Void])]
    param
    (
        [string]$currentStepName
    )
    if ($?) { Write-Output "$currentStepName - OK" } else { Write-Output "SCRIPT ERROR! Exiting."; exit 1 } 
}

function Rename-Solution {
    [OutputType([System.Void])]
    param (
        [string]$targetProjectFullPath,
        [string]$sourceProjectName,
        [string]$targetProjectName
    )       

    Get-ChildItem -Path "$targetProjectFullPath" -Include "$sourceProjectName.*" -Recurse  -File `
    | ForEach-Object {
        $OldName = $_.Name;
        $NewName = $_.Name -replace "^$sourceProjectName\b", "$targetProjectName";
        
        if ($OldName -ne $NewName) {
            Rename-Item -Path $_.PSPath -NewName $NewName;
        }
    };

    Get-ChildItem -Path "$targetProjectFullPath" -Include "$sourceProjectName.*" -Recurse  -Directory `
    | ForEach-Object {
        $OldName = $_.Name;
        $NewName = $_.Name -replace "^$sourceProjectName\b", "$targetProjectName";
        
        if ($OldName -ne $NewName) {
            Rename-Item -Path $_.PSPath -NewName $NewName;
        }
    };
}

function Copy-BaseSolution {
    [OutputType([string])]
    param (
        [string] $sourceProjectFullPath,
        [string] $targetProjectFullPath
    )

    if ($sourceProjectFullPath -ne $targetProjectFullPath) {
        New-Item $targetProjectFullPath -ItemType Directory -Force | Out-Null;
        
        Copy-Item -Path "$sourceProjectFullPath*" $targetProjectFullPath -Recurse -Force -Exclude @(".git","scripts") | Out-Null;
        
        return $targetProjectFullPath;
    }

    return $sourceProjectFullPath;
}

function Save-CurrentDirectory {
    [OutputType([string])]
    param ()

    return Get-Location
}

function Restore-CurrentDirectory {
    [OutputType([System.Void])]
    param (
        [string]$currentLocation
    )

    Set-Location $currentLocation
}

function Rename-ReferencesInSolution {
    [OutputType([System.Void])]
    param (
        [string]$targetSolutionFullPath, 
        [string]$sourceProjectFullPath, 
        [string]$targetProjectName
    )     

    $UpdatedSolution = Get-Content -Path $targetSolutionFullPath `
    | ForEach-Object { 
        if ($_ -match ("\b(" + $sourceProjectFullPath + ")\b")) {             
            $_ -replace $($matches[1]), $targetProjectName 
        } 
        else { 
            $_ 
        } 
    }
    Set-Content -Path $targetSolutionFullPath -Value $UpdatedSolution -Encoding unicode;    
}

function Rename-AssemblyNameAndRootNamespace {
    [OutputType([System.Void])]
    param (
        [string]$targetSolutionFullPath,
        [string]$sourceProjectFullPath, 
        [string]$targetProjectName
    ) 

    $pattern = "$sourceProjectFullPath"

    Get-ChildItem -Path "$targetSolutionFullPath\*" -Recurse -Include *.csproj `
    | Select-Object @{ label = 'Path'; expression = { ($_.FullName) } }, @{ label = 'Content'; expression = { (Get-Content $_.FullName) } } `
    | ForEach-Object {            
        if ($_.Content -match ($pattern)) { 
            Set-Content -Path ($_.Path) -Value ($_.Content -replace $sourceProjectFullPath, $targetProjectName )
        }
    } 
}

function Rename-NamespacesAndUsingsFromClasses {
    [OutputType([System.Void])]
    param (
        [string]$targetSolutionFullPath,
        [string]$sourceProjectFullPath, 
        [string]$targetProjectName
    )

    $pattern = "(?:namespace|using)\W(\b$sourceProjectFullPath)"

    Get-ChildItem -Path "$targetSolutionFullPath\*" -Recurse -Include *.cs `
    | Select-Object @{ label = 'Path'; expression = { ($_.FullName) } }, @{ label = 'Content'; expression = { (Get-Content $_.FullName) } } `
    | ForEach-Object {                        
        if ($_.Content -match ($pattern)) { 
            Set-Content -Path ($_.Path) -Value ($_.Content -replace $sourceProjectFullPath, $targetProjectName )
        }
    } 
}

function Rename-AssemblyInfo {
    [OutputType([System.Void])]
    param (
        [string]$targetSolutionFullPath,
        [string]$sourceProjectFullPath, 
        [string]$targetProjectName
    )

    $RelativePathToAssemblyInfo = $RelativePathToAssemblyInfo.Trim().Trim('"')
    
    if ($RelativePathToAssemblyInfo) {
        $RelativePathToAssemblyInfo = "$targetSolutionFullPath$RelativePathToAssemblyInfo".Trim().Trim('"')
        if (!(Test-Path $RelativePathToAssemblyInfo)) { throw "The path does not exist: $RelativePathToAssemblyInfo." }
        else {
            (Get-Content $RelativePathToAssemblyInfo) |
            ForEach-Object { if ($_ -match "\b$sourceProjectFullPath") { $_ -replace $sourceProjectFullPath, $targetProjectName } else { $_ } } |
            Set-Content $RelativePathToAssemblyInfo
        }
    }
}

Write-Host "=====---- Starting configuration of your template."

$CurrentDirectory = Save-CurrentDirectory

Start-ProceedOrExit "---------.... Copying if target is different from source."
$SourceProjectFullPath = Copy-BaseSolution $SourceProjectFullPath $TargetProjectFullPath;

Start-ProceedOrExit "---------.... Renaming files and directories in the new solution: $SourceProjectName to $TargetProjectName."
Rename-Solution $TargetProjectFullPath $SourceProjectName $TargetProjectName

Start-ProceedOrExit "---------.... Updating references in the target solution file: $TargetSolutionFullPath."
Rename-ReferencesInSolution $TargetSolutionFullPath $SourceProjectName $TargetProjectName

Start-ProceedOrExit "---------.... Renaming assemblies and root namespaces in target: $TargetProjectFullPath."
Rename-AssemblyNameAndRootNamespace $TargetProjectFullPath $SourceProjectName $TargetProjectName

Start-ProceedOrExit "---------.... Renaming namespaces and usings in cs files: $TargetProjectFullPath."
Rename-NamespacesAndUsingsFromClasses $TargetProjectFullPath $SourceProjectName $TargetProjectName

Start-ProceedOrExit "---------.... Updating AssemblyInfo if it exists: $TargetProjectFullPath."
Rename-AssemblyInfo $TargetProjectFullPath $SourceProjectName $TargetProjectName

Restore-CurrentDirectory $CurrentDirectory

Write-Host "=====---- Template configuration completed."
