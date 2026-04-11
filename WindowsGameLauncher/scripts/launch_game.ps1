param(
    [string]$ProfilePath = "..\gameProfile.json",
    [string]$ProjectRoot = ".."
)

$ErrorActionPreference = "Stop"

$projectRootFull = Resolve-Path $ProjectRoot
$profileFull = Resolve-Path $ProfilePath

$logsDir = Join-Path $projectRootFull "logs"
if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir | Out-Null
}

Write-Host "Launching WindowsGameLauncher..."
Write-Host "Project Root: $projectRootFull"
Write-Host "Profile: $profileFull"

# Optional cleanup before run
$cleanupScript = Join-Path $PSScriptRoot "cleanup_run.ps1"
if (Test-Path $cleanupScript) {
    Write-Host "Running cleanup script..."
    & $cleanupScript -ProjectRoot $projectRootFull
}

$exePath = Join-Path $projectRootFull "bin\Debug\net10.0\WindowsGameLauncher.exe"

if (-not (Test-Path $exePath)) {
    Write-Error "Executable not found at $exePath. Please build the project first."
    exit 1
}

& $exePath $profileFull
$exitCode = $LASTEXITCODE

Write-Host "WindowsGameLauncher exited with code $exitCode"

# Optional diagnostics after run
$diagnosticScript = Join-Path $PSScriptRoot "collect_diagnostics.ps1"
if (Test-Path $diagnosticScript) {
    Write-Host "Collecting diagnostics..."
    & $diagnosticScript -ProjectRoot $projectRootFull
}

exit $exitCode

