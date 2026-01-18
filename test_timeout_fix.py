#!/usr/bin/env python3
"""
Test document upload to verify the timeout fix works.
"""

import requests
import json
import os

def test_document_upload():
    """Test uploading a document and processing it."""
    
    print("🧪 Testing Document Upload with Timeout Fix")
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
        
        # Use existing test PDF
        test_file_path = "test-document.pdf"
        
        if not os.path.exists(test_file_path):
            print(f"❌ Test file not found: {test_file_path}")
            return False
        
        print(f"📄 Using existing test document: {test_file_path}")
        
        # Upload document
        with open(test_file_path, "rb") as f:
            files = {"file": (test_file_path, f, "application/pdf")}
            upload_response = requests.post(
                "http://localhost:5000/api/v1/documents/upload",
                files=files,
                cookies=session_cookies
            )
        
        if upload_response.status_code == 200:
            upload_data = upload_response.json()
            document_id = upload_data.get("documentId")
            print(f"✅ Document uploaded successfully: {document_id}")
            
            # Wait a bit for processing
            print("⏳ Waiting for processing...")
            import time
            time.sleep(10)
            
            # Check document status
            status_response = requests.get(
                f"http://localhost:5000/api/v1/documents/{document_id}/status",
                cookies=session_cookies
            )
            
            if status_response.status_code == 200:
                status_data = status_response.json()
                print(f"📊 Document status: {status_data.get('status')}")
                
                if status_data.get("status") == "completed":
                    print("🎉 Document processed successfully!")
                    
                    # Check for entities
                    entities_response = requests.get(
                        f"http://localhost:5000/api/v1/documents/{document_id}/entities",
                        cookies=session_cookies
                    )
                    
                    if entities_response.status_code == 200:
                        entities_data = entities_response.json()
                        entity_count = len(entities_data.get("entities", []))
                        print(f"📋 Extracted {entity_count} entities")
                        
                        if entity_count > 0:
                            print("✅ Entity extraction working with timeout fix!")
                            return True
                        else:
                            print("⚠️ No entities extracted")
                            return False
                    else:
                        print(f"❌ Failed to get entities: {entities_response.status_code}")
                        return False
                else:
                    print(f"⚠️ Document not completed: {status_data}")
                    return False
            else:
                print(f"❌ Failed to check status: {status_response.status_code}")
                return False
        else:
            print(f"❌ Upload failed: {upload_response.status_code}")
            print(f"Response: {upload_response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

if __name__ == "__main__":
    success = test_document_upload()
    if success:
        print("\n🎉 TIMEOUT FIX TEST PASSED!")
    else:
        print("\n💥 TIMEOUT FIX TEST FAILED!")
