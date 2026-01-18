#!/usr/bin/env python3
"""
Test the patients API to see if the new document appears
"""

import requests

def test_patients_api():
    """Test the patients API endpoint"""
    BASE_URL = "http://localhost:5000"
    
    # Login
    login_data = {"email": "test@example.com", "password": "Test123456"}
    response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    
    if response.status_code != 200:
        print("❌ Login failed")
        return
    
    token = response.json().get("access_token")
    headers = {"Authorization": f"Bearer {token}"}
    
    # Get patients
    response = requests.get(f"{BASE_URL}/api/v1/patients/dashboard", headers=headers)
    
    if response.status_code == 200:
        data = response.json()
        patients = data.get("patients", [])
        total_count = data.get("totalCount", 0)
        
        print(f"📊 Total Patients: {total_count}")
        print(f"📋 Patients with documents:")
        
        for patient in patients[:5]:  # Show first 5
            name = patient.get("name", "Unknown")
            mrn = patient.get("mrn", "N/A")
            doc_count = patient.get("documentCount", 0)
            last_upload = patient.get("lastDocumentUploadedAt", "Never")
            
            print(f"  • {name} ({mrn}) - {doc_count} docs, last: {last_upload}")
            
            # Check if this patient has the 360 button enabled
            if doc_count > 0:
                print(f"    ✅ 360 View button should be enabled")
            else:
                print(f"    ❌ 360 View button disabled (no documents)")
        
        # Look for Olivia Phone specifically
        olivia = next((p for p in patients if "Olivia" in p.get("name", "")), None)
        if olivia:
            print(f"\n🎯 Found Olivia Phone:")
            print(f"   • Name: {olivia.get('name')}")
            print(f"   • MRN: {olivia.get('mrn')}")
            print(f"   • Document Count: {olivia.get('documentCount')}")
            print(f"   • Last Upload: {olivia.get('lastDocumentUploadedAt')}")
            print(f"   ✅ 360 View should be accessible")
            
            # Test 360 view
            patient_id = olivia.get("id")
            response = requests.get(f"{BASE_URL}/api/v1/patients/{patient_id}/360", headers=headers)
            
            if response.status_code == 200:
                entities = response.json().get("entities", [])
                print(f"   ✅ 360 View API: {len(entities)} entities returned")
            else:
                print(f"   ❌ 360 View API failed: {response.status_code}")
        
        return True
    else:
        print(f"❌ API failed: {response.status_code}")
        return False

if __name__ == "__main__":
    test_patients_api()
