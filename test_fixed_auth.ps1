# Test the fixed authentication flow
Write-Host "=== Testing Fixed Authentication Flow ==="

# Step 1: Login and get the session with cookies
Write-Host "`n1. Attempting login..."
$loginBody = @{
    email = "admin@clinicalintelligence.com"
    password = "Admin@123456"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/v1/auth/login" -Method POST -Body $loginBody -Headers @{"Content-Type"="application/json"} -SessionVariable session
    
    Write-Host "Login successful! Status: $($loginResponse.StatusCode)"
    Write-Host "Response: $($loginResponse.Content)"
    
    # Check if Set-Cookie header was present
    $setCookieHeader = $loginResponse.Headers["Set-Cookie"]
    if ($setCookieHeader) {
        Write-Host "`nSet-Cookie header found:"
        Write-Host "  $setCookieHeader"
    } else {
        Write-Host "`nNo Set-Cookie header found in response"
    }
    
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
    Write-Host "Dashboard access successful! Status: $($dashboardResponse.StatusCode)"
    Write-Host "Response: $($dashboardResponse.Content)"
} catch {
    Write-Host "Dashboard access failed: $($_.Exception.Message)"
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
    
    # Show what cookies were sent
    if ($_.Exception.Response) {
        $request = $_.Exception.Response.RequestMessage
        $cookieHeader = $request.Headers.GetValues("Cookie") -join "; "
        Write-Host "Cookies sent: $cookieHeader"
    }
}

Write-Host "`n=== Test Complete ==="
