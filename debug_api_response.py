#!/usr/bin/env python3
"""
Debug the API response structure to understand the field names.
"""

import requests
import json

def debug_api_response():
    """Debug the API response structure."""
    
    print("🔍 Debugging API Response Structure")
    print("=" * 40)
    
    # Login
    login_data = {
        "email": "test@example.com", 
        "password": "Test123456"
    }
    
    try:
        login_response = requests.post("http://localhost:5000/api/v1/auth/login", json=login_data)
        if login_response.status_code != 200:
            print(f"❌ Login failed: {login_response.status_code}")
            return
        
        session_cookies = login_response.cookies
        print("✅ Login successful")
        
        # Get entities and debug structure
        entities_response = requests.get(
            "http://localhost:5000/api/v1/entities/360-view",
            cookies=session_cookies
        )
        
        if entities_response.status_code == 200:
            data = entities_response.json()
            entities = data.get('entities', [])
            
            print(f"📊 Response structure:")
            print(f"   Status: {entities_response.status_code}")
            print(f"   Entities count: {len(entities)}")
            
            if entities:
                print(f"\n🔍 First entity structure:")
                first_entity = entities[0]
                for key, value in first_entity.items():
                    print(f"   {key}: {value}")
                
                # Check for patient ID field
                patient_id_fields = ['patientId', 'PatientId', 'patient_id', 'Patient_id']
                found_patient_id = None
                for field in patient_id_fields:
                    if field in first_entity:
                        found_patient_id = field
                        break
                
                if found_patient_id:
                    print(f"\n✅ Found patient ID field: {found_patient_id}")
                    print(f"   Value: {first_entity[found_patient_id]}")
                else:
                    print(f"\n❌ No patient ID field found in: {list(first_entity.keys())}")
            else:
                print("⚠️ No entities found")
        else:
            print(f"❌ Failed to get entities: {entities_response.status_code}")
            print(f"Response: {entities_response.text}")
            
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    debug_api_response()
