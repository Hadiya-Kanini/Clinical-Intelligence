#!/usr/bin/env python3
"""
Check document status
"""

import requests
import json

BASE_URL = "http://localhost:5000"

def check_document_status():
    """Check status of uploaded document"""
    document_id = "5cf84765-f8bf-41b9-8a95-cc8b790fa495"
    
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
    
    # Check document status
    response = requests.get(f"{BASE_URL}/api/v1/documents/{document_id}/status", headers=headers)
    print(f"Status Code: {response.status_code}")
    print(f"Response: {response.text}")
    
    if response.status_code == 200:
        data = response.json()
        print(f"Document Status: {data.get('status')}")
        print(f"Progress: {data.get('progress', {})}")

if __name__ == "__main__":
    check_document_status()
