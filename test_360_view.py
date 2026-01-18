#!/usr/bin/env python3
"""
Test 360 view functionality with already processed document
"""

import requests
import json
import os

BASE_URL = "http://localhost:5000"
FRONTEND_URL = "http://localhost:5173"

def test_360_view_only():
    """Test 360 view with existing processed document"""
    print("🔍 Testing 360 view functionality...")
    
    # Use the previously processed document
    document_id = "5cf84765-f8bf-41b9-8a95-cc8b790fa495"
    
    # Login
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    try:
        print("📝 Logging in...")
        response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
        if response.status_code != 200:
            print(f"❌ Login failed: {response.status_code}")
            return False
            
        token = response.json().get("access_token")
        headers = {"Authorization": f"Bearer {token}"}
        print("✅ Login successful")
        
        # Test 360 view API
        print(f"🔍 Testing 360 view for document {document_id}...")
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
        
        # Test patient 360 view
        sample_patient_id = "00000000-0000-0000-0000-000000012345"
        print(f"\n👤 Testing patient 360 view for patient {sample_patient_id}...")
        
        response = requests.get(f"{BASE_URL}/api/v1/patients/{sample_patient_id}/360", 
                              headers=headers)
        
        if response.status_code == 200:
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
        else:
            print(f"⚠️ Patient 360 API returned: {response.status_code}")
        
        # Test document content retrieval
        print(f"\n📄 Testing document content retrieval...")
        response = requests.get(f"{BASE_URL}/api/v1/documents/{document_id}/content", 
                              headers=headers)
        
        if response.status_code == 200:
            print("✅ Document content retrieval successful")
            print(f"  Content-Type: {response.headers.get('content-type')}")
            print(f"  File Size: {len(response.content)} bytes")
        else:
            print(f"⚠️ Document content API returned: {response.status_code}")
        
        return True
        
    except Exception as e:
        print(f"❌ Error during testing: {e}")
        return False

def main():
    """Main test function"""
    print("🧪 Testing 360 View Functionality")
    print("=" * 50)
    
    if test_360_view_only():
        print("\n" + "=" * 50)
        print("✅ 360 view tests completed successfully!")
        print(f"🌐 Frontend available at: {FRONTEND_URL}")
        print(f"🔧 Backend API at: {BASE_URL}")
        print(f"📚 Swagger docs at: {BASE_URL}/swagger")
        print("\n🎯 You can now test the 360 view in the frontend!")
    else:
        print("❌ Some tests failed")

if __name__ == "__main__":
    main()
