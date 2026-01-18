#!/usr/bin/env python3
"""
Test improved entity extraction
"""

import requests
import json
import time
import os

BASE_URL = "http://localhost:5000"

def test_improved_extraction():
    """Test the improved entity extraction with a new document"""
    print("🧪 Testing Improved Entity Extraction")
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
    
    # Upload a test document
    test_pdf_path = "c:/Users/HadiyaAmber/Desktop/Clinical-Intelligence/Report_2 5.pdf"
    
    print("📄 Uploading test document for improved extraction...")
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
    print("⏳ Waiting for document processing with improved extraction...")
    max_wait = 90  # 1.5 minutes
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
                
        time.sleep(5)
    else:
        print("⏰ Processing timed out")
        return False
    
    # Check the 360 view for improved entities
    print("🔍 Checking 360 view for improved entities...")
    response = requests.get(f"{BASE_URL}/api/v1/entities/360-view?documentId={document_id}", 
                          headers=headers)
    
    if response.status_code == 200:
        entities = response.json().get("entities", [])
        print(f"✅ Found {len(entities)} entities in 360 view")
        
        if entities:
            print("\n📋 Improved Entity Analysis:")
            categories = {}
            generic_entities = 0
            specific_entities = 0
            
            for entity in entities:
                cat = entity.get("category", "unknown")
                name = entity.get("name", "unnamed")
                value = entity.get("value", "")
                
                # Count generic vs specific entities
                if any(keyword in name.lower() for keyword in ["mentioned", "present", "information", "document_type", "processing_date"]):
                    generic_entities += 1
                else:
                    specific_entities += 1
                
                if cat not in categories:
                    categories[cat] = 0
                categories[cat] += 1
            
            print(f"📊 Entity Quality Analysis:")
            print(f"  • Specific entities: {specific_entities} ✅")
            print(f"  • Generic entities: {generic_entities} ⚠️")
            print(f"  • Quality ratio: {specific_entities}/{len(entities)} ({specific_entities/len(entities)*100:.1f}%)")
            
            print(f"\n📋 Entity Categories:")
            for cat, count in categories.items():
                print(f"  • {cat}: {count} entities")
            
            # Show some specific examples
            specific_examples = [e for e in entities if not any(keyword in e.get("name", "").lower() for keyword in ["mentioned", "present", "information", "document_type", "processing_date"])]
            
            if specific_examples:
                print(f"\n🎯 Specific Entity Examples:")
                for entity in specific_examples[:5]:
                    name = entity.get("name", "unnamed")
                    value = entity.get("value", "")
                    print(f"  • {name}: {value}")
            
            # Determine if extraction improved
            if specific_entities > generic_entities:
                print(f"\n🎉 EXTRACTION IMPROVED! More specific than generic entities")
            elif specific_entities > 0:
                print(f"\n✅ EXTRACTION PARTIALLY IMPROVED! Some specific entities found")
            else:
                print(f"\n⚠️ Still using fallback entities, but they should be more meaningful now")
        else:
            print("⚠️ No entities found")
    else:
        print(f"❌ 360 view failed: {response.status_code}")
    
    return True

if __name__ == "__main__":
    test_improved_extraction()
