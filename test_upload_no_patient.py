#!/usr/bin/env python3
"""
Test script to verify document upload WITHOUT patient ID
"""
import requests
import os

# Configuration
BASE_URL = "http://localhost:5000"
LOGIN_URL = f"{BASE_URL}/api/v1/auth/login"
UPLOAD_URL = f"{BASE_URL}/api/v1/documents/upload"

def test_upload_without_patient_id():
    """Test upload without patient ID"""
    print("=" * 50)
    print("Testing Document Upload WITHOUT Patient ID")
    print("=" * 50)
    
    # Step 1: Login
    print("\n1. Logging in...")
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    login_response = requests.post(LOGIN_URL, json=login_data)
    if login_response.status_code != 200:
        print(f"❌ Login failed: {login_response.status_code}")
        print(login_response.text)
        return
    
    token = login_response.json().get('access_token')
    print("✅ Login successful")
    
    # Step 2: Upload document WITHOUT patient ID
    print("\n2. Uploading document without patient ID...")
    
    # Create test PDF file
    test_file_path = "test-document.pdf"
    if not os.path.exists(test_file_path):
        # Create a simple PDF
        with open(test_file_path, 'wb') as f:
            f.write(b'%PDF-1.4\n')
            f.write(b'1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n')
            f.write(b'2 0 obj<</Type/Pages/Count 1/Kids[3 0 R]>>endobj\n')
            f.write(b'3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]>>endobj\n')
            f.write(b'xref\n0 4\n0000000000 65535 f\n')
            f.write(b'0000000009 00000 n\n0000000056 00000 n\n0000000115 00000 n\n')
            f.write(b'trailer<</Size 4/Root 1 0 R>>\nstartxref\n190\n%%EOF\n')
    
    headers = {'Authorization': f'Bearer {token}'}
    
    with open(test_file_path, 'rb') as f:
        files = {'file': ('test-document.pdf', f, 'application/pdf')}
        # NOTE: NO patientId in form data
        data = {}
        
        upload_response = requests.post(UPLOAD_URL, headers=headers, files=files, data=data)
    
    print(f"Upload Status: {upload_response.status_code}")
    
    if upload_response.status_code == 200:
        result = upload_response.json()
        print("✅ Upload successful!")
        print(f"   Document ID: {result.get('documentId')}")
        print(f"   File Name: {result.get('fileName')}")
        print(f"   Status: {result.get('status')}")
        print(f"   Is Valid: {result.get('isValid')}")
        print(f"\n✅ Patient ID is now optional - document will be processed and patient info extracted!")
    else:
        print(f"❌ Upload failed")
        print(upload_response.text)

if __name__ == "__main__":
    test_upload_without_patient_id()
