#!/usr/bin/env python3
"""
Test entity storage with real IDs
"""

import requests
import json
import psycopg2

BASE_URL = "http://localhost:5000"

def get_real_ids():
    """Get real patient and document IDs from database"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Get real IDs
        cursor.execute("SELECT \"Id\" FROM patients LIMIT 1")
        patient = cursor.fetchone()
        
        cursor.execute("SELECT \"Id\" FROM documents LIMIT 1")
        document = cursor.fetchone()
        
        conn.close()
        
        return patient[0] if patient else None, document[0] if document else None
        
    except Exception as e:
        print(f"❌ Database error: {e}")
        return None, None

def test_entity_storage_real():
    """Test entity storage with real IDs"""
    print("🔍 Testing Entity Storage with Real IDs")
    print("=" * 40)
    
    # Get real IDs
    patient_id, document_id = get_real_ids()
    if not patient_id or not document_id:
        print("❌ Could not get real IDs")
        return
    
    print(f"👤 Patient ID: {patient_id}")
    print(f"📄 Document ID: {document_id}")
    
    # Test data
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
            }
        ]
    }
    
    # Test with API key authentication
    headers = {
        'Content-Type': 'application/json',
        'X-API-Key': 'worker-secret-key-2024'
    }
    
    print("🔑 Testing with worker API key...")
    response = requests.post(f"{BASE_URL}/api/v1/documents/{document_id}/entities", 
                             json=test_payload, headers=headers)
    
    print(f"Status Code: {response.status_code}")
    print(f"Response: {response.text}")
    
    if response.status_code == 200:
        result = response.json()
        print(f"✅ Success: {result.get('entitiesStored')} entities stored")
    else:
        print(f"❌ Failed: {response.status_code}")

if __name__ == "__main__":
    test_entity_storage_real()
