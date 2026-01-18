#!/usr/bin/env python3
"""
Test the specific patient endpoint that Patient360Page is using.
"""

import requests
import json

def test_patient_endpoint():
    """Test the /api/v1/patients/{patientId} endpoint."""
    
    print("🔍 Testing Patient Endpoint (Patient360Page API)")
    print("=" * 55)
    
    # Login
    login_data = {
        "email": "test@example.com", 
        "password": "Test123456"
    }
    
    try:
        login_response = requests.post("http://localhost:5000/api/v1/auth/login", json=login_data)
        if login_response.status_code != 200:
            print(f"❌ Login failed: {login_response.status_code}")
            return False
        
        session_cookies = login_response.cookies
        print("✅ Login successful")
        
        # Test Olivia's patient endpoint
        olivia_patient_id = "ef4324bc-6dec-4ade-8243-bd8d8c428ea1"
        
        print(f"\n👤 Testing patient endpoint for Olivia: {olivia_patient_id}")
        
        patient_response = requests.get(
            f"http://localhost:5000/api/v1/patients/{olivia_patient_id}",
            cookies=session_cookies
        )
        
        print(f"📊 Status Code: {patient_response.status_code}")
        
        if patient_response.status_code == 200:
            patient_data = patient_response.json()
            print("✅ Patient endpoint successful!")
            
            print(f"\n📋 Patient Data Structure:")
            print(f"  Keys: {list(patient_data.keys())}")
            
            if 'patient' in patient_data:
                patient_info = patient_data['patient']
                print(f"  Patient Name: {patient_info.get('name', 'N/A')}")
                print(f"  Patient MRN: {patient_info.get('mrn', 'N/A')}")
                print(f"  Patient DOB: {patient_info.get('dateOfBirth', 'N/A')}")
            
            if 'entities' in patient_data:
                entities = patient_data['entities']
                print(f"  Entities Count: {len(entities)}")
                
                # Show entity categories
                categories = {}
                for entity in entities:
                    category = entity.get('category', 'Unknown')
                    categories[category] = categories.get(category, 0) + 1
                
                print(f"  Entity Categories: {dict(categories)}")
                
                # Show sample entities
                print(f"\n📄 Sample Entities:")
                for i, entity in enumerate(entities[:5]):
                    print(f"  {i+1}. {entity.get('category', 'Unknown')}: {entity.get('name', 'Unknown')} = {entity.get('value', 'Unknown')}")
            
            if 'documents' in patient_data:
                documents = patient_data['documents']
                print(f"  Documents Count: {len(documents)}")
                
                print(f"\n📁 Recent Documents:")
                for i, doc in enumerate(documents[:3]):
                    print(f"  {i+1}. {doc.get('fileName', 'Unknown')} - {doc.get('status', 'Unknown')}")
            
            print(f"\n🌐 Frontend URL: http://localhost:5173/patients/{olivia_patient_id}")
            print("✅ This should work in the frontend now!")
            
            return True
            
        else:
            print(f"❌ Patient endpoint failed: {patient_response.status_code}")
            print(f"Response: {patient_response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

if __name__ == "__main__":
    success = test_patient_endpoint()
    if success:
        print("\n🎉 PATIENT ENDPOINT TEST PASSED!")
    else:
        print("\n💥 PATIENT ENDPOINT TEST FAILED!")
