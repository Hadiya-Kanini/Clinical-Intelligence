#!/usr/bin/env python3
"""
Debug 360 view response structure
"""

import requests
import json

BASE_URL = "http://localhost:5000"

def debug_360_response():
    """Debug the 360 view response structure"""
    document_id = "ca78ac9f-5e92-45eb-8066-ef0d8fd55b1a"
    
    print("🔍 Debugging 360 View Response Structure")
    print("=" * 50)
    
    # Login
    login_data = {"email": "test@example.com", "password": "Test123456"}
    response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    
    if response.status_code != 200:
        print("❌ Login failed")
        return
    
    token = response.json().get("access_token")
    headers = {"Authorization": f"Bearer {token}"}
    
    # Get 360 view
    response = requests.get(f"{BASE_URL}/api/v1/entities/360-view?documentId={document_id}", 
                          headers=headers)
    
    if response.status_code == 200:
        data = response.json()
        print("📊 Full Response Structure:")
        print(json.dumps(data, indent=2))
        
        entities = data.get("entities", [])
        if entities:
            print(f"\n📋 First Entity Structure:")
            print(json.dumps(entities[0], indent=2))
    else:
        print(f"❌ API failed: {response.status_code}")

if __name__ == "__main__":
    debug_360_response()
