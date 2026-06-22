# ================================
# JPG to JPEG Smart Renamer
# Works in the current folder
# ================================

# Use current working directory
$TargetFolder = Get-Location

# Log file in the same folder
$LogFile = "$TargetFolder\rename_log.txt"

# Clear / create the log file
"" | Out-File -FilePath $LogFile

# Get all .jpg files recursively
$jpgFiles = Get-ChildItem -Path $TargetFolder -Recurse -Filter *.jpg

if ($jpgFiles.Count -eq 0) {
    Write-Host "❌ No .jpg files found in: $TargetFolder"
    exit
}

Write-Host "Found $($jpgFiles.Count) .jpg files to process..."
Write-Host "-------------------------------------------"

foreach ($file in $jpgFiles) {

    # Get directory and base name
    $dir = $file.DirectoryName
    $base = $file.BaseName
    $newName = "$dir\$base.jpeg"

    # If a .jpeg already exists, add a numbered suffix
    $counter = 1
    while (Test-Path $newName) {
        $newName = "$dir\$base($counter).jpeg"
        $counter++
    }

    # Rename the file
    try {
        Rename-Item -Path $file.FullName -NewName (Split-Path $newName -Leaf)
        Write-Host "Renamed: $($file.FullName) → $newName" -ForegroundColor Green
        "$($file.FullName) → $newName" | Out-File -FilePath $LogFile -Append
    }
    catch {
        Write-Host "⚠️ Failed to rename: $($file.FullName)" -ForegroundColor Red
        "FAILED: $($file.FullName)" | Out-File -FilePath $LogFile -Append
    }
}

Write-Host "-------------------------------------------"
Write-Host "✅ All possible files have been renamed."
Write-Host "📄 Log file saved at: $LogFile"