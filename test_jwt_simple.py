#!/usr/bin/env python3
import requests
import json
import base64

BASE_URL = "http://localhost:5002"
LOGIN_URL = f"{BASE_URL}/api/v1/auth/login"

def decode_jwt_payload(token):
    """Decode JWT payload without verification"""
    try:
        # Split token into parts
        parts = token.split('.')
        if len(parts) != 3:
            print("❌ Invalid JWT format")
            return None
        
        # Decode payload (second part)
        payload = parts[1]
        # Add padding if needed
        payload += '=' * (4 - len(payload) % 4)
        decoded = base64.urlsafe_b64decode(payload)
        return json.loads(decoded)
    except Exception as e:
        print(f"❌ JWT decode error: {e}")
        return None

def test_jwt():
    """Test JWT token structure"""
    print("Testing JWT token structure...")
    
    # Login
    login_data = {"email": "test@example.com", "password": "Test123456"}
    
    try:
        response = requests.post(LOGIN_URL, json=login_data)
        if response.status_code != 200:
            print(f"❌ Login failed: {response.status_code}")
            return
        
        data = response.json()
        token = data.get('access_token')
        if not token:
            print("❌ No access_token in response")
            return
        
        print(f"✅ Token received (length: {len(token)})")
        
        # Decode payload
        payload = decode_jwt_payload(token)
        if not payload:
            return
        
        print("\n📋 JWT Payload:")
        print(f"sub (User ID): {payload.get('sub', 'MISSING')}")
        print(f"email: {payload.get('email', 'MISSING')}")
        print(f"iss (Issuer): {payload.get('iss', 'MISSING')}")
        print(f"aud (Audience): {payload.get('aud', 'MISSING')}")
        print(f"exp (Expiration): {payload.get('exp', 'MISSING')}")
        
        # Check if sub claim exists
        if 'sub' not in payload:
            print("\n❌ PROBLEM: 'sub' claim is missing from JWT!")
        elif not payload['sub']:
            print("\n❌ PROBLEM: 'sub' claim is empty!")
        else:
            print(f"\n✅ 'sub' claim found: {payload['sub']}")
            
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    test_jwt()