<#
.SYNOPSIS
    Pre-flight validation script for PostgreSQL 15+ installation.

.DESCRIPTION
    Validates PostgreSQL prerequisites:
    - PostgreSQL version >= 15
    - Service is running and listening on expected port
    - Credentials are valid (SELECT 1 test)
    - Disk space >= 10GB available

.PARAMETER Host
    PostgreSQL host. Defaults to localhost.

.PARAMETER Port
    PostgreSQL port. Defaults to 5432.

.PARAMETER Database
    Database name. Defaults to postgres.

.PARAMETER Username
    PostgreSQL username. Defaults to postgres.

.PARAMETER Password
    PostgreSQL password. Required.

.EXAMPLE
    .\validate_postgresql_prereqs.ps1 -Password "your_password"
#>

[CmdletBinding()]
param(
    [string]$Host = "localhost",
    [int]$Port = 5432,
    [string]$Database = "postgres",
    [string]$Username = "postgres",
    [Parameter(Mandatory = $false)]
    [string]$Password
)

$ErrorActionPreference = "Stop"
$script:ValidationErrors = @()

function Write-ValidationResult {
    param(
        [string]$Check,
        [bool]$Passed,
        [string]$Message
    )
    
    $status = if ($Passed) { "[PASS]" } else { "[FAIL]" }
    $color = if ($Passed) { "Green" } else { "Red" }
    
    Write-Host "$status $Check" -ForegroundColor $color
    if ($Message) {
        Write-Host "       $Message" -ForegroundColor $(if ($Passed) { "Gray" } else { "Yellow" })
    }
    
    if (-not $Passed) {
        $script:ValidationErrors += "$Check`: $Message"
    }
}

function Test-PostgreSQLVersion {
    Write-Host "`nChecking PostgreSQL version..." -ForegroundColor Cyan
    
    try {
        $psqlPath = Get-Command "psql" -ErrorAction SilentlyContinue
        if (-not $psqlPath) {
            $commonPaths = @(
                "C:\Program Files\PostgreSQL\17\bin\psql.exe",
                "C:\Program Files\PostgreSQL\16\bin\psql.exe",
                "C:\Program Files\PostgreSQL\15\bin\psql.exe",
                "C:\Program Files\PostgreSQL\14\bin\psql.exe"
            )
            foreach ($path in $commonPaths) {
                if (Test-Path $path) {
                    $psqlPath = $path
                    break
                }
            }
        }
        
        if (-not $psqlPath) {
            Write-ValidationResult -Check "PostgreSQL Installation" -Passed $false -Message "psql not found in PATH or common installation directories"
            return $false
        }
        
        $versionOutput = & $psqlPath --version 2>&1
        if ($versionOutput -match "psql \(PostgreSQL\) (\d+)\.") {
            $majorVersion = [int]$Matches[1]
            if ($majorVersion -ge 15) {
                Write-ValidationResult -Check "PostgreSQL Version" -Passed $true -Message "Version $majorVersion detected (>= 15 required)"
                return $true
            } else {
                Write-ValidationResult -Check "PostgreSQL Version" -Passed $false -Message "Version $majorVersion detected. Version 15 or higher is required."
                return $false
            }
        } else {
            Write-ValidationResult -Check "PostgreSQL Version" -Passed $false -Message "Could not parse version from: $versionOutput"
            return $false
        }
    }
    catch {
        Write-ValidationResult -Check "PostgreSQL Version" -Passed $false -Message $_.Exception.Message
        return $false
    }
}

function Test-PostgreSQLService {
    Write-Host "`nChecking PostgreSQL service status..." -ForegroundColor Cyan
    
    try {
        $service = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue | Where-Object { $_.Status -eq "Running" } | Select-Object -First 1
        
        if ($service) {
            Write-ValidationResult -Check "PostgreSQL Service" -Passed $true -Message "Service '$($service.Name)' is running"
            return $true
        } else {
            $allServices = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
            if ($allServices) {
                $serviceNames = ($allServices | ForEach-Object { "$($_.Name) ($($_.Status))" }) -join ", "
                Write-ValidationResult -Check "PostgreSQL Service" -Passed $false -Message "Found services but none running: $serviceNames"
            } else {
                Write-ValidationResult -Check "PostgreSQL Service" -Passed $false -Message "No PostgreSQL service found. Ensure PostgreSQL is installed as a Windows service."
            }
            return $false
        }
    }
    catch {
        Write-ValidationResult -Check "PostgreSQL Service" -Passed $false -Message $_.Exception.Message
        return $false
    }
}

function Test-PostgreSQLPort {
    param([string]$TargetHost, [int]$TargetPort)
    
    Write-Host "`nChecking PostgreSQL port connectivity..." -ForegroundColor Cyan
    
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $asyncResult = $tcpClient.BeginConnect($TargetHost, $TargetPort, $null, $null)
        $waitHandle = $asyncResult.AsyncWaitHandle
        
        if ($waitHandle.WaitOne(5000, $false)) {
            $tcpClient.EndConnect($asyncResult)
            $tcpClient.Close()
            Write-ValidationResult -Check "Port Connectivity" -Passed $true -Message "Port $TargetPort is reachable on $TargetHost"
            return $true
        } else {
            $tcpClient.Close()
            Write-ValidationResult -Check "Port Connectivity" -Passed $false -Message "Connection to ${TargetHost}:${TargetPort} timed out after 5 seconds"
            return $false
        }
    }
    catch {
        Write-ValidationResult -Check "Port Connectivity" -Passed $false -Message "Cannot connect to ${TargetHost}:${TargetPort} - $($_.Exception.Message)"
        return $false
    }
}

