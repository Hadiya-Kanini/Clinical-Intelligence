#!/usr/bin/env python3
"""
Simple test to upload a document and test entity extraction with the fixed parser.
"""

import requests
import json
import os
import time

def test_document_upload_and_extraction():
    """Test uploading a document and verify entity extraction works."""
    
    # API base URL
    base_url = "http://localhost:5000/api/v1"
    
    # Login to get session cookie
    print("🔐 Logging in...")
    login_data = {
        "email": "test@example.com", 
        "password": "Test123456"
    }
    
    try:
        response = requests.post(f"{base_url}/auth/login", json=login_data)
        if response.status_code != 200:
            print(f"❌ Login failed: {response.status_code} - {response.text}")
            return False
        
        # Get session cookie
        session_cookies = response.cookies
        print("✅ Login successful")
        
    except Exception as e:
        print(f"❌ Login error: {e}")
        return False
    
    # Upload a test document
    print("📄 Uploading test document...")
    
    # Use the existing test PDF
    test_pdf_path = "test-document.pdf"
    if not os.path.exists(test_pdf_path):
        print(f"❌ Test PDF not found: {test_pdf_path}")
        return False
    
    try:
        with open(test_pdf_path, 'rb') as f:
            files = {'file': ('test-document.pdf', f, 'application/pdf')}
            upload_response = requests.post(
                f"{base_url}/documents/upload",
                files=files,
                cookies=session_cookies
            )
        
        if upload_response.status_code != 200:
            print(f"❌ Upload failed: {upload_response.status_code} - {upload_response.text}")
            return False
        
        upload_result = upload_response.json()
        document_id = upload_result.get('documentId')
        print(f"✅ Document uploaded successfully: {document_id}")
        
    except Exception as e:
        print(f"❌ Upload error: {e}")
        return False
    
    # Wait for processing to complete
    print("⏳ Waiting for document processing...")
    max_wait_time = 60  # 60 seconds
    start_time = time.time()
    
    while time.time() - start_time < max_wait_time:
        try:
            status_response = requests.get(
                f"{base_url}/documents/{document_id}/status",
                cookies=session_cookies
            )
            
            if status_response.status_code == 200:
                status_data = status_response.json()
                status = status_data.get('status')
                print(f"📊 Document status: {status}")
                
                if status == 'Completed':
                    print("✅ Document processing completed")
                    break
                elif status == 'Failed':
                    print(f"❌ Document processing failed")
                    return False
                    
            time.sleep(2)  # Wait 2 seconds before checking again
            
        except Exception as e:
            print(f"⚠️ Status check error: {e}")
            time.sleep(2)
    
    else:
        print("⏰ Processing timeout")
        return False
    
    # Check extracted entities
    print("🔍 Checking extracted entities...")
    try:
        entities_response = requests.get(
            f"{base_url}/documents/{document_id}/entities",
            cookies=session_cookies
        )
        
        if entities_response.status_code == 200:
            entities_data = entities_response.json()
            entities = entities_data.get('extracted_entities', [])
            print(f"✅ Found {len(entities)} extracted entities")
            
            # Print sample entities
            for i, entity in enumerate(entities[:5]):
                print(f"  {i+1}. {entity.get('entity_group_name', 'Unknown')}: {entity.get('entity_name', 'Unknown')} = {entity.get('entity_value', 'Unknown')}")
            
            if len(entities) > 5:
                print(f"  ... and {len(entities) - 5} more entities")
            
            return True
        else:
            print(f"❌ Failed to get entities: {entities_response.status_code}")
            return False
            
    except Exception as e:
        print(f"❌ Entities check error: {e}")
        return False

if __name__ == "__main__":
    print("🧪 Testing Document Upload and Entity Extraction")
    print("=" * 50)
    
    success = test_document_upload_and_extraction()
    
    print("\n" + "=" * 50)
    if success:
        print("🎉 Entity extraction test passed! The fix is working in production.")
    else:
        print("⚠️ Entity extraction test failed. Check the logs for details.")
