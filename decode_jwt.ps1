# Decode the JWT token to see what's inside
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJiZjlkYWRiZC1hYTZhLTQ4MjYtOWZmYS1hNDBlMzY2ZDM0ZDQiLCJqdGkiOiJhYjJkMWRhZS0yOGNjLTQ5MDgtYjZhMi04NjE1MDVlM2NlZTMiLCJpYXQiOjE3Njg1Njk2NDEsImVtYWlsIjoiYWRtaW5AY2xpbmljYWxpbnRlbGxpZ2VuY2UuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJyb2xlIjoiQWRtaW4iLCJuYW1lIjoiU3RhdGljIEFkbWluIiwic2lkIjoiYjIxZGIwOGEtMzZiYS00OTNlLWI3N2UtNzFkZGUwY2Q0ODc0IiwiZXhwIjoxNzY4NTczMjQxLCJpc3MiOiJDbGluaWNhbEludGVsbGlnZW5jZSIsImF1ZCI6IkNsaW5pY2FsSW50ZWxsaWdlbmNlLlVzZXJzIn0.sW9VZH4_-EZHUHlOSEcKblA-vrb7Rnqu6wRPsXFr8P4"

# Split token into parts
$parts = $token -split '\.'
$header = $parts[0]
$payload = $parts[1]
$signature = $parts[2]

# Decode base64 (add padding if needed)
function Base64Decode($base64) {
    # Remove URL-safe characters and add padding
    $base64 = $base64.Replace('-', '+').Replace('_', '/')
    while ($base64.Length % 4) {
        $base64 += '='
    }
    return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($base64))
}

try {
    $decodedHeader = Base64Decode $header
    $decodedPayload = Base64Decode $payload
    
    Write-Host "JWT Header:"
    $decodedHeader | ConvertFrom-Json | ConvertTo-Json -Depth 3
    
    Write-Host "`nJWT Payload:"
    $payloadObj = $decodedPayload | ConvertFrom-Json
    $payloadObj | ConvertTo-Json -Depth 3
    
    Write-Host "`nSession ID (sid): $($payloadObj.sid)"
    Write-Host "User ID (sub): $($payloadObj.sub)"
    Write-Host "Email: $($payloadObj.email)"
    Write-Host "Role: $($payloadObj.role)"
    
} catch {
    Write-Host "Error decoding token: $($_.Exception.Message)"
}
