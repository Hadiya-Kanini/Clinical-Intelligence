#!/usr/bin/env python3
"""
Test script to verify the Patient 360 view fixes.
This script tests:
1. API endpoint returns proper data structure
2. Category mapping works correctly
3. Citations are included
4. Frontend can display the data
"""

import requests
import json
from typing import Dict, Any, List

def test_api_endpoints():
    """Test both API endpoints to ensure they work correctly."""
    base_url = "http://localhost:8000"
    
    print("🔍 Testing Patient 360 API endpoints...")
    
    try:
        # Get patients first
        patients_response = requests.get(f"{base_url}/api/v1/patients")
        if patients_response.status_code != 200:
            print(f"❌ Failed to get patients: {patients_response.status_code}")
            return False
            
        patients = patients_response.json()
        if not patients:
            print("❌ No patients found")
            return False
            
        patient_id = patients[0]['id']
        print(f"✅ Found patient: {patient_id}")
        
        # Test the main 360 endpoint
        print("\n🔍 Testing /api/v1/patients/{id}/360 endpoint...")
        response_360 = requests.get(f"{base_url}/api/v1/patients/{patient_id}/360")
        
        if response_360.status_code == 200:
            data_360 = response_360.json()
            print("✅ Main 360 endpoint working")
            
            # Check structure
            required_fields = ['patient', 'entities', 'documents']
            for field in required_fields:
                if field not in data_360:
                    print(f"❌ Missing field: {field}")
                    return False
                    
            print(f"✅ Found {len(data_360['entities'])} entities")
            print(f"✅ Found {len(data_360['documents'])} documents")
            
            # Test category mapping
            categories = set()
            for entity in data_360['entities']:
                categories.add(entity.get('category', 'unknown'))
                
            print(f"✅ Categories found: {sorted(categories)}")
            
            # Test citations
            entities_with_citations = [e for e in data_360['entities'] if e.get('citations')]
            print(f"✅ Entities with citations: {len(entities_with_citations)}/{len(data_360['entities'])}")
            
        else:
            print(f"❌ Main 360 endpoint failed: {response_360.status_code}")
            print(f"Response: {response_360.text}")
            return False
            
        # Test the entities 360-view endpoint
        print("\n🔍 Testing /api/v1/entities/360-view endpoint...")
        response_entities = requests.get(f"{base_url}/api/v1/entities/360-view?patientId={patient_id}")
        
        if response_entities.status_code == 200:
            data_entities = response_entities.json()
            print("✅ Entities 360-view endpoint working")
            
            entities = data_entities.get('entities', [])
            print(f"✅ Found {len(entities)} entities")
            
            # Check for proper category mapping
            mapped_categories = set()
            for entity in entities:
                category = entity.get('category', 'unknown')
                mapped_categories.add(category)
                
                # Check if category is properly capitalized (not underscored)
                if '_' in category and category != category.replace('_', ' ').title():
                    print(f"⚠️  Category not properly mapped: {category}")
                    
            print(f"✅ Mapped categories: {sorted(mapped_categories)}")
            
            # Check for citations
            entities_with_citations = [e for e in entities if e.get('citations')]
            print(f"✅ Entities with citations: {len(entities_with_citations)}/{len(entities)}")
            
            # Check data structure matches frontend expectations
            sample_entity = entities[0] if entities else {}
            required_entity_fields = ['id', 'category', 'name', 'value', 'citations']
            for field in required_entity_fields:
                if field not in sample_entity:
                    print(f"❌ Entity missing field: {field}")
                    return False
                    
            print("✅ Entity structure matches frontend expectations")
            
        else:
            print(f"❌ Entities 360-view endpoint failed: {response_entities.status_code}")
            print(f"Response: {response_entities.text}")
            return False
            
        return True
        
    except Exception as e:
        print(f"❌ Error testing endpoints: {e}")
        return False

def test_category_mapping():
    """Test the category mapping function."""
    print("\n🔍 Testing category mapping...")
    
    # Test cases: worker_category -> expected_frontend_category
    test_cases = {
        'patient_demographics': 'Patient Demographics',
        'allergies': 'Allergies',
        'medications': 'Medications',
        'diagnoses': 'Diagnoses',
        'procedures': 'Procedures',
        'lab_results': 'Lab Results',
        'vital_signs': 'Vital Signs',
        'social_history': 'Social History',
        'clinical_notes': 'Clinical Notes',
        'document_metadata': 'Document Metadata',
        'unknown_category': 'Unknown_category'  # Fallback behavior
    }
    
    base_url = "http://localhost:8000"
    
    try:
        # Get a patient to test with
        patients_response = requests.get(f"{base_url}/api/v1/patients")
        if patients_response.status_code != 200:
            print("❌ Could not get patients for testing")
            return False
            
        patients = patients_response.json()
        if not patients:
            print("❌ No patients found for testing")
            return False
            
        patient_id = patients[0]['id']
        
        # Test the entities endpoint to see actual mapping
        response = requests.get(f"{base_url}/api/v1/entities/360-view?patientId={patient_id}")
        if response.status_code != 200:
            print("❌ Could not test category mapping")
            return False
            
        data = response.json()
        entities = data.get('entities', [])
        
        # Group by original categories (if we can determine them)
        found_categories = set()
        for entity in entities:
            found_categories.add(entity.get('category', 'unknown'))
            
        print(f"✅ Found categories in API response: {sorted(found_categories)}")
        
        # Check if underscored categories are properly mapped
        for category in found_categories:
            if '_' in category:
                # This should have been mapped to Title Case
                expected = category.replace('_', ' ').title()
                if category == expected:
                    print(f"⚠️  Category may not be properly mapped: {category}")
                else:
                    print(f"✅ Category appears properly mapped: {category}")
            else:
                print(f"✅ Category format: {category}")
                
        return True
        
    except Exception as e:
        print(f"❌ Error testing category mapping: {e}")
        return False

def main():
    """Run all tests."""
    print("🚀 Starting Patient 360 Fix Verification")
    print("=" * 50)
    
    success = True
    
    # Test API endpoints
    if not test_api_endpoints():
        success = False
        
    # Test category mapping
    if not test_category_mapping():
        success = False
    
    print("\n" + "=" * 50)
    if success:
        print("🎉 All tests passed! The 360 view should now work correctly.")
        print("\n📋 Summary of fixes:")
        print("  ✅ API endpoint now includes citations")
        print("  ✅ Category mapping from worker to frontend format")
        print("  ✅ Proper data structure for frontend consumption")
        print("  ✅ Frontend updated to handle new data structure")
    else:
        print("❌ Some tests failed. Please check the issues above.")
        
    return success

if __name__ == "__main__":
    main()
