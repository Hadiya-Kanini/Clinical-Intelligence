#!/usr/bin/env python3
"""
Test entity storage API directly
"""

import requests
import json

BASE_URL = "http://localhost:5000"

def test_entity_storage_api():
    """Test the entity storage API endpoint directly"""
    print("🔍 Testing Entity Storage API")
    print("=" * 40)
    
    # Test data
    test_payload = {
        "patientId": "00000000-0000-0000-0000-000000012345",
        "documentId": "fefbeb04-9d0a-40e0-bab6-2fce59a2330e",
        "entities": [
            {
                "entityGroupName": "document_metadata",
                "entityName": "document_type",
                "entityValue": "medical_report",
                "rationale": "Test entity",
                "sourceText": "Test source text",
                "confidence": 0.95,
                "documentLocation": {"page": 1, "section": "test"},
                "mappedCategory": "Document Metadata"
            },
            {
                "entityGroupName": "patient_demographics",
                "entityName": "patient_identified",
                "entityValue": "patient_information_present",
                "rationale": "Test entity",
                "sourceText": "Test source text",
                "confidence": 0.90,
                "documentLocation": {"page": 1, "section": "test"},
                "mappedCategory": "Patient Demographics"
            }
        ]
    }
    
    # Try with API key authentication (worker auth)
    headers = {
        'Content-Type': 'application/json',
        'X-API-Key': 'worker-secret-key-2024'
    }
    
    print("🔑 Testing with worker API key...")
    response = requests.post(f"{BASE_URL}/api/v1/documents/fefbeb04-9d0a-40e0-bab6-2fce59a2330e/entities", 
                             json=test_payload, headers=headers)
    
    print(f"Status Code: {response.status_code}")
    print(f"Response: {response.text}")
    
    if response.status_code == 200:
        result = response.json()
        print(f"✅ Success: {result.get('entitiesStored')} entities stored")
    else:
        print(f"❌ Failed: {response.status_code}")
        
        # Try with JWT auth as fallback
        print("\n🔐 Testing with JWT authentication...")
        login_data = {
            "email": "test@example.com",
            "password": "Test123456"
        }
        
        login_response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
        if login_response.status_code == 200:
            token = login_response.json().get("access_token")
            jwt_headers = {
                'Content-Type': 'application/json',
                'Authorization': f'Bearer {token}'
            }
            
            response = requests.post(f"{BASE_URL}/api/v1/documents/fefbeb04-9d0a-40e0-bab6-2fce59a2330e/entities", 
                                 json=test_payload, headers=jwt_headers)
            
            print(f"JWT Status Code: {response.status_code}")
            print(f"JWT Response: {response.text}")

if __name__ == "__main__":
    test_entity_storage_api()
