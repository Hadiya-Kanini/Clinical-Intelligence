#!/usr/bin/env python3
"""
Create a test patient and test entity storage
"""

import requests
import json
import psycopg2
from datetime import datetime

BASE_URL = "http://localhost:5000"

def create_test_patient():
    """Create a test patient via direct database insert"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Create test patient
        patient_id = '00000000-0000-0000-0000-000000999999'
        
        cursor.execute("""
            INSERT INTO patients (
                "Id", "Mrn", "GivenName", "FamilyName", "DateOfBirth", 
                "Gender", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt"
            ) VALUES (
                %s, 'TEST999', 'Test', 'Patient', '1990-01-01', 
                'Unknown', true, false, NOW(), NOW()
            )
            ON CONFLICT ("Id") DO NOTHING
        """, (patient_id,))
        
        conn.commit()
        print(f"✅ Created test patient: {patient_id}")
        
        # Get a document
        cursor.execute("SELECT \"Id\" FROM documents LIMIT 1")
        document = cursor.fetchone()
        
        if document:
            doc_id = document[0]
            print(f"📄 Using document: {doc_id}")
            
            # Link document to patient
            cursor.execute("""
                UPDATE documents 
                SET "PatientId" = %s 
                WHERE "Id" = %s
            """, (patient_id, doc_id))
            
            conn.commit()
            print(f"🔗 Linked document to patient")
            
            conn.close()
            return patient_id, doc_id
        
        conn.close()
        return None, None
        
    except Exception as e:
        print(f"❌ Error: {e}")
        return None, None

def test_entity_storage():
    """Test entity storage after creating patient"""
    print("🧪 Testing Entity Storage After Creating Patient")
    print("=" * 50)
    
    # Create patient
    patient_id, document_id = create_test_patient()
    if not patient_id or not document_id:
        print("❌ Could not create patient or get document")
        return
    
    # Test entity storage
    test_payload = {
        "patientId": str(patient_id),
        "documentId": str(document_id),
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
                "entityName": "test_name",
                "entityValue": "Test Patient",
                "rationale": "Test entity",
                "sourceText": "Test source text",
                "confidence": 0.90,
                "documentLocation": {"page": 1, "section": "test"},
                "mappedCategory": "Patient Demographics"
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
            else:
                print(f"❌ 360 view failed: {view_response.status_code}")
    else:
        print(f"❌ Entity storage failed: {response.status_code}")

if __name__ == "__main__":
    test_entity_storage()
