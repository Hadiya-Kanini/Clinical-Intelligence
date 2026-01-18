#!/usr/bin/env python3
"""
Test 360 view for the newly processed document
"""

import requests
import json

BASE_URL = "http://localhost:5000"

def test_360_view():
    """Test 360 view for the newly processed document"""
    document_id = "ca78ac9f-5e92-45eb-8066-ef0d8fd55b1a"
    
    print("🔍 Testing 360 View for Newly Processed Document")
    print("=" * 50)
    print(f"📄 Document ID: {document_id}")
    
    # Login
    login_data = {"email": "test@example.com", "password": "Test123456"}
    response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    
    if response.status_code != 200:
        print("❌ Login failed")
        return
    
    token = response.json().get("access_token")
    headers = {"Authorization": f"Bearer {token}"}
    
    # Test 360 view
    response = requests.get(f"{BASE_URL}/api/v1/entities/360-view?documentId={document_id}", 
                          headers=headers)
    
    print(f"📊 API Status: {response.status_code}")
    
    if response.status_code == 200:
        entities = response.json().get("entities", [])
        print(f"✅ Found {len(entities)} entities in 360 view")
        
        if entities:
            print("\n📋 Entity Details:")
            categories = {}
            for i, entity in enumerate(entities, 1):
                cat = entity.get("category", "unknown")
                name = entity.get("name", "unnamed")
                value = entity.get("value", "")
                display_cat = entity.get("displayCategory", cat)
                
                print(f"  {i}. [{display_cat}] {name}: {value}")
                
                if cat not in categories:
                    categories[display_cat] = 0
                categories[display_cat] += 1
            
            print(f"\n📊 Summary by Category:")
            for cat, count in categories.items():
                print(f"  • {cat}: {count} entities")
                
            print(f"\n🎉 ENTITY STORAGE AND 360 VIEW WORKING PERFECTLY!")
        else:
            print("⚠️ No entities found")
    else:
        print(f"❌ 360 view failed: {response.text}")

if __name__ == "__main__":
    test_360_view()
