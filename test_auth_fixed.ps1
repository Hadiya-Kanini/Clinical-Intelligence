# Test authentication flow after fixes
Write-Host "=== Testing Fixed Authentication Flow ===" -ForegroundColor Green

# Configuration
$baseUrl = "http://localhost:5000"
$adminEmail = "admin@clinicalintelligence.com"
$adminPassword = "Admin@123456"

# Step 1: Login and capture cookies
Write-Host "`n1. Testing login..." -ForegroundColor Yellow
$loginBody = @{
    email = $adminEmail
    password = $adminPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "$baseUrl/api/v1/auth/login" -Method POST -Body $loginBody -Headers @{"Content-Type"="application/json"} -SessionVariable session
    
    Write-Host "✅ Login successful!" -ForegroundColor Green
    $loginData = $loginResponse.Content | ConvertFrom-Json
    Write-Host "User: $($loginData.user.email) ($($loginData.user.role))"
    
    # Check cookies
    $cookies = $session.Cookies.GetCookies("$baseUrl")
    Write-Host "Cookies received: $($cookies.Count)"
    foreach ($cookie in $cookies) {
        Write-Host "  - $($cookie.Name): $($cookie.Value.Substring(0, [Math]::Min(20, $cookie.Value.Length)))..."
    }
    
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test /auth/me endpoint
Write-Host "`n2. Testing /auth/me endpoint..." -ForegroundColor Yellow
try {
    $meResponse = Invoke-WebRequest -Uri "$baseUrl/api/v1/auth/me" -Method GET -WebSession $session
    Write-Host "✅ /auth/me successful!" -ForegroundColor Green
    $meData = $meResponse.Content | ConvertFrom-Json
    Write-Host "User data: $($meData.email) - $($meData.role)"
} catch {
    Write-Host "❌ /auth/me failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
}

# Step 3: Test dashboard endpoint
Write-Host "`n3. Testing dashboard endpoint..." -ForegroundColor Yellow
try {
    $dashboardResponse = Invoke-WebRequest -Uri "$baseUrl/api/v1/dashboard/stats" -Method GET -WebSession $session
    Write-Host "✅ Dashboard access successful!" -ForegroundColor Green
    $dashboardData = $dashboardResponse.Content | ConvertFrom-Json
    Write-Host "Stats: Uploads today: $($dashboardData.uploadsToday), Processing: $($dashboardData.processing)"
} catch {
    Write-Host "❌ Dashboard access failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
}

# Step 4: Test documents endpoint
Write-Host "`n4. Testing documents endpoint..." -ForegroundColor Yellow
try {
    $documentsResponse = Invoke-WebRequest -Uri "$baseUrl/api/v1/documents" -Method GET -WebSession $session
    Write-Host "✅ Documents access successful!" -ForegroundColor Green
    $documentsData = $documentsResponse.Content | ConvertFrom-Json
    Write-Host "Documents: Total: $($documentsData.total), Page: $($documentsData.page)"
} catch {
    Write-Host "❌ Documents access failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
}

# Step 5: Test ping endpoint
Write-Host "`n5. Testing ping endpoint..." -ForegroundColor Yellow
try {
    $pingResponse = Invoke-WebRequest -Uri "$baseUrl/api/v1/ping" -Method GET -WebSession $session
    Write-Host "✅ Ping successful!" -ForegroundColor Green
    $pingData = $pingResponse.Content | ConvertFrom-Json
    Write-Host "Ping response: $($pingData.status)"
} catch {
    Write-Host "❌ Ping failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
}

# Step 6: Test logout
Write-Host "`n6. Testing logout..." -ForegroundColor Yellow
try {
    $logoutResponse = Invoke-WebRequest -Uri "$baseUrl/api/v1/auth/logout" -Method POST -WebSession $session
    Write-Host "✅ Logout successful!" -ForegroundColor Green
    $logoutData = $logoutResponse.Content | ConvertFrom-Json
    Write-Host "Logout response: $($logoutData.status)"
} catch {
    Write-Host "❌ Logout failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
}

# Step 7: Verify logout worked by testing protected endpoint
Write-Host "`n7. Verifying logout (should fail)..." -ForegroundColor Yellow
try {
    $verifyResponse = Invoke-WebRequest -Uri "$baseUrl/api/v1/auth/me" -Method GET -WebSession $session
    Write-Host "❌ Logout verification failed - still authenticated!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✅ Logout verified - correctly returned 401!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Unexpected status: $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
    }
}

Write-Host "`n=== Authentication Test Complete ===" -ForegroundColor Green