# Run this script as Administrator to install pgvector
# Right-click -> "Run as Administrator"

Write-Host "Installing pgvector for PostgreSQL 18..." -ForegroundColor Cyan

$pgPath = "C:\Program Files\PostgreSQL\18"
$sourcePath = "pgvector_extracted"

if (-not (Test-Path $pgPath)) {
    Write-Host "PostgreSQL 18 not found at $pgPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $sourcePath)) {
    Write-Host "pgvector files not found at $sourcePath" -ForegroundColor Red
    exit 1
}

Write-Host "Copying pgvector files to PostgreSQL directory..." -ForegroundColor Gray

try {
    # Copy all files from the extracted archive
    Copy-Item -Path "$sourcePath\*" -Destination $pgPath -Recurse -Force
    
    Write-Host "Files copied successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Restart PostgreSQL service: Get-Service postgresql* | Restart-Service" -ForegroundColor Gray
    Write-Host "2. Connect to your database and run: CREATE EXTENSION vector;" -ForegroundColor Gray
    Write-Host "3. Verify installation: SELECT extname, extversion FROM pg_extension WHERE extname='vector';" -ForegroundColor Gray
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
