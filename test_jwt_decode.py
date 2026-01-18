import base64
import json

# Test JWT token from the login response
jwt_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJiYzhkYmM1Zi0yMDc4LTRjMjYtOTRkOC01MmQzNWYyOTA2OTEiLCJqdGkiOiIyNWFiYmUzMi0yODNhLTQwN2ItOTk5Mi1kYzJhMzBkMTM1Y2UiLCJpYXQiOjE3Njg2MzA3MjksImVtYWlsIjoidGVzdEBleGFtcGxlLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0YW5kYXJkIiwicm9sZSI6IlN0YW5kYXJkIiwibmFtZSI6IlRlc3QgVXNlciIsInNpZCI6ImU4ODJmZjQ0LWYzZTktNGMxOS05MjYxLWJmNzY4YzY4OGM5YyIsImV4cCI6MTc2ODYzNDMyOSwiaXNzIjoiQ2xpbmljYWxJbnRlbGxpZ2VuY2UiLCJhdWQiOiJDbGluaWNhbEludGVsbGlnZW5jZS5Vc2VycyJ9.s2bDHI9aogWiQzkxkId-2J7GoBNSKJDhuJuTzIT7aoA"

# Split the token
parts = jwt_token.split('.')
if len(parts) != 3:
    print("Invalid JWT token structure")
    exit(1)

# Decode the payload
payload = parts[1]
# Add padding if needed
padding = '=' * (4 - len(payload) % 4)
payload += padding

try:
    decoded_payload = base64.b64decode(payload)
    payload_json = json.loads(decoded_payload)
    
    print("JWT Payload:")
    print(json.dumps(payload_json, indent=2))
    
    # Check key claims
    print(f"\nKey Claims:")
    print(f"sub (User ID): {payload_json.get('sub', 'MISSING')}")
    print(f"email: {payload_json.get('email', 'MISSING')}")
    print(f"iss (Issuer): {payload_json.get('iss', 'MISSING')}")
    print(f"aud (Audience): {payload_json.get('aud', 'MISSING')}")
    print(f"exp (Expiration): {payload_json.get('exp', 'MISSING')}")
    
except Exception as e:
    print(f"Error decoding payload: {e}")
