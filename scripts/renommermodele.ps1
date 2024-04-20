param (
    [Alias('sp')]
    [parameter(
        HelpMessage = "Nom du projet existant (Hubbix.Exemple par défault).",
        Mandatory = $false,
        ValueFromPipeline = $false)]
    [ValidateScript({            
            if (Test-Path "..\..\Franz\$_.sln".Trim().Trim('"')) { 
                return $true 
            } 
            else { 
                throw "La solution modèle du projet n'existe pas ou été renommé : \MicroserviceArchitecture\$_.sln." 
            }
        })]
    [string]$NomProjetSource = "Minerva",
    
    [Alias('tp')]
    [parameter(
        HelpMessage = "Nom du nouveau projet.",
        Mandatory = $false,
        ValueFromPipeline = $false)]
 
    [string]$NomProjetCible = "Franz" ,
    
    [Alias('odtp')]
    [parameter(
        HelpMessage = "Chemin de sortie du nouveau projet. (par défault: ../) ",
        Mandatory = $false,
        ValueFromPipeline = $false)]
    [string]$RepertoireRacineSortieProjetCible = "",

    [Alias('ai')]
    [parameter(
        HelpMessage = "Chemin relatif vers AssemblyInfo.cs file, e.g. "".\Properties\AssemblyInfo.cs"".`nSpecifier chaine vide par ne pas traiter l'AssemblyInfo.",
        Mandatory = $false,
        ValueFromPipeline = $false)]
    [AllowEmptyString()]
    [string]$CheminRelatifVersAssemblyInfo = ""
)

$NomProjetSource = $NomProjetSource.Trim().Trim("")
$NomProjetSourceComplet = "$(Resolve-Path "..")\"
$CheminCompletSolutionSource = "$NomProjetSourceComplet$NomProjetSource.sln"
$NomProjetCible = $NomProjetCible.Trim().Trim("")


if ($isSameSourceAndCibleProjet = ($RepertoireRacineSortieProjetCible.Trim() -eq "") ) {    
    $NomProjetCibleComplet = "..\"
}
else {
    $NomProjetCibleComplet = "$RepertoireRacineSortieProjetCible$NomProjetCible\"
}

$CheminCompletSolutionCible = "$NomProjetCibleComplet$NomProjetCible.sln"

if ( (Test-Path $CheminCompletSolutionSource -PathType Leaf) -ne $true) {
    throw "Le fichier de solution source n'a pas été trouvé : $CheminCompletSolutionSource"
}

function Start-ProceedOrExit {
    [OutputType([System.Void])]
    param
    (
        [string]$nomEtapeEnCours
    )
    if ($?) { Write-Output "$nomEtapeEnCours - OK" } else { Write-Output "Script EN ERREUR ! Sortie du traitement."; exit 1 } 
}


function Rename-Solution {
    [OutputType([System.Void])]
    param (
        [string]$nomProjetCibleComplet,
        [string]$nomProjetSource,
        [string]$nomProjetCible
    )       

    Get-ChildItem -Path "$nomProjetCibleComplet" -Include "$nomProjetSource.*" -Recurse  -File `
    | ForEach-Object {
        $AncienNom = $_.Name;
        $NouveauNom = $_.Name -replace "^$nomProjetSource\b", "$nomProjetCible";
        
        if ($AncienNom -ne $NouveauNom) {
            Rename-Item -Path $_.PSPath -NewName $NouveauNom;
        }
    };

    Get-ChildItem -Path "$nomProjetCibleComplet" -Include "$nomProjetSource.*" -Recurse  -Directory `
    | ForEach-Object {
        $AncienNom = $_.Name;
        $NouveauNom = $_.Name -replace "^$nomProjetSource\b", "$nomProjetCible";
        
        if ($AncienNom -ne $NouveauNom) {
            Rename-Item -Path $_.PSPath -NewName $NouveauNom;
        }
    };
    
}

