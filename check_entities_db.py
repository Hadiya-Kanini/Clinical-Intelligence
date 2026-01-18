#!/usr/bin/env python3
"""
Check entities in database
"""

import requests
import json

BASE_URL = "http://localhost:5000"

def check_entities():
    """Check what entities exist in the database"""
    
    # Login
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    if response.status_code != 200:
        print("❌ Login failed")
        return
    
    token = response.json().get("access_token")
    headers = {"Authorization": f"Bearer {token}"}
    
    # Try to get all entities (no filters)
    print("🔍 Checking for any entities in database...")
    
    # Try different endpoints
    endpoints_to_try = [
        "/api/v1/entities",
        "/api/v1/entities/all",
        "/api/v1/entities/list",
        "/api/v1/patients/00000000-0000-0000-0000-000000012345/entities"
    ]
    
    for endpoint in endpoints_to_try:
        try:
            response = requests.get(f"{BASE_URL}{endpoint}", headers=headers)
            print(f"  {endpoint}: {response.status_code}")
            if response.status_code == 200:
                data = response.json()
                print(f"    Response: {json.dumps(data, indent=2)[:200]}...")
        except Exception as e:
            print(f"  {endpoint}: Error - {e}")

if __name__ == "__main__":
    check_entities()
