foreach ($dir in @($logsDir, $tempDir, $archiveDir)) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }
}

# Archive old daily log files except today's
$today = Get-Date -Format "yyyyMMdd"
Get-ChildItem $logsDir -Filter "log_*.txt" -File -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.BaseName -ne "log_$today") {
        Move-Item $_.FullName (Join-Path $archiveDir $_.Name) -Force
    }
}


# Delete old temp files
Get-ChildItem $tempDir -File -ErrorAction SilentlyContinue |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-3) } |
    Remove-Item -Force -ErrorAction SilentlyContinue

Write-Host "Cleanup completed"