function Test-PostgreSQLConnection {
    param(
        [string]$TargetHost,
        [int]$TargetPort,
        [string]$TargetDatabase,
        [string]$TargetUsername,
        [string]$TargetPassword
    )
    
    Write-Host "`nValidating PostgreSQL credentials..." -ForegroundColor Cyan
    
    if (-not $TargetPassword) {
        Write-ValidationResult -Check "Database Connection" -Passed $false -Message "Password not provided. Use -Password parameter."
        return $false
    }
    
    try {
        $psqlPath = Get-Command "psql" -ErrorAction SilentlyContinue
        if (-not $psqlPath) {
            $commonPaths = @(
                "C:\Program Files\PostgreSQL\17\bin\psql.exe",
                "C:\Program Files\PostgreSQL\16\bin\psql.exe",
                "C:\Program Files\PostgreSQL\15\bin\psql.exe"
            )
            foreach ($path in $commonPaths) {
                if (Test-Path $path) {
                    $psqlPath = $path
                    break
                }
            }
        }
        
        if (-not $psqlPath) {
            Write-ValidationResult -Check "Database Connection" -Passed $false -Message "psql not found"
            return $false
        }
        
        $env:PGPASSWORD = $TargetPassword
        $result = & $psqlPath -h $TargetHost -p $TargetPort -U $TargetUsername -d $TargetDatabase -c "SELECT 1 AS connection_test;" -t 2>&1
        $env:PGPASSWORD = $null
        
        if ($LASTEXITCODE -eq 0 -and $result -match "1") {
            Write-ValidationResult -Check "Database Connection" -Passed $true -Message "Successfully connected and executed SELECT 1"
            return $true
        } else {
            $errorMsg = ($result | Out-String).Trim()
            if ($errorMsg -match "password|authentication|FATAL") {
                Write-ValidationResult -Check "Database Connection" -Passed $false -Message "Authentication failed. Check username and password."
            } else {
                Write-ValidationResult -Check "Database Connection" -Passed $false -Message "Connection test failed: $errorMsg"
            }
            return $false
        }
    }
    catch {
        Write-ValidationResult -Check "Database Connection" -Passed $false -Message $_.Exception.Message
        return $false
    }
}

function Test-DiskSpace {
    param([int]$MinimumGB = 10)
    
    Write-Host "`nChecking available disk space..." -ForegroundColor Cyan
    
    try {
        $drive = (Get-Location).Drive
        if (-not $drive) {
            $drive = Get-PSDrive -Name "C"
        }
        
        $freeSpaceGB = [math]::Round($drive.Free / 1GB, 2)
        
        if ($freeSpaceGB -ge $MinimumGB) {
            Write-ValidationResult -Check "Disk Space" -Passed $true -Message "${freeSpaceGB}GB available (>= ${MinimumGB}GB required)"
            return $true
        } else {
            Write-ValidationResult -Check "Disk Space" -Passed $false -Message "Only ${freeSpaceGB}GB available. Minimum ${MinimumGB}GB required."
            return $false
        }
    }
    catch {
        Write-ValidationResult -Check "Disk Space" -Passed $false -Message $_.Exception.Message
        return $false
    }
}

# Main execution
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Pre-flight Validation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Target: ${Host}:${Port}/${Database}" -ForegroundColor Gray
Write-Host "User: $Username" -ForegroundColor Gray

$versionOk = Test-PostgreSQLVersion
$serviceOk = Test-PostgreSQLService
$portOk = Test-PostgreSQLPort -TargetHost $Host -TargetPort $Port
$connectionOk = Test-PostgreSQLConnection -TargetHost $Host -TargetPort $Port -TargetDatabase $Database -TargetUsername $Username -TargetPassword $Password
$diskOk = Test-DiskSpace -MinimumGB 10

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Validation Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$allPassed = $versionOk -and $serviceOk -and $portOk -and $connectionOk -and $diskOk

if ($allPassed) {
    Write-Host "`nAll pre-flight checks PASSED" -ForegroundColor Green
    Write-Host "PostgreSQL is ready for use." -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nPre-flight validation FAILED" -ForegroundColor Red
    Write-Host "Please address the following issues:" -ForegroundColor Yellow
    foreach ($error in $script:ValidationErrors) {
        Write-Host "  - $error" -ForegroundColor Yellow
    }
    Write-Host "`nTroubleshooting tips:" -ForegroundColor Cyan
    Write-Host "  1. Ensure PostgreSQL 15+ is installed" -ForegroundColor Gray
    Write-Host "  2. Start the PostgreSQL service: Start-Service postgresql*" -ForegroundColor Gray
    Write-Host "  3. Verify firewall allows connections on port $Port" -ForegroundColor Gray
    Write-Host "  4. Check pg_hba.conf for authentication settings" -ForegroundColor Gray
    exit 1
}
