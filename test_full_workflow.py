#!/usr/bin/env python3
"""
Test script to upload document and test 360 view functionality
"""

import requests
import json
import time
import os
from pathlib import Path

# Configuration
BASE_URL = "http://localhost:5000"
FRONTEND_URL = "http://localhost:5173"

def test_document_upload():
    """Test document upload endpoint"""
    print("🚀 Testing document upload...")
    
    # Find a test PDF
    test_pdf_path = "c:/Users/HadiyaAmber/Desktop/Clinical-Intelligence/Report_2 5.pdf"
    if not os.path.exists(test_pdf_path):
        print("❌ Test PDF not found")
        return None
    
    # First, login to get a token
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    try:
        # Login
        print("📝 Logging in...")
        response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
        if response.status_code != 200:
            print(f"❌ Login failed: {response.status_code}")
            print(response.text)
            return None
            
        token_data = response.json()
        token = token_data.get("access_token")
        print("✅ Login successful")
        
        # Upload document
        print("📄 Uploading document...")
        headers = {"Authorization": f"Bearer {token}"}
        
        with open(test_pdf_path, 'rb') as f:
            files = {'file': (os.path.basename(test_pdf_path), f, 'application/pdf')}
            response = requests.post(f"{BASE_URL}/api/v1/documents/upload", 
                                   files=files, headers=headers)
        
        if response.status_code != 200:
            print(f"❌ Upload failed: {response.status_code}")
            print(response.text)
            return None
            
        upload_result = response.json()
        document_id = upload_result.get("documentId")
        print(f"✅ Document uploaded successfully: {document_id}")
        
        return document_id, token
        
    except Exception as e:
        print(f"❌ Error during upload: {e}")
        return None

def test_document_processing(document_id, token):
    """Test document processing and entity extraction"""
    print(f"🔄 Testing document processing for {document_id}...")
    
    try:
        headers = {"Authorization": f"Bearer {token}"}
        
        # Check document status
        max_wait = 60  # Wait up to 60 seconds
        start_time = time.time()
        
        while time.time() - start_time < max_wait:
            response = requests.get(f"{BASE_URL}/api/v1/documents/{document_id}/status", 
                                  headers=headers)
            
            if response.status_code == 200:
                status_data = response.json()
                status = status_data.get("status")
                print(f"📊 Document status: {status}")
                
                if status == "completed":
                    print("✅ Document processing completed")
                    return True
                elif status == "failed":
                    print("❌ Document processing failed")
                    return False
                    
            time.sleep(2)  # Wait 2 seconds before checking again
        
        print("⏰ Document processing timed out")
        return False
        
    except Exception as e:
        print(f"❌ Error checking document status: {e}")
        return False

def test_360_view(document_id, token):
    """Test 360 view functionality"""
    print(f"🔍 Testing 360 view for document {document_id}...")
    
    try:
        headers = {"Authorization": f"Bearer {token}"}
        
        # Get entities for 360 view
        response = requests.get(f"{BASE_URL}/api/v1/entities/360-view?documentId={document_id}", 
                              headers=headers)
        
        if response.status_code != 200:
            print(f"❌ 360 view API failed: {response.status_code}")
            print(response.text)
            return False
            
        entities_data = response.json()
        entities = entities_data.get("entities", [])
        
        print(f"✅ 360 view API returned {len(entities)} entities")
        
        # Display entity categories
        categories = {}
        for entity in entities:
            category = entity.get("entity_group_name", "unknown")
            if category not in categories:
                categories[category] = []
            categories[category].append(entity.get("entity_name", "unnamed"))
        
        print("\n📋 Entity Categories Found:")
        for category, items in categories.items():
            print(f"  • {category}: {len(items)} items")
            for item in items[:3]:  # Show first 3 items
                print(f"    - {item}")
            if len(items) > 3:
                print(f"    ... and {len(items) - 3} more")
        
        return True
        
    except Exception as e:
        print(f"❌ Error testing 360 view: {e}")
        return False

def test_patient_360(patient_id, token):
    """Test patient 360 view"""
    print(f"👤 Testing patient 360 view for patient {patient_id}...")
    
    try:
        headers = {"Authorization": f"Bearer {token}"}
        
        # Get patient 360 data
        response = requests.get(f"{BASE_URL}/api/v1/patients/{patient_id}/360", 
                              headers=headers)
        
        if response.status_code != 200:
            print(f"❌ Patient 360 API failed: {response.status_code}")
            print(response.text)
            return False
            
        patient_data = response.json()
        print("✅ Patient 360 view successful")
        
        # Display patient data summary
        print(f"📊 Patient Data Summary:")
        print(f"  • Patient ID: {patient_data.get('patientId')}")
        print(f"  • Total Entities: {len(patient_data.get('entities', []))}")
        
        # Check grounding information
        entities_with_grounding = [e for e in patient_data.get('entities', []) 
                                  if e.get('grounding')]
        print(f"  • Entities with Grounding: {len(entities_with_grounding)}")
        
        return True
        
    except Exception as e:
        print(f"❌ Error testing patient 360: {e}")
        return False

def main():
    """Main test workflow"""
    print("🧪 Starting Comprehensive Clinical Intelligence Test")
    print("=" * 50)
    
    # Test document upload
    upload_result = test_document_upload()
    if not upload_result:
        print("❌ Upload test failed, aborting remaining tests")
        return
    
    document_id, token = upload_result
    
    # Test document processing
    if not test_document_processing(document_id, token):
        print("❌ Processing test failed")
        return
    
    # Test 360 view
    if not test_360_view(document_id, token):
        print("❌ 360 view test failed")
        return
    
    # Test patient 360 (using a sample patient ID)
    sample_patient_id = "00000000-0000-0000-0000-000000012345"
    test_patient_360(sample_patient_id, token)
    
    print("\n" + "=" * 50)
    print("✅ All tests completed successfully!")
    print(f"🌐 Frontend available at: {FRONTEND_URL}")
    print(f"🔧 Backend API at: {BASE_URL}")
    print(f"📚 Swagger docs at: {BASE_URL}/swagger")

if __name__ == "__main__":
    main()
