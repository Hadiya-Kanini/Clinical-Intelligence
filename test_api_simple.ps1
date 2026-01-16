# API Endpoint Testing Script
$baseUrl = "http://localhost:5000"
$apiUrl = "$baseUrl/api/v1"

Write-Host "Testing Clinical Intelligence API Endpoints" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Test 1: Health Check
Write-Host "`n1. Testing Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method GET -TimeoutSec 10
    Write-Host "✓ Health Check: PASSED" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Health Check: FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Swagger/OpenAPI
Write-Host "`n2. Testing Swagger Documentation..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/swagger/v1/swagger.json" -Method GET -TimeoutSec 10
    Write-Host "✓ Swagger: PASSED" -ForegroundColor Green
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Cyan
} catch {
    Write-Host "✗ Swagger: FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Login Endpoint (without credentials - should fail)
Write-Host "`n3. Testing Login Endpoint (validation)..." -ForegroundColor Yellow
try {
    $loginData = @{
        email = ""
        password = ""
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$apiUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json" -TimeoutSec 10
    Write-Host "✗ Login Validation: UNEXPECTED SUCCESS" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✓ Login Validation: PASSED (correctly rejected empty credentials)" -ForegroundColor Green
    } else {
        Write-Host "✗ Login Validation: FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 4: Protected Endpoint (should require auth)
Write-Host "`n4. Testing Protected Endpoint (auth required)..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$apiUrl/ping" -Method GET -TimeoutSec 10
    Write-Host "✗ Auth Protection: FAILED (should require authentication)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ Auth Protection: PASSED (correctly requires authentication)" -ForegroundColor Green
    } else {
        Write-Host "✗ Auth Protection: FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=============================================" -ForegroundColor Green
Write-Host "API Endpoint Testing Complete" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green