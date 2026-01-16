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

# Test 5: CORS Headers
Write-Host "`n5. Testing CORS Configuration..." -ForegroundColor Yellow
try {
    $headers = @{
        'Origin' = 'http://localhost:5173'
        'Access-Control-Request-Method' = 'POST'
        'Access-Control-Request-Headers' = 'Content-Type'
    }
    $response = Invoke-WebRequest -Uri "$baseUrl/health" -Method OPTIONS -Headers $headers -TimeoutSec 10
    
    if ($response.Headers['Access-Control-Allow-Origin']) {
        Write-Host "✓ CORS: PASSED" -ForegroundColor Green
        Write-Host "CORS Origin: $($response.Headers['Access-Control-Allow-Origin'])" -ForegroundColor Cyan
    } else {
        Write-Host "✗ CORS: FAILED (no CORS headers)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ CORS: FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Database Health Check (requires admin auth - should fail)
Write-Host "`n6. Testing Database Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health/db" -Method GET -TimeoutSec 10
    Write-Host "✗ DB Health Auth: FAILED (should require admin auth)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ DB Health Auth: PASSED (correctly requires admin authentication)" -ForegroundColor Green
    } else {
        Write-Host "✗ DB Health Auth: FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 7: API Version Validation
Write-Host "`n7. Testing API Version Validation..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/v2/ping" -Method GET -TimeoutSec 10
    Write-Host "✗ Version Validation: FAILED (should reject unsupported version)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✓ Version Validation: PASSED (correctly rejects v2)" -ForegroundColor Green
    } else {
        Write-Host "✗ Version Validation: FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 8: Rate Limiting on Login
Write-Host "`n8. Testing Rate Limiting..." -ForegroundColor Yellow
try {
    $loginData = @{
        email = "test@example.com"
        password = "wrongpassword"
    } | ConvertTo-Json
    
    # Make multiple rapid requests
    $rateLimited = $false
    for ($i = 1; $i -le 6; $i++) {
        try {
            Invoke-RestMethod -Uri "$apiUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json" -TimeoutSec 5
        } catch {
            if ($_.Exception.Response.StatusCode -eq 429) {
                Write-Host "✓ Rate Limiting: PASSED (triggered after $i attempts)" -ForegroundColor Green
                $rateLimited = $true
                break
            }
        }
    }
    if (-not $rateLimited) {
        Write-Host "✗ Rate Limiting: FAILED (should have been rate limited)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Rate Limiting: ERROR" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=============================================" -ForegroundColor Green
Write-Host "API Endpoint Testing Complete" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green