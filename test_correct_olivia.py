#!/usr/bin/env python3
"""
Test the correct Olivia patient with the recent upload.
"""

import requests
import json

def test_correct_olivia():
    """Test the correct Olivia patient (Olivia Phone)."""
    
    print("🔍 Testing Correct Olivia Patient (Olivia Phone)")
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
        
        # Test the correct Olivia patient
        olivia_phone_id = "ef4324bc-6dec-4ade-8243-bd8d8c428ea1"
        
        print(f"\n👤 Testing Olivia Phone: {olivia_phone_id}")
        
        patient_response = requests.get(
            f"http://localhost:5000/api/v1/patients/{olivia_phone_id}",
            cookies=session_cookies
        )
        
        print(f"📊 Status Code: {patient_response.status_code}")
        
        if patient_response.status_code == 200:
            patient_data = patient_response.json()
            print("✅ Patient endpoint successful!")
            
            # Patient info
            patient_info = patient_data.get('patient', {})
            print(f"\n📋 Patient Info:")
            print(f"  Name: {patient_info.get('name', 'N/A')}")
            print(f"  MRN: {patient_info.get('mrn', 'N/A')}")
            print(f"  DOB: {patient_info.get('dateOfBirth', 'N/A')}")
            
            # Entities
            entities = patient_data.get('entities', [])
            print(f"  Entities: {len(entities)}")
            
            # Documents
            documents = patient_data.get('documents', [])
            print(f"  Documents: {len(documents)}")
            
            if len(entities) > 0:
                print(f"\n🎯 SUCCESS! Olivia Phone has {len(entities)} entities!")
                
                # Show entity categories
                categories = {}
                for entity in entities:
                    category = entity.get('category', 'Unknown')
                    categories[category] = categories.get(category, 0) + 1
                
                print(f"📊 Entity Categories: {dict(categories)}")
                
                # Show sample entities
                print(f"\n📄 Sample Entities:")
                for i, entity in enumerate(entities[:5]):
                    print(f"  {i+1}. {entity.get('category', 'Unknown')}: {entity.get('name', 'Unknown')} = {entity.get('value', 'Unknown')}")
                
                print(f"\n🌐 Frontend URL: http://localhost:5173/patients/{olivia_phone_id}")
                print("✅ This URL should show Olivia's 69 entities!")
                
                return True
            else:
                print(f"\n❌ Olivia Phone has no entities!")
                print("🔍 Checking if entities exist in the 360-view endpoint...")
                
                # Check the 360-view endpoint
                entities_response = requests.get(
                    f"http://localhost:5000/api/v1/entities/360-view?patientId={olivia_phone_id}",
                    cookies=session_cookies
                )
                
                if entities_response.status_code == 200:
                    entities_data = entities_response.json()
                    entities_360 = entities_data.get('entities', [])
                    print(f"📊 360-view endpoint shows: {len(entities_360)} entities")
                    
                    if len(entities_360) > 0:
                        print(f"✅ Entities exist in 360-view but not in patient endpoint!")
                        print("🔍 There might be a data consistency issue between endpoints")
                    else:
                        print(f"❌ No entities found in 360-view endpoint either")
                else:
                    print(f"❌ 360-view endpoint failed: {entities_response.status_code}")
                
                return False
            
        else:
            print(f"❌ Patient endpoint failed: {patient_response.status_code}")
            print(f"Response: {patient_response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

if __name__ == "__main__":
    success = test_correct_olivia()
    if success:
        print("\n🎉 CORRECT OLIVIA TEST PASSED!")
    else:
        print("\n💥 CORRECT OLIVIA TEST FAILED!")
