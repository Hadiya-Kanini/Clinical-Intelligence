import requests
import json

# Configuration
BASE_URL = "http://localhost:5002"
LOGIN_URL = f"{BASE_URL}/api/v1/auth/login"
ME_URL = f"{BASE_URL}/api/v1/auth/me"
DASHBOARD_URL = f"{BASE_URL}/api/v1/dashboard/stats"

def test_endpoints():
    """Test different authenticated endpoints"""
    print("🧪 Testing Authenticated Endpoints")
    print("=" * 50)
    
    # Login
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    session = requests.Session()
    login_response = session.post(LOGIN_URL, json=login_data)
    
    if login_response.status_code != 200:
        print(f"❌ Login failed: {login_response.status_code}")
        return
    
    # Extract JWT token
    login_result = login_response.json()
    token = login_result.get('access_token')
    
    if not token:
        print("❌ No token in login response")
        return
    
    print("✅ Login successful")
    
    # Set up headers with JWT token
    headers = {
        'Authorization': f'Bearer {token}',
        'Content-Type': 'application/json'
    }
    
    # Test /auth/me endpoint
    print("\nTesting /auth/me endpoint...")
    me_response = requests.get(ME_URL, headers=headers)
    print(f"Status: {me_response.status_code}")
    print(f"Response: {me_response.text[:200]}...")
    
    # Test dashboard endpoint
    print("\nTesting /dashboard/stats endpoint...")
    dashboard_response = requests.get(DASHBOARD_URL, headers=headers)
    print(f"Status: {dashboard_response.status_code}")
    print(f"Response: {dashboard_response.text[:200]}...")
    
    # Test upload endpoint with JWT in header (should fail with 401)
    print("\nTesting upload endpoint with JWT...")
    upload_url = f"{BASE_URL}/api/v1/documents/upload"
    
    # Create a simple test file
    test_content = "Test document content"
    files = {'file': ('test.txt', test_content, 'text/plain')}
    
    upload_response = requests.post(upload_url, files=files, headers=headers)
    print(f"Status: {upload_response.status_code}")
    print(f"Response: {upload_response.text[:300]}...")

if __name__ == "__main__":
    test_endpoints()
