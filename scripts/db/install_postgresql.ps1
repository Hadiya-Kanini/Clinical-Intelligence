<#
.SYNOPSIS
    PostgreSQL database and user bootstrap script for Clinical Intelligence.

.DESCRIPTION
    Creates the application database and a least-privilege application user.
    This script assumes PostgreSQL is already installed and running.
    Run validate_postgresql_prereqs.ps1 first to verify prerequisites.

.PARAMETER Host
    PostgreSQL host. Defaults to localhost.

.PARAMETER Port
    PostgreSQL port. Defaults to 5432.

.PARAMETER AdminUsername
    PostgreSQL admin username. Defaults to postgres.

.PARAMETER AdminPassword
    PostgreSQL admin password. Required.

.PARAMETER AppDatabase
    Application database name. Defaults to ClinicalIntelligence.

.PARAMETER AppUsername
    Application user name. Defaults to clinicalintelligence_app.

.PARAMETER AppPassword
    Application user password. Required.

.EXAMPLE
    .\install_postgresql.ps1 -AdminPassword "admin_pass" -AppPassword "app_pass"
#>

[CmdletBinding()]
param(
    [string]$Host = "localhost",
    [int]$Port = 5432,
    [string]$AdminUsername = "postgres",
    [Parameter(Mandatory = $true)]
    [string]$AdminPassword,
    [string]$AppDatabase = "ClinicalIntelligence",
    [string]$AppUsername = "clinicalintelligence_app",
    [Parameter(Mandatory = $true)]
    [string]$AppPassword
)

$ErrorActionPreference = "Stop"

function Get-PsqlPath {
    $psqlPath = Get-Command "psql" -ErrorAction SilentlyContinue
    if ($psqlPath) {
        return $psqlPath.Source
    }
    
    $commonPaths = @(
        "C:\Program Files\PostgreSQL\17\bin\psql.exe",
        "C:\Program Files\PostgreSQL\16\bin\psql.exe",
        "C:\Program Files\PostgreSQL\15\bin\psql.exe"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    throw "psql not found. Ensure PostgreSQL is installed and psql is in PATH."
}

function Invoke-PostgreSQLCommand {
    param(
        [string]$Command,
        [string]$Database = "postgres",
        [string]$Description
    )
    
    Write-Host "  $Description..." -ForegroundColor Gray
    
    $psqlPath = Get-PsqlPath
    $env:PGPASSWORD = $AdminPassword
    
    try {
        $result = & $psqlPath -h $Host -p $Port -U $AdminUsername -d $Database -c $Command 2>&1
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -ne 0) {
            $errorOutput = ($result | Out-String).Trim()
            if ($errorOutput -notmatch "already exists") {
                throw "Command failed: $errorOutput"
            }
            Write-Host "    (Already exists - skipping)" -ForegroundColor Yellow
        } else {
            Write-Host "    Done" -ForegroundColor Green
        }
        
        return $result
    }
    finally {
        $env:PGPASSWORD = $null
    }
}

function Test-DatabaseExists {
    param([string]$DatabaseName)
    
    $psqlPath = Get-PsqlPath
    $env:PGPASSWORD = $AdminPassword
    
    try {
        $result = & $psqlPath -h $Host -p $Port -U $AdminUsername -d "postgres" -t -c "SELECT 1 FROM pg_database WHERE datname = '$DatabaseName';" 2>&1
        return ($result -match "1")
    }
    finally {
        $env:PGPASSWORD = $null
    }
}

function Test-UserExists {
    param([string]$Username)
    
    $psqlPath = Get-PsqlPath
    $env:PGPASSWORD = $AdminPassword
    
    try {
        $result = & $psqlPath -h $Host -p $Port -U $AdminUsername -d "postgres" -t -c "SELECT 1 FROM pg_roles WHERE rolname = '$Username';" 2>&1
        return ($result -match "1")
    }
    finally {
        $env:PGPASSWORD = $null
    }
}

