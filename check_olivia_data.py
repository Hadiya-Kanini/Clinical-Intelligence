#!/usr/bin/env python3
"""
Check if Olivia's data is available in the 360° view API.
"""

import requests
import json

def check_olivia_data():
    """Check if Olivia's extracted entities are available."""
    
    print("🔍 Checking Olivia's Data in 360° View")
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
        
        # Check all entities
        print("\n📊 Checking all entities...")
        entities_response = requests.get(
            "http://localhost:5000/api/v1/entities/360-view",
            cookies=session_cookies
        )
        
        if entities_response.status_code == 200:
            data = entities_response.json()
            entities = data.get('entities', [])
            print(f"✅ Found {len(entities)} total entities")
            
            # Group by patient
            patients = {}
            for entity in entities:
                patient_id = entity.get('patientId')
                patient_name = entity.get('patientName', 'Unknown')
                
                if patient_id not in patients:
                    patients[patient_id] = {
                        'name': patient_name,
                        'entities': [],
                        'categories': set()
                    }
                
                patients[patient_id]['entities'].append(entity)
                patients[patient_id]['categories'].add(entity.get('category', 'Unknown'))
            
            print(f"\n👥 Found {len(patients)} patients:")
            for patient_id, patient_data in patients.items():
                print(f"  - {patient_data['name']} ({patient_id}): {len(patient_data['entities'])} entities")
                print(f"    Categories: {', '.join(patient_data['categories'])}")
            
            # Check specifically for Olivia
            olivia_found = False
            for patient_id, patient_data in patients.items():
                if 'Olivia' in patient_data['name']:
                    olivia_found = True
                    print(f"\n🎯 Found Olivia's data:")
                    print(f"  Patient ID: {patient_id}")
                    print(f"  Name: {patient_data['name']}")
                    print(f"  Entities: {len(patient_data['entities'])}")
                    print(f"  Categories: {', '.join(patient_data['categories'])}")
                    
                    # Show sample entities
                    print(f"\n📋 Sample entities:")
                    for i, entity in enumerate(patient_data['entities'][:5]):
                        print(f"  {i+1}. {entity.get('category', 'Unknown')}: {entity.get('name', 'Unknown')} = {entity.get('value', 'Unknown')}")
                    
                    # Test frontend URL
                    frontend_url = f"http://localhost:5173/patients/{patient_id}"
                    print(f"\n🌐 Frontend URL: {frontend_url}")
                    print("✅ Try this URL to see Olivia's 360° view!")
                    break
            
            if not olivia_found:
                print("\n❌ Olivia's data not found in the system")
                print("🔍 Checking recent documents...")
                
                # Check recent documents
                docs_response = requests.get(
                    "http://localhost:5000/api/v1/documents",
                    cookies=session_cookies
                )
                
                if docs_response.status_code == 200:
                    docs = docs_response.json()
                    print(f"📄 Found {len(docs)} recent documents:")
                    for doc in docs[:5]:
                        print(f"  - {doc.get('originalName', 'Unknown')} ({doc.get('id', 'Unknown')}) - {doc.get('status', 'Unknown')}")
                
                return False
            
            return True
            
        else:
            print(f"❌ Failed to get entities: {entities_response.status_code}")
            print(f"Response: {entities_response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

if __name__ == "__main__":
    success = check_olivia_data()
    if success:
        print("\n🎉 OLIVIA'S DATA IS AVAILABLE!")
    else:
        print("\n💥 OLIVIA'S DATA NOT FOUND!")
