#!/usr/bin/env python3
"""
Debug login to see what token format is returned
"""

import requests
import json

BASE_URL = "http://localhost:5000"

def test_login():
    """Test login and examine response"""
    print("🔍 Debugging login...")
    
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    try:
        response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
        print(f"Status Code: {response.status_code}")
        print(f"Headers: {dict(response.headers)}")
        print(f"Response Body: {response.text}")
        
        if response.status_code == 200:
            data = response.json()
            print(f"JSON Keys: {list(data.keys())}")
            for key, value in data.items():
                print(f"  {key}: {value}")
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    test_login()
