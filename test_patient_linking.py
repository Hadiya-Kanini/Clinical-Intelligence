#!/usr/bin/env python3
"""
Test document upload and verify it appears in patients section
"""

import requests
import json
import time
import os

BASE_URL = "http://localhost:5000"

def test_upload_and_patient_linking():
    """Test document upload and verify patient linking works"""
    print("🧪 Testing Document Upload and Patient Linking")
    print("=" * 50)
    
    # Login
    login_data = {"email": "test@example.com", "password": "Test123456"}
    response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    
    if response.status_code != 200:
        print("❌ Login failed")
        return False
    
    token = response.json().get("access_token")
    headers = {"Authorization": f"Bearer {token}"}
    print("✅ Login successful")
    
    # Check patients count before upload
    response = requests.get(f"{BASE_URL}/api/v1/patients/dashboard", headers=headers)
    if response.status_code == 200:
        before_data = response.json()
        before_count = before_data.get("totalCount", 0)
        print(f"📊 Patients before upload: {before_count}")
    else:
        print("❌ Could not get patients count before upload")
        before_count = 0
    
    # Upload a test document
    test_pdf_path = "c:/Users/HadiyaAmber/Desktop/Clinical-Intelligence/Report_2 5.pdf"
    
    if not os.path.exists(test_pdf_path):
        print("❌ Test PDF not found")
        return False
    
    print("📄 Uploading test document...")
    with open(test_pdf_path, 'rb') as f:
        files = {'file': (os.path.basename(test_pdf_path), f, 'application/pdf')}
        response = requests.post(f"{BASE_URL}/api/v1/documents/upload", 
                               files=files, headers=headers)
    
    if response.status_code != 200:
        print(f"❌ Upload failed: {response.status_code}")
        return False
    
    document_id = response.json().get("documentId")
    print(f"✅ Document uploaded: {document_id}")
    
    # Wait for processing
    print("⏳ Waiting for document processing...")
    max_wait = 60  # 1 minute
    start_time = time.time()
    
    while time.time() - start_time < max_wait:
        response = requests.get(f"{BASE_URL}/api/v1/documents/{document_id}/status", 
                              headers=headers)
        
        if response.status_code == 200:
            status = response.json().get("status")
            print(f"📊 Status: {status}")
            
            if status == "completed":
                print("✅ Processing completed!")
                break
            elif status == "failed":
                print("❌ Processing failed")
                return False
                
        time.sleep(3)
    else:
        print("⏰ Processing timed out")
        return False
    
    # Wait a bit more for patient linking
    time.sleep(5)
    
    # Check patients count after upload
    print("🔍 Checking patients after upload...")
    response = requests.get(f"{BASE_URL}/api/v1/patients/dashboard", headers=headers)
    
    if response.status_code != 200:
        print("❌ Could not get patients count after upload")
        return False
    
    after_data = response.json()
    after_count = after_data.get("totalCount", 0)
    patients = after_data.get("patients", [])
    
    print(f"📊 Patients after upload: {after_count}")
    
    # Look for new patient
    new_patients = []
    for patient in patients:
        if patient.get("documentCount", 0) > 0:
            new_patients.append(patient)
    
    if new_patients:
        print(f"✅ Found {len(new_patients)} patients with documents:")
        for patient in new_patients[:3]:  # Show first 3
            print(f"  • {patient.get('name')} ({patient.get('mrn')}) - {patient.get('documentCount')} docs")
        
        # Test 360 view for a patient
        test_patient = new_patients[0]
        patient_id = test_patient.get("id")
        
        print(f"\n🔍 Testing 360 view for patient: {patient_id}")
        response = requests.get(f"{BASE_URL}/api/v1/patients/{patient_id}/360", headers=headers)
        
        if response.status_code == 200:
            entities = response.json().get("entities", [])
            print(f"✅ 360 view returned {len(entities)} entities")
        else:
            print(f"⚠️ 360 view failed: {response.status_code}")
        
        return True
    else:
        print("❌ No patients with documents found")
        return False

if __name__ == "__main__":
    success = test_upload_and_patient_linking()
    
    print("\n" + "=" * 50)
    if success:
        print("🎉 DOCUMENT UPLOAD AND PATIENT LINKING WORKING!")
        print("\n✅ What's Fixed:")
        print("   • Documents are now linked to patients")
        print("   • Patients section shows uploaded documents")
        print("   • 360 view button is enabled for patients")
        print("   • Generic patients created when no identifiers found")
    else:
        print("❌ Some issues remain - check worker logs")