# Main execution
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Database Bootstrap" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Target: ${Host}:${Port}" -ForegroundColor Gray
Write-Host "Database: $AppDatabase" -ForegroundColor Gray
Write-Host "App User: $AppUsername" -ForegroundColor Gray
Write-Host ""

try {
    # Step 1: Create application user
    Write-Host "[1/4] Creating application user..." -ForegroundColor Cyan
    
    if (Test-UserExists -Username $AppUsername) {
        Write-Host "  User '$AppUsername' already exists" -ForegroundColor Yellow
        Invoke-PostgreSQLCommand -Command "ALTER USER $AppUsername WITH PASSWORD '$AppPassword';" -Description "Updating password"
    } else {
        Invoke-PostgreSQLCommand -Command "CREATE USER $AppUsername WITH PASSWORD '$AppPassword';" -Description "Creating user"
    }
    
    # Step 2: Create application database
    Write-Host "`n[2/4] Creating application database..." -ForegroundColor Cyan
    
    if (Test-DatabaseExists -DatabaseName $AppDatabase) {
        Write-Host "  Database '$AppDatabase' already exists" -ForegroundColor Yellow
    } else {
        Invoke-PostgreSQLCommand -Command "CREATE DATABASE `"$AppDatabase`" OWNER $AppUsername;" -Description "Creating database"
    }
    
    # Step 3: Grant privileges
    Write-Host "`n[3/4] Configuring privileges..." -ForegroundColor Cyan
    
    Invoke-PostgreSQLCommand -Database $AppDatabase -Command "GRANT CONNECT ON DATABASE `"$AppDatabase`" TO $AppUsername;" -Description "Granting connect privilege"
    Invoke-PostgreSQLCommand -Database $AppDatabase -Command "GRANT USAGE ON SCHEMA public TO $AppUsername;" -Description "Granting schema usage"
    Invoke-PostgreSQLCommand -Database $AppDatabase -Command "GRANT CREATE ON SCHEMA public TO $AppUsername;" -Description "Granting schema create"
    Invoke-PostgreSQLCommand -Database $AppDatabase -Command "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO $AppUsername;" -Description "Setting default table privileges"
    Invoke-PostgreSQLCommand -Database $AppDatabase -Command "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO $AppUsername;" -Description "Setting default sequence privileges"
    
    # Step 4: Validate connection with app user
    Write-Host "`n[4/4] Validating application user connection..." -ForegroundColor Cyan
    
    $psqlPath = Get-PsqlPath
    $env:PGPASSWORD = $AppPassword
    
    try {
        $result = & $psqlPath -h $Host -p $Port -U $AppUsername -d $AppDatabase -c "SELECT 1 AS connection_test;" -t 2>&1
        
        if ($LASTEXITCODE -eq 0 -and $result -match "1") {
            Write-Host "  Application user can connect successfully" -ForegroundColor Green
        } else {
            throw "Application user connection test failed: $result"
        }
    }
    finally {
        $env:PGPASSWORD = $null
    }
    
    # Success summary
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Bootstrap Complete" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Connection string for .env:" -ForegroundColor Cyan
    Write-Host "DATABASE_CONNECTION_STRING=`"Host=$Host;Port=$Port;Database=$AppDatabase;Username=$AppUsername;Password=<your_app_password>`"" -ForegroundColor White
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Update .env with the connection string above" -ForegroundColor Gray
    Write-Host "  2. Run enable_pgvector.sql to enable vector extension" -ForegroundColor Gray
    Write-Host "  3. Run EF Core migrations: dotnet ef database update" -ForegroundColor Gray
    
    exit 0
}
catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "Bootstrap Failed" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Verify PostgreSQL is running: Get-Service postgresql*" -ForegroundColor Gray
    Write-Host "  2. Check admin credentials are correct" -ForegroundColor Gray
    Write-Host "  3. Ensure admin user has CREATE DATABASE privilege" -ForegroundColor Gray
    Write-Host "  4. Run validate_postgresql_prereqs.ps1 first" -ForegroundColor Gray
    
    exit 1
}
