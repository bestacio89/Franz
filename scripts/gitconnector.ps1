param(
    [Parameter(Mandatory = $true)]
    [string]$RemoteUrl,

    [string]$Branch = "main",

    [switch]$Initialize,

    [switch]$Force
)

# =========================================================
# VALIDATION
# =========================================================

function Write-Step($message) {
    Write-Host "---- $message"
}

function Invoke-Git($arguments) {

    $result = git $arguments 2>&1

    if ($LASTEXITCODE -ne 0) {
        throw "Git command failed:`n$result"
    }

    return $result
}


if (!(Get-Command git -ErrorAction SilentlyContinue)) {
    throw "Git is not installed or not available in PATH."
}


# =========================================================
# INITIALIZE REPOSITORY
# =========================================================

if ($Initialize) {

    if (!(Test-Path ".git")) {

        Write-Step "Initializing git repository..."

        Invoke-Git "init"

    }
    else {

        Write-Step "Git repository already initialized."

    }
}


if (!(Test-Path ".git")) {

    throw @"
Current directory is not a git repository.

Run with:

./Connect-GitRemote.ps1 `
    -RemoteUrl <url> `
    -Initialize
"@

}


# =========================================================
# REMOTE MANAGEMENT
# =========================================================

Write-Step "Checking existing remote..."

$existingRemote = git remote get-url origin 2>$null


if ($existingRemote) {

    if (!$Force) {

        throw @"
Remote origin already exists:

$existingRemote

Use -Force to replace it.
"@

    }


    Write-Step "Replacing existing origin..."

    Invoke-Git "remote remove origin"

}


Write-Step "Adding remote origin..."

Invoke-Git @(
    "remote",
    "add",
    "origin",
    $RemoteUrl
)


# =========================================================
# BRANCH CONFIGURATION
# =========================================================

Write-Step "Configuring default branch..."

Invoke-Git @(
    "branch",
    "-M",
    $Branch
)


# =========================================================
# VERIFICATION
# =========================================================

Write-Step "Verifying configuration..."

Invoke-Git "remote -v"


Write-Host ""
Write-Host "================================="
Write-Host " Git remote configured successfully"
Write-Host "================================="
Write-Host ""

Write-Host "Next steps:"
Write-Host ""
Write-Host "git add ."
Write-Host "git commit -m `"Initial commit`""
Write-Host "git push -u origin $Branch"