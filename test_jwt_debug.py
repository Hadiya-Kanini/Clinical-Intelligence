#!/usr/bin/env python3
"""
Debug script to test JWT token generation and validation
"""
import requests
import json
import jwt
import base64

# Configuration
BASE_URL = "http://localhost:5002"
LOGIN_URL = f"{BASE_URL}/api/v1/auth/login"
ME_URL = f"{BASE_URL}/api/v1/auth/me"

def decode_jwt_payload(token):
    """Decode JWT payload without verification for debugging"""
    try:
        # Split the token and decode the payload
        parts = token.split('.')
        if len(parts) != 3:
            print("Invalid JWT format")
            return None
        
        # Add padding if needed
        payload = parts[1]
        payload += '=' * (4 - len(payload) % 4)
        
        # Decode base64
        decoded = base64.urlsafe_b64decode(payload)
        return json.loads(decoded)
    except Exception as e:
        print(f"Error decoding JWT: {e}")
        return None

def test_login_and_jwt():
    """Test login and examine JWT token"""
    print("🔍 Testing JWT Token Generation and Validation")
    print("=" * 60)
    
    # Login credentials
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    try:
        session = requests.Session()
        login_response = session.post(LOGIN_URL, json=login_data, allow_redirects=True)
        print(f"Login Response Status: {login_response.status_code}")
        
        if login_response.status_code == 200:
            response_data = login_response.json()
            access_token = response_data.get('access_token')
            
            if access_token:
                print(f"✅ Login successful")
                print(f"Token length: {len(access_token)}")
                
                # Decode and examine JWT payload
                payload = decode_jwt_payload(access_token)
                if payload:
                    print("\n📋 JWT Payload:")
                    for key, value in payload.items():
                        print(f"  {key}: {value}")
                    
                    # Check for sub claim
                    if 'sub' in payload:
                        print(f"\n✅ 'sub' claim found: {payload['sub']}")
                    else:
                        print(f"\n❌ 'sub' claim missing!")
                
                # Test /auth/me endpoint
                print(f"\n🔍 Testing /auth/me endpoint...")
                me_response = session.get(ME_URL)
                print(f"Auth/me Response Status: {me_response.status_code}")
                print(f"Auth/me Response: {me_response.text[:300]}...")
                
                # Also test with Authorization header
                print(f"\n🔍 Testing with Authorization header...")
                headers = {'Authorization': f'Bearer {access_token}'}
                me_response_header = requests.get(ME_URL, headers=headers)
                print(f"Auth/me (header) Response Status: {me_response_header.status_code}")
                print(f"Auth/me (header) Response: {me_response_header.text[:300]}...")
                
            else:
                print("❌ No access token in response")
                print(f"Response: {login_response.text}")
        else:
            print("❌ Login failed")
            print(f"Response: {login_response.text}")
            
    except Exception as e:
        print(f"Request failed: {e}")

if __name__ == "__main__":
    test_login_and_jwt()