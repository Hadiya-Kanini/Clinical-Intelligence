import base64
import json
import hashlib
import hmac
import time

# Create a simple JWT token manually
def create_simple_jwt():
    header = {
        "alg": "HS256",
        "typ": "JWT"
    }
    
    payload = {
        "sub": "bc8dbc5f-2078-4c26-94d8-52d35f290691",
        "email": "test@example.com",
        "name": "Test User",
        "role": "Standard",
        "iss": "ClinicalIntelligence",
        "aud": "ClinicalIntelligence.Users",
        "iat": int(time.time()),
        "exp": int(time.time()) + 3600
    }
    
    # Encode header and payload
    header_b64 = base64.urlsafe_b64encode(json.dumps(header).encode()).decode().rstrip('=')
    payload_b64 = base64.urlsafe_b64encode(json.dumps(payload).encode()).decode().rstrip('=')
    
    # Create signature (without actual signing for testing)
    signature = "test_signature"
    
    # Combine parts
    jwt_token = f"{header_b64}.{payload_b64}.{signature}"
    
    return jwt_token, payload

# Test the manual JWT
jwt_token, payload = create_simple_jwt()
print(f"Manual JWT: {jwt_token}")
print(f"Payload: {json.dumps(payload, indent=2)}")

# Test this token against the backend
import requests

BASE_URL = "http://localhost:5000"
ME_URL = f"{BASE_URL}/api/v1/auth/me"

headers = {'Authorization': f'Bearer {jwt_token}'}
response = requests.get(ME_URL, headers=headers)

print(f"\nTest Result:")
print(f"Status: {response.status_code}")
print(f"Response: {response.text}")
