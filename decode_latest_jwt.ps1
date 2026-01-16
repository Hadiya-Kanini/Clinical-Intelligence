# Decode the latest JWT token from the test
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJiZjlkYWRiZC1hYTZhLTQ4MjYtOWZmYS1hNDBlMzY2ZDM0ZDQiLCJqdGkiOiJmYjUyYWZlZi02ZTI5LTRlZmMtODZmZC03YjVkMTI5MjU1NzgiLCJpYXQiOjE3Njg1Njk4ODksImVtYWlsIjoiYWRtaW5AY2xpbmljYWxpbnRlbGxpZ2VuY2UuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJyb2xlIjoiQWRtaW4iLCJuYW1lIjoiU3RhdGljIEFkbWluIiwic2lkIjoiMWJmMDdkNjQtZmM1OC00MTk3LTg0NWYtNWQwNmQxNDU5MjA3IiwiZXhwIjoxNzY4NTczNDg5LCJpc3MiOiJDbGluaWNhbEludGVsbGlnZW5jZSIsImF1ZCI6IkNsaW5pY2FsSW50ZWxsaWdlbmNlLlVzZXJzIn0.KuGMIzVy0BO5ylA7E-hDnRg4O2gB-nMXejGDlUzmAQc"

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
    
    Write-Host "JWT Payload Analysis:"
    $payloadObj | ConvertTo-Json -Depth 3
    
    Write-Host "`nKey Claims:"
    Write-Host "  User ID (sub): $($payloadObj.sub)"
    Write-Host "  Session ID (sid): $($payloadObj.sid)"
    Write-Host "  Email: $($payloadObj.email)"
    Write-Host "  Role: $($payloadObj.role)"
    
    # Check if sub claim is a valid GUID
    try {
        $guid = [Guid]::Parse($payloadObj.sub)
        Write-Host "  User ID is valid GUID: $guid"
    } catch {
        Write-Host "  User ID is NOT a valid GUID: $($payloadObj.sub)"
    }
    
} catch {
    Write-Host "Error decoding token: $($_.Exception.Message)"
}
