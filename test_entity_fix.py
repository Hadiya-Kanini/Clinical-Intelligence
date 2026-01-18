#!/usr/bin/env python3
"""
Test the entity storage fix
"""

import requests
import json
import time

BASE_URL = "http://localhost:5000"

def test_entity_storage_fix():
    """Test that entity storage now works with the fallback mechanism"""
    print("🧪 Testing Entity Storage Fix")
    print("=" * 40)
    
    # Login
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    if response.status_code != 200:
        print("❌ Login failed")
        return
    
    token = response.json().get("access_token")
    headers = {"Authorization": f"Bearer {token}"}
    print("✅ Login successful")
    
    # Upload a new document to test the fix
    test_pdf_path = "c:/Users/HadiyaAmber/Desktop/Clinical-Intelligence/Report_2 5.pdf"
    
    print("📄 Uploading test document...")
    with open(test_pdf_path, 'rb') as f:
        files = {'file': (os.path.basename(test_pdf_path), f, 'application/pdf')}
        response = requests.post(f"{BASE_URL}/api/v1/documents/upload", 
                               files=files, headers=headers)
    
    if response.status_code != 200:
        print(f"❌ Upload failed: {response.status_code}")
        return
    
    document_id = response.json().get("documentId")
    print(f"✅ Document uploaded: {document_id}")
    
    # Wait for processing
    print("⏳ Waiting for document processing...")
    max_wait = 120  # 2 minutes
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
                break
                
        time.sleep(5)
    
    # Check 360 view for entities
    print("🔍 Checking 360 view for entities...")
    response = requests.get(f"{BASE_URL}/api/v1/entities/360-view?documentId={document_id}", 
                          headers=headers)
    
    if response.status_code == 200:
        entities = response.json().get("entities", [])
        print(f"✅ Found {len(entities)} entities in 360 view")
        
        if entities:
            print("📋 Entity Categories:")
            categories = {}
            for entity in entities:
                cat = entity.get("entity_group_name", "unknown")
                if cat not in categories:
                    categories[cat] = 0
                categories[cat] += 1
            
            for cat, count in categories.items():
                print(f"  • {cat}: {count} entities")
        else:
            print("⚠️ Still no entities found - checking worker logs")
    else:
        print(f"❌ 360 view failed: {response.status_code}")

if __name__ == "__main__":
    import os
    test_entity_storage_fix()
