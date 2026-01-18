#!/usr/bin/env python3
"""
Check the improved extraction results
"""

import requests

BASE_URL = "http://localhost:5000"

def check_improved_results():
    """Check the improved extraction results"""
    document_id = "6633081e-111d-4df6-b0d3-000baf514d9e"
    
    print("🔍 Checking Improved Extraction Results")
    print("=" * 40)
    print(f"📄 Document ID: {document_id}")
    
    # Login
    login_data = {"email": "test@example.com", "password": "Test123456"}
    response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    
    if response.status_code != 200:
        print("❌ Login failed")
        return
    
    token = response.json().get("access_token")
    headers = {"Authorization": f"Bearer {token}"}
    
    # Get 360 view
    response = requests.get(f"{BASE_URL}/api/v1/entities/360-view?documentId={document_id}", 
                          headers=headers)
    
    if response.status_code == 200:
        data = response.json()
        entities = data.get("entities", [])
        print(f"✅ Found {len(entities)} entities in 360 view")
        
        if entities:
            print("\n📋 Entity Quality Analysis:")
            generic_entities = 0
            specific_entities = 0
            
            # Analyze entity quality
            for entity in entities:
                name = entity.get("name", "unnamed")
                value = entity.get("value", "")
                category = entity.get("category", "unknown")
                
                # Check if it's a generic placeholder
                if any(keyword in name.lower() for keyword in ["mentioned", "present", "information", "document_type", "processing_date"]):
                    generic_entities += 1
                    print(f"  ❌ Generic: {category} - {name}: {value}")
                else:
                    specific_entities += 1
                    print(f"  ✅ Specific: {category} - {name}: {value}")
            
            print(f"\n📊 Quality Summary:")
            print(f"  • Specific entities: {specific_entities} 🎯")
            print(f"  • Generic entities: {generic_entities} ⚠️")
            print(f"  • Quality ratio: {specific_entities}/{len(entities)} ({specific_entities/len(entities)*100:.1f}%)")
            
            if specific_entities > generic_entities:
                print(f"\n🎉 EXCELLENT! More specific than generic entities!")
            elif specific_entities > len(entities) * 0.5:
                print(f"\n✅ GOOD! Majority are specific entities!")
            elif specific_entities > 0:
                print(f"\n👍 IMPROVED! Some specific entities found!")
            else:
                print(f"\n⚠️ Still mostly generic, but fallback is working")
                
        else:
            print("❌ No entities found")
    else:
        print(f"❌ 360 view failed: {response.status_code}")

if __name__ == "__main__":
    check_improved_results()