function Copy-BaseSolution {
    [OutputType([string])]
    param (
        [string] $nomProjetSourceComplet,
        [string] $nomProjetCibleComplet
    )

    if ($nomProjetSourceComplet -ne $nomProjetCibleComplet) {
        New-Item $nomProjetCibleComplet -ItemType Directory -Force | Out-Null;
        
        Copy-Item -Path "$nomProjetSourceComplet*" $nomProjetCibleComplet -Recurse -Force -Exclude @(".git","scripts") | Out-Null;
        
        return $nomProjetCibleComplet;
    }

    return $nomProjetSourceComplet;
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
        [string]$cheminCompletSolutionCible, 
        [string]$nomProjetSourceComplet, 
        [string]$nomProjetCible
    )     

    $SolutionMiseAJour = Get-Content -Path $cheminCompletSolutionCible `
    | ForEach-Object { 
        if ($_ -match ("\b(" + $nomProjetSourceComplet + ")\b")) {             
            $_ -replace $($matches[1]), $nomProjetCible 
        } 
        else { 
            $_ 
        } 
    }
    Set-Content -Path $cheminCompletSolutionCible -Value $SolutionMiseAJour -Encoding unicode;    
}

function Rename-AssemblyNameAndRootNamespace {
    [OutputType([System.Void])]
    param (
        [string]$cheminCompletSolutionCible,
        [string]$nomProjetSourceComplet, 
        [string]$nomProjetCible
    ) 

    $pattern = "$nomProjetSourceComplet"

    Get-ChildItem -Path "$cheminCompletSolutionCible\*" -Recurse -Include *.csproj `
    | Select-Object @{ label = 'Path'; expression = { ($_.FullName) } }, @{ label = 'Content'; expression = { (Get-Content $_.FullName) } } `
    | ForEach-Object {            
            
        if ($_.Content -match ($pattern)) { 
            Set-Content -Path ($_.Path) -Value ($_.Content -replace $nomProjetSourceComplet, $nomProjetCible )
        }
    } 
}

function Rename-NamespaceAndUsingFromClass {
    [OutputType([System.Void])]
    param (
        [string]$cheminCompletSolutionCible,
        [string]$nomProjetSourceComplet, 
        [string]$nomProjetCible
    )

    $pattern = "(?:namespace|using)\W(\b$nomProjetSourceComplet)"

    Get-ChildItem -Path "$cheminCompletSolutionCible\*" -Recurse -Include *.cs `
    | Select-Object @{ label = 'Path'; expression = { ($_.FullName) } }, @{ label = 'Content'; expression = { (Get-Content $_.FullName) } } `
    | ForEach-Object {                        
        if ($_.Content -match ($pattern)) { 
            Set-Content -Path ($_.Path) -Value ($_.Content -replace $nomProjetSourceComplet, $nomProjetCible )
        }
    } 
}

function Rename-AssemblyInfo {
    [OutputType([System.Void])]
    param (
        [string]$cheminCompletSolutionCible,
        [string]$nomProjetSourceComplet, 
        [string]$nomProjetCible
    )

    $CheminRelatifVersAssemblyInfo = $CheminRelatifVersAssemblyInfo.Trim().Trim('"')
    
    if ($CheminRelatifVersAssemblyInfo) {
        $CheminRelatifVersAssemblyInfo = "$cheminCompletSolutionCible$CheminRelatifVersAssemblyInfo".Trim().Trim('"')
        if (!(Test-Path $CheminRelatifVersAssemblyInfo)) { throw "Le chemin n'existe pas. $CheminRelatifVersAssemblyInfo." }
        else {
            (Get-Content $CheminRelatifVersAssemblyInfo) |
            ForEach-Object { if ($_ -match "\b$nomProjetSourceComplet") { $_ -replace $nomProjetSourceComplet, $nomProjetCible } else { $_ } } |
            Set-Content $CheminRelatifVersAssemblyInfo
        }
    }
}

Write-Host "=====---- Début de configuration de votre modèle."

$RepertoireCourant = Save-CurrentDirectory

Start-ProceedOrExit "---------.... Copie si la cible est différente de la destination.";
$NomProjetSourceComplet = Copy-BaseSolution $NomProjetSourceComplet $NomProjetCibleComplet;

Start-ProceedOrExit "---------.... Renommage des fichiers et répertoires de la nouvelle solution : $NomProjetSource vers $NomProjetCible."
Rename-Solution $NomProjetCibleComplet $NomProjetSource $NomProjetCible

Start-ProceedOrExit "---------.... Renommage des références dans le fichier de solution cible : $CheminCompletSolutionCible."
Rename-ReferencesInSolution $CheminCompletSolutionCible $NomProjetSource $NomProjetCible

Start-ProceedOrExit "---------.... Renommage des assemblies et namespace des projets dans la cible : $NomProjetCibleComplet."
Rename-AssemblyNameAndRootNamespace $NomProjetCibleComplet $NomProjetSource $NomProjetCible

Start-ProceedOrExit "---------.... Renommage des namespaces et using des fichiers cs dans la cible : $NomProjetCibleComplet."
Rename-NamespaceAndUsingFromClass $NomProjetCibleComplet $NomProjetSource $NomProjetCible

Start-ProceedOrExit "---------.... Mise à jour de l'assemblie info si il existe dans la cible : $NomProjetCibleComplet."
Rename-AssemblyInfo $NomProjetCibleComplet $NomProjetSource $NomProjetCible

Restore-CurrentDirectory $RepertoireCourant

Write-Host "=====---- Fin de configuration de votre modèle."
