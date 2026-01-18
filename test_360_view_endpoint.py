#!/usr/bin/env python3
"""
Test script to verify the 360° view endpoint works with authentication.
"""

import requests
import json

def test_360_view_endpoint():
    """Test the 360° view endpoint with authentication."""
    
    # Login to get auth token
    print("🔐 Logging in...")
    login_data = {
        "email": "test@example.com", 
        "password": "Test123456"
    }
    
    try:
        # Login
        login_response = requests.post("http://localhost:5000/api/v1/auth/login", json=login_data)
        if login_response.status_code != 200:
            print(f"❌ Login failed: {login_response.status_code} - {login_response.text}")
            return False
        
        # Get session cookies
        session_cookies = login_response.cookies
        print("✅ Login successful")
        
        # Test 360° view endpoint
        print("🎯 Testing 360° view endpoint...")
        
        entities_response = requests.get(
            "http://localhost:5000/api/v1/entities/360-view",
            cookies=session_cookies
        )
        
        if entities_response.status_code == 200:
            data = entities_response.json()
            entities = data.get('entities', [])
            
            print(f"✅ 360° view working! Found {len(entities)} entities")
            
            if entities:
                print("\n📋 Sample entities:")
                for i, entity in enumerate(entities[:3]):
                    print(f"  {i+1}. {entity['category']}: {entity['name']} = {entity['value']}")
                    print(f"     📂 Display: {entity.get('displayCategory', 'N/A')}")
                    print(f"     👤 Patient: {entity.get('patientName', 'N/A')}")
                    print()
                
                print(f"👤 Patient: {entities[0].get('patientName', 'N/A')} ({entities[0].get('patientMrn', 'N/A')})")
                print(f"📄 Document: {entities[0].get('documentName', 'N/A')}")
                print(f"📅 Date: {entities[0].get('documentDate', 'N/A')}")
            
            return True
        else:
            print(f"❌ 360° view failed: {entities_response.status_code} - {entities_response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

if __name__ == "__main__":
    test_360_view_endpoint()
