#!/usr/bin/env python3
"""
Test entity storage with ERD patient
"""

import requests
import json

BASE_URL = "http://localhost:5000"

def test_entity_storage_erd():
    """Test entity storage with ERD patient"""
    print("🧪 Testing Entity Storage with ERD Patient")
    print("=" * 40)
    
    # Use the ERD patient and document we just linked
    patient_id = "dca8e532-3276-419c-8be0-025e6c4dd105"
    document_id = "db56280e-7f83-4159-a90e-347ea290e2f3"
    
    print(f"👤 ERD Patient ID: {patient_id}")
    print(f"📄 Document ID: {document_id}")
    
    # Test entity storage
    test_payload = {
        "patientId": str(patient_id),
        "documentId": str(document_id),
        "entities": [
            {
                "entityGroupName": "document_metadata",
                "entityName": "document_type",
                "entityValue": "medical_report",
                "rationale": "Test entity from worker",
                "sourceText": "Test source text",
                "confidence": 0.95,
                "documentLocation": {"page": 1, "section": "test"},
                "mappedCategory": "Document Metadata"
            },
            {
                "entityGroupName": "patient_demographics",
                "entityName": "patient_name",
                "entityValue": "Test Patient",
                "rationale": "Extracted from document header",
                "sourceText": "Patient: Test Patient",
                "confidence": 0.90,
                "documentLocation": {"page": 1, "section": "header"},
                "mappedCategory": "Patient Demographics"
            },
            {
                "entityGroupName": "diagnoses",
                "entityName": "hypertension",
                "entityValue": "Essential hypertension",
                "rationale": "Found in diagnosis section",
                "sourceText": "Diagnosis: Essential hypertension",
                "confidence": 0.85,
                "documentLocation": {"page": 2, "section": "diagnosis"},
                "mappedCategory": "Diagnoses"
            }
        ]
    }
    
    headers = {
        'Content-Type': 'application/json',
        'X-API-Key': 'worker-secret-key-2024'
    }
    
    print("🔑 Testing entity storage...")
    response = requests.post(f"{BASE_URL}/api/v1/documents/{document_id}/entities", 
                             json=test_payload, headers=headers)
    
    print(f"Status Code: {response.status_code}")
    print(f"Response: {response.text}")
    
    if response.status_code == 200:
        result = response.json()
        print(f"✅ Success: {result.get('entitiesStored')} entities stored")
        
        # Test 360 view
        print("\n🔍 Testing 360 view...")
        login_data = {"email": "test@example.com", "password": "Test123456"}
        login_response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
        
        if login_response.status_code == 200:
            token = login_response.json().get("access_token")
            jwt_headers = {'Authorization': f'Bearer {token}'}
            
            view_response = requests.get(f"{BASE_URL}/api/v1/entities/360-view?documentId={document_id}", 
                                       headers=jwt_headers)
            
            if view_response.status_code == 200:
                entities = view_response.json().get("entities", [])
                print(f"✅ 360 view found {len(entities)} entities")
                
                # Show entity categories
                categories = {}
                for entity in entities:
                    cat = entity.get("entity_group_name", "unknown")
                    if cat not in categories:
                        categories[cat] = 0
                    categories[cat] += 1
                
                print("📋 Entity Categories:")
                for cat, count in categories.items():
                    print(f"  • {cat}: {count} entities")
                    
                print("\n🎉 ENTITY STORAGE FIX SUCCESSFUL!")
            else:
                print(f"❌ 360 view failed: {view_response.status_code}")
        else:
            print("❌ Login failed")
    else:
        print(f"❌ Entity storage failed: {response.status_code}")

if __name__ == "__main__":
    test_entity_storage_erd()
