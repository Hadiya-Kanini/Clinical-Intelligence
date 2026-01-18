#!/usr/bin/env python3
"""
Debug the patients page to see why it's not showing updated data.
"""

import requests
import json

def debug_patients_page():
    """Debug the patients page data loading."""
    
    print("🔍 Debugging Patients Page Data Loading")
    print("=" * 50)
    
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
        
        # Check patients list endpoint
        print("\n📋 Checking patients dashboard endpoint...")
        patients_response = requests.get(
            "http://localhost:5000/api/v1/patients/dashboard",
            cookies=session_cookies
        )
        
        print(f"📊 Patients List Status: {patients_response.status_code}")
        
        if patients_response.status_code == 200:
            patients_data = patients_response.json()
            patients_list = patients_data.get('patients', [])
            total_count = patients_data.get('totalCount', 0)
            
            print(f"✅ Found {len(patients_list)} patients in the list (Total: {total_count})")
            
            print(f"\n👥 Patients in List:")
            for i, patient in enumerate(patients_list):
                print(f"  {i+1}. {patient.get('name', 'Unknown')} (ID: {patient.get('id', 'Unknown')})")
                print(f"     MRN: {patient.get('mrn', 'N/A')}")
                print(f"     Documents: {patient.get('documentCount', 0)}")
                print(f"     Last Upload: {patient.get('lastDocumentUploadedAt', 'N/A')}")
            
            # Check if Olivia is in the list
            olivia_found = False
            olivia_data = None
            for patient in patients_list:
                if 'Olivia' in patient.get('name', ''):
                    olivia_found = True
                    olivia_data = patient
                    break
            
            if olivia_found:
                print(f"\n🎯 Found Olivia in patients list!")
                print(f"  Name: {olivia_data.get('name')}")
                print(f"  ID: {olivia_data.get('id')}")
                print(f"  MRN: {olivia_data.get('mrn')}")
                print(f"  Documents: {olivia_data.get('documentCount')}")
                print(f"  Last Upload: {olivia_data.get('lastDocumentUploadedAt')}")
                
                # Test the specific patient endpoint
                olivia_id = olivia_data.get('id')
                if olivia_id:
                    print(f"\n🔍 Testing Olivia's detailed endpoint...")
                    patient_detail_response = requests.get(
                        f"http://localhost:5000/api/v1/patients/{olivia_id}",
                        cookies=session_cookies
                    )
                    
                    if patient_detail_response.status_code == 200:
                        detail_data = patient_detail_response.json()
                        entities_count = len(detail_data.get('entities', []))
                        print(f"✅ Olivia's detailed endpoint works: {entities_count} entities")
                        
                        if entities_count > 0:
                            print(f"🎯 Frontend URL should work: http://localhost:5173/patients/{olivia_id}")
                            print("✅ Try this URL - it should show Olivia's 69 entities!")
                        else:
                            print(f"⚠️ Olivia's detailed endpoint shows no entities")
                    else:
                        print(f"❌ Olivia's detailed endpoint failed: {patient_detail_response.status_code}")
                
                return True
            else:
                print(f"\n❌ Olivia not found in patients list!")
                print("🔍 This is the issue - the patients dashboard isn't returning updated data")
                
                # Check what the patients endpoint is actually returning
                print(f"\n🔍 Raw patients data structure:")
                print(f"  Type: {type(patients_data)}")
                print(f"  Keys: {list(patients_data.keys()) if isinstance(patients_data, dict) else 'Not a dict'}")
                
                return False
            
        else:
            print(f"❌ Patients list failed: {patients_response.status_code}")
            print(f"Response: {patients_response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

if __name__ == "__main__":
    success = debug_patients_page()
    if success:
        print("\n🎉 PATIENTS PAGE DEBUG COMPLETED!")
    else:
        print("\n💥 PATIENTS PAGE DEBUG FAILED!")
