param(
    [string]$ProjectRoot = ".."
)

$ErrorActionPreference = "Continue"

$projectRootFull = Resolve-Path $ProjectRoot
$logsDir = Join-Path $projectRootFull "logs"
$diagDir = Join-Path $logsDir "diagnostics"

if (-not (Test-Path $diagDir)) {
    New-Item -ItemType Directory -Path $diagDir | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outFile = Join-Path $diagDir "diagnostics_$timestamp.txt"

"=== Windows Game Launcher Diagnostics ===" | Out-File $outFile
"Timestamp: $(Get-Date)" | Out-File $outFile -Append
"Machine: $env:COMPUTERNAME" | Out-File $outFile -Append
"User: $env:USERNAME" | Out-File $outFile -Append
"" | Out-File $outFile -Append

"=== Running Processes ===" | Out-File $outFile -Append
Get-Process | Sort-Object ProcessName | Format-Table ProcessName, Id, CPU, WorkingSet -AutoSize | Out-String |
    Out-File $outFile -Append

"" | Out-File $outFile -Append
"=== Recent Application Event Log Entries ===" | Out-File $outFile -Append

try {
    Get-WinEvent -LogName Application -MaxEvents 20 |
        Select-Object TimeCreated, Id, LevelDisplayName, ProviderName, Message |
        Format-List | Out-String | Out-File $outFile -Append
}
catch {
    "Failed to query Application event log: $_" | Out-File $outFile -Append
}

"" | Out-File $outFile -Append
"=== Environment Variables ===" | Out-File $outFile -Append
Get-ChildItem Env: | Sort-Object Name | Format-Table Name, Value -AutoSize | Out-String |
    Out-File $outFile -Append

Write-Host "Diagnostics written to $outFile"