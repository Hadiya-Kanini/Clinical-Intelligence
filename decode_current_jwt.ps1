# Decode the current JWT token from the test
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJiZjlkYWRiZC1hYTZhLTQ4MjYtOWZmYS1hNDBlMzY2ZDM0ZDQiLCJqdGkiOiJmMmE1ODFmYS02NzBjLTQ2ZDQtYTdiZi1mMWNlZDA5YjZhZWQiLCJpYXQiOjE3Njg1NzAyNzgsImVtYWlsIjoiYWRtaW5AY2xpbmljYWxpbnRlbGxpZ2VuY2UuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJyb2xlIjoiQWRtaW4iLCJuYW1lIjoiU3RhdGljIEFkbWluIiwic2lkIjoiMDY5Mjc1YTQtNTVlZi00ZWUwLWFhY2MtMTRjNjZmYjVlZWYxIiwiZXhwIjoxNzY4NTczODc4LCJpc3MiOiJDbGluaWNhbEludGVsbGlnZW5jZSIsImF1ZCI6IkNsaW5pY2FsSW50ZWxsaWdlbmNlLlVzZXJzIn0.q17USRfji011BwwkkN5LTrM_1FfnWQqKl5PdVirmM8w"

# Split token into parts
$parts = $token -split '\.'
$header = $parts[0]
$payload = $parts[1]

# Decode base64 (add padding if needed)
function Base64Decode($base64) {
    $base64 = $base64.Replace('-', '+').Replace('_', '/')
    while ($base64.Length % 4) {
        $base64 += '='
    }
    return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($base64))
}

try {
    $decodedPayload = Base64Decode $payload
    $payloadObj = $decodedPayload | ConvertFrom-Json
    
    Write-Host "Current JWT Token Analysis:"
    Write-Host "========================"
    
    # Check expiration
    $exp = [DateTimeOffset]::FromUnixTimeSeconds([int]$payloadObj.exp)
    $now = [DateTimeOffset]::UtcNow
    $isExpired = $exp -lt $now
    
    Write-Host "Expiration:"
    Write-Host "  Token expires at: $exp"
    Write-Host "  Current time: $now"
    Write-Host "  Is expired: $isExpired"
    
    Write-Host "`nClaims:"
    Write-Host "  Issuer (iss): $($payloadObj.iss)"
    Write-Host "  Audience (aud): $($payloadObj.aud)"
    Write-Host "  User ID (sub): $($payloadObj.sub)"
    Write-Host "  Session ID (sid): $($payloadObj.sid)"
    Write-Host "  Email: $($payloadObj.email)"
    Write-Host "  Role: $($payloadObj.role)"
    
    # Check if values match expected configuration
    Write-Host "`nValidation Check:"
    Write-Host "  Expected Issuer: ClinicalIntelligence"
    Write-Host "  Expected Audience: ClinicalIntelligence.Users"
    Write-Host "  Issuer matches: $($payloadObj.iss -eq 'ClinicalIntelligence')"
    Write-Host "  Audience matches: $($payloadObj.aud -eq 'ClinicalIntelligence.Users')"
    
} catch {
    Write-Host "Error decoding token: $($_.Exception.Message)"
}
