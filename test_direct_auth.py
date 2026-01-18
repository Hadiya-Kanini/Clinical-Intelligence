import requests
import json

# Test direct authentication against backend (no frontend proxy)
BASE_URL = "http://localhost:5000"
LOGIN_URL = f"{BASE_URL}/api/v1/auth/login"
ME_URL = f"{BASE_URL}/api/v1/auth/me"

def test_direct_auth():
    """Test direct authentication"""
    print("🧪 Testing direct authentication...")
    
    # Step 1: Login
    login_data = {
        "email": "test@example.com", 
        "password": "Test123456"
    }
    
    login_response = requests.post(LOGIN_URL, json=login_data)
    print(f"Login Status: {login_response.status_code}")
    
    if login_response.status_code != 200:
        print(f"Login failed: {login_response.text}")
        return
    
    login_result = login_response.json()
    token = login_result.get('access_token')
    print(f"✅ Got token: {len(token)} chars")
    
    # Step 2: Use token immediately
    headers = {'Authorization': f'Bearer {token}'}
    print(f"🔍 Testing with token: {token[:50]}...")
    
    me_response = requests.get(ME_URL, headers=headers)
    print(f"Auth/me Status: {me_response.status_code}")
    print(f"Auth/me Response: {me_response.text}")
    
    # Step 3: Test dashboard endpoint
    dashboard_response = requests.get(f"{BASE_URL}/api/v1/dashboard/stats", headers=headers)
    print(f"Dashboard Status: {dashboard_response.status_code}")
    print(f"Dashboard Response: {dashboard_response.text[:200]}...")

if __name__ == "__main__":
    test_direct_auth()
