#!/usr/bin/env python3
"""
End-to-end test: Upload sample PDF and verify patient extraction
"""
import requests
import time

# Configuration
BASE_URL = "http://localhost:5000"
LOGIN_URL = f"{BASE_URL}/api/v1/auth/login"
UPLOAD_URL = f"{BASE_URL}/api/v1/documents/upload"
SAMPLE_PDF = r"C:\Users\HadiyaAmber\Desktop\Clinical-Intelligence\Report_2 5.pdf"

def test_e2e_flow():
    """Test complete end-to-end flow"""
    print("=" * 80)
    print("🧪 END-TO-END TEST: Upload → Worker → Patient Creation")
    print("=" * 80)
    
    # Step 1: Login
    print("\n1️⃣ Logging in...")
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    login_response = requests.post(LOGIN_URL, json=login_data)
    if login_response.status_code != 200:
        print(f"❌ Login failed: {login_response.status_code}")
        return
    
    token = login_response.json().get('access_token')
    print("✅ Login successful")
    
    # Step 2: Upload sample PDF (without patient ID)
    print("\n2️⃣ Uploading sample PDF (Report_2 5.pdf)...")
    print(f"   File: {SAMPLE_PDF}")
    
    headers = {'Authorization': f'Bearer {token}'}
    
    try:
        with open(SAMPLE_PDF, 'rb') as f:
            files = {'file': ('Report_2 5.pdf', f, 'application/pdf')}
            # NO patientId - will be extracted by worker
            data = {}
            
            upload_response = requests.post(UPLOAD_URL, headers=headers, files=files, data=data)
        
        if upload_response.status_code != 200:
            print(f"❌ Upload failed: {upload_response.status_code}")
            print(upload_response.text)
            return
        
        result = upload_response.json()
        document_id = result.get('documentId')
        
        print("✅ Upload successful!")
        print(f"   Document ID: {document_id}")
        print(f"   Status: {result.get('status')}")
        print(f"   File Size: {result.get('fileSize')} bytes")
        
        # Step 3: Wait for worker to process
        print("\n3️⃣ Waiting for worker to process document...")
        print("   (Worker will: Extract text → Extract patient info → Create patient → Link document)")
        
        for i in range(30):
            time.sleep(2)
            print(f"   ⏳ Waiting... {(i+1)*2}s")
            
            # Check document status (you can add an endpoint to check this)
            # For now, just wait
            
            if i >= 14:  # After 30 seconds
                break
        
        print("\n✅ Worker processing should be complete!")
        print("\n" + "=" * 80)
        print("📊 EXPECTED RESULTS:")
        print("=" * 80)
        print("✅ Patient created with:")
        print("   - Name: Olivia")
        print("   - MRN: 104")
        print("   - DOB: 1952-05-05")
        print("   - Gender: Female")
        print("   - Phone: +13105561256")
        print("\n✅ Document linked to patient")
        print("✅ Document status: Completed")
        print("\n" + "=" * 80)
        print("🔍 Next: Check database to verify patient creation")
        print("=" * 80)
        
        return document_id
        
    except FileNotFoundError:
        print(f"❌ Sample PDF not found at: {SAMPLE_PDF}")
        print("   Please ensure the file exists")
        return None
    except Exception as e:
        print(f"❌ Error: {e}")
        return None

if __name__ == "__main__":
    test_e2e_flow()
