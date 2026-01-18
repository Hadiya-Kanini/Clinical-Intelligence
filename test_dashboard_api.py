#!/usr/bin/env python3
"""
Test Patient Dashboard API endpoint
"""
import requests
import json

BASE_URL = "http://localhost:5000/api/v1"

def test_dashboard():
    print("=" * 80)
    print("🧪 Testing Patient Dashboard API")
    print("=" * 80)
    
    # Login first
    print("\n1️⃣ Logging in...")
    login_response = requests.post(
        f"{BASE_URL}/auth/login",
        json={
            "email": "test@example.com",
            "password": "Test123456"
        }
    )
    
    if login_response.status_code != 200:
        print(f"❌ Login failed: {login_response.status_code}")
        print(login_response.text)
        return
    
    print("✅ Login successful")
    
    # Get cookies
    cookies = login_response.cookies
    
    # Test dashboard endpoint
    print("\n2️⃣ Fetching patient dashboard...")
    dashboard_response = requests.get(
        f"{BASE_URL}/patients/dashboard",
        cookies=cookies
    )
    
    if dashboard_response.status_code != 200:
        print(f"❌ Dashboard request failed: {dashboard_response.status_code}")
        print(dashboard_response.text)
        return
    
    print("✅ Dashboard data retrieved")
    
    data = dashboard_response.json()
    
    print("\n" + "=" * 80)
    print("📊 Dashboard Results")
    print("=" * 80)
    print(f"Total Patients: {data['totalCount']}")
    print(f"Page: {data['page']} of {data['totalPages']}")
    print(f"Page Size: {data['pageSize']}")
    
    if data['patients']:
        print(f"\n📋 Patients ({len(data['patients'])} shown):")
        print("-" * 80)
        for patient in data['patients']:
            print(f"\n👤 {patient['name']}")
            print(f"   MRN: {patient['mrn']}")
            print(f"   DOB: {patient.get('dateOfBirth', 'N/A')}")
            print(f"   Contact: {patient.get('contact', 'N/A')}")
            print(f"   Documents: {patient['documentCount']}")
            print(f"   Last Upload: {patient.get('lastDocumentUploadedAt', 'Never')}")
            print(f"   Created: {patient['createdAt']}")
    else:
        print("\n⚠️ No patients found in database")
    
    # Test search functionality
    print("\n" + "=" * 80)
    print("3️⃣ Testing search functionality...")
    print("=" * 80)
    
    search_response = requests.get(
        f"{BASE_URL}/patients/dashboard",
        params={"search": "Olivia"},
        cookies=cookies
    )
    
    if search_response.status_code == 200:
        search_data = search_response.json()
        print(f"✅ Search results: {search_data['totalCount']} patients found")
        if search_data['patients']:
            for patient in search_data['patients']:
                print(f"   - {patient['name']} (MRN: {patient['mrn']})")
    else:
        print(f"❌ Search failed: {search_response.status_code}")
    
    print("\n" + "=" * 80)
    print("✅ Dashboard API Test Complete!")
    print("=" * 80)

if __name__ == "__main__":
    test_dashboard()
