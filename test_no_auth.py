import requests

# Test an endpoint that doesn't require authorization
BASE_URL = "http://localhost:5000"

def test_no_auth():
    print("🧪 Testing no-auth endpoint...")
    
    # Test health endpoint
    health_response = requests.get(f"{BASE_URL}/health")
    print(f"Health Status: {health_response.status_code}")
    print(f"Health Response: {health_response.text}")
    
    # Test ping endpoint (requires auth)
    ping_response = requests.get(f"{BASE_URL}/api/v1/ping")
    print(f"Ping Status: {ping_response.status_code}")
    print(f"Ping Response: {ping_response.text}")

if __name__ == "__main__":
    test_no_auth()
