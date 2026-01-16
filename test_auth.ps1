# Test authentication flow
Write-Host "=== Testing Authentication Flow ==="

# Step 1: Login and capture cookies
Write-Host "`n1. Attempting login..."
$loginBody = @{
    email = "admin@clinicalintelligence.com"
    password = "Admin@123456"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/v1/auth/login" -Method POST -Body $loginBody -Headers @{"Content-Type"="application/json"} -SessionVariable session
    
    Write-Host "Login successful!"
    Write-Host "Response: $($loginResponse.Content)"
    
    # Check cookies in session
    Write-Host "`nCookies in session:"
    $session.Cookies.GetCookies("http://localhost:5000") | ForEach-Object {
        Write-Host "  $($_.Name) = $($_.Value)"
    }
    
} catch {
    Write-Host "Login failed: $($_.Exception.Message)"
    exit
}

# Step 2: Try to access dashboard with session cookies
Write-Host "`n2. Attempting to access dashboard..."
try {
    $dashboardResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/v1/dashboard/stats" -Method GET -WebSession $session -Headers @{"Content-Type"="application/json"}
    Write-Host "Dashboard access successful!"
    Write-Host "Response: $($dashboardResponse.Content)"
} catch {
    Write-Host "Dashboard access failed: $($_.Exception.Message)"
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
}

Write-Host "`n=== Test Complete ==="
