import requests
import json

# Test authentication on port 5000
BASE_URL = "http://localhost:5000"
LOGIN_URL = f"{BASE_URL}/api/v1/auth/login"

def test_login():
    """Test login on port 5000"""
    print("🧪 Testing login on port 5000...")
    
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    try:
        response = requests.post(LOGIN_URL, json=login_data)
        print(f"Status: {response.status_code}")
        print(f"Response: {response.text[:300]}...")
        
        if response.status_code == 200:
            login_result = response.json()
            token = login_result.get('access_token')
            print(f"✅ Login successful! Token length: {len(token) if token else 0}")
            
            # Test the token with auth/me endpoint
            headers = {'Authorization': f'Bearer {token}'}
            me_response = requests.get(f"{BASE_URL}/api/v1/auth/me", headers=headers)
            print(f"Auth/me Status: {me_response.status_code}")
            print(f"Auth/me Response: {me_response.text[:200]}...")
            
        else:
            print("❌ Login failed")
            
    except Exception as e:
        print(f"❌ Request failed: {e}")

if __name__ == "__main__":
    test_login()
