import base64
import json
import sys

# JWT token from the login response (first part)
jwt_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJiYzhkYmM1Zi0yMDc4LTRjMjYtOTRkOC01MmQzNWYyOTA2OTEiLCJqdGkiOiIyYTU2YzQ0YS0wZmJlLTRiMmUtYmJlZi1kNzk1ZjYwM2YyN2UiLCJpYXQiOjE3Njg2Mjk2NTEsImVtYWlsIjoidGVzdEBleGFtcGxlLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0YW5kYXJkIiwicm9sZSI6IlN0YW5kYXJkIiwibmFtZSI6IlRlc3QgVXNlciIsInNpZCI6IjljYjdiNzYzLTJmYmItNGI4OC1hMGFkLWFiNzZmYWEwYTFhYSIsImV4cCI6MTc2ODYzMzI1MSwiaXNzIjoiQ2xpbmljYWxJbnRlbGxpZ2VuY2UiLCJhdWQiOiJDbGluaWNhbEludGVsbGlnZW5jZS5Vc2VycyJ9.s2bDHI9aogWiQzkxkId-2J7GoBNSKJDhuJuTzIT7aoA"

# Split the token
parts = jwt_token.split('.')
if len(parts) != 3:
    print("Invalid JWT token structure")
    sys.exit(1)

# Decode the payload (middle part)
payload = parts[1]
# Add padding if needed
padding = '=' * (4 - len(payload) % 4)
payload += padding

try:
    decoded_payload = base64.b64decode(payload)
    payload_json = json.loads(decoded_payload)
    
    print("JWT Payload:")
    print(json.dumps(payload_json, indent=2))
    
    # Check for specific claims
    print("\nKey Claims:")
    print(f"sub (User ID): {payload_json.get('sub', 'MISSING')}")
    print(f"email: {payload_json.get('email', 'MISSING')}")
    print(f"name: {payload_json.get('name', 'MISSING')}")
    print(f"role: {payload_json.get('role', 'MISSING')}")
    
except Exception as e:
    print(f"Error decoding payload: {e}")
