#!/usr/bin/env python3
"""
Test script to verify the entity extraction JSON parsing fix.
"""

import json
import sys
import os
import logging

# Set up debug logging
logging.basicConfig(level=logging.DEBUG)

# Add the worker directory to the path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'worker'))

from worker.entity_extraction.response_parser import parse_entity_extraction_response

# Test with a truncated JSON response similar to what we saw in the logs
truncated_response = '''```json
{
  "schema_version": "1.0",
  "document_id": "7918071c-4c10-40db-88ee-08db11d8971e",
  "extracted_entities": [
    {
      "entity_group_name": "patient_demographics",
      "entity_name": "name",
      "entity_value": "Olivia",
      "rationale": "Patient's name identified from 'PATIENT INFORMATION' section.",
      "source_text": "Olivia",
      "document_location": {
        "page": 1,
        "section": "PATIENT INFORMATION"
      },
      "conflicts": []
    },
    {
      "entity_group_name": "patient_demographics",
      "entity_name": "dob",
      "entity_value": "05/05/1952",
      "rationale": "Patient's date of birth identified from 'PATIENT INFORMATION' section.",
      "source_text": "05/05/1952",
      "document_location": {
        "page": 1,
        "section": "PATIENT INFORMATION"
      },
      "conflicts": []
    },
    {
      "entity_group_name": "lab_results",
      "entity_name": "Triglycerides",
      "entity_value": "158(H)",
      "unit": "mg/dL",
      "reference_range": "0-149",
      "date": "07/03/2023",
      "rationale": "Lab result for Triglycerides extracted from 'Results' section.",
      "source_text": "Triglycerides 158('''

def test_truncated_json_parsing():
    """Test that the parser can handle truncated JSON responses."""
    print("🧪 Testing truncated JSON parsing...")
    
    try:
        # This should parse successfully despite being truncated
        parsed = parse_entity_extraction_response(truncated_response)
        
        print(f"✅ Successfully parsed truncated JSON!")
        print(f"📊 Extracted {len(parsed.get('extracted_entities', []))} entities")
        
        # Print some sample entities
        entities = parsed.get('extracted_entities', [])
        for i, entity in enumerate(entities[:3]):
            print(f"  {i+1}. {entity.get('entity_group_name', 'Unknown')}: {entity.get('entity_name', 'Unknown')} = {entity.get('entity_value', 'Unknown')}")
        
        if len(entities) > 3:
            print(f"  ... and {len(entities) - 3} more entities")
        
        return True
        
    except Exception as e:
        print(f"❌ Failed to parse truncated JSON: {e}")
        return False

def test_complete_json_parsing():
    """Test that the parser still works with complete JSON."""
    print("\n🧪 Testing complete JSON parsing...")
    
    complete_response = '''```json
{
  "schema_version": "1.0",
  "document_id": "test-doc-id",
  "extracted_entities": [
    {
      "entity_group_name": "patient_demographics",
      "entity_name": "name",
      "entity_value": "John Doe",
      "rationale": "Test entity",
      "source_text": "John Doe",
      "document_location": {
        "page": 1,
        "section": "TEST"
      },
      "conflicts": []
    }
  ]
}```'''
    
    try:
        parsed = parse_entity_extraction_response(complete_response)
        print(f"✅ Successfully parsed complete JSON!")
        print(f"📊 Extracted {len(parsed.get('extracted_entities', []))} entities")
        return True
        
    except Exception as e:
        print(f"❌ Failed to parse complete JSON: {e}")
        return False

if __name__ == "__main__":
    print("🔧 Testing Entity Extraction JSON Parser Fixes")
    print("=" * 50)
    
    success1 = test_truncated_json_parsing()
    success2 = test_complete_json_parsing()
    
    print("\n" + "=" * 50)
    if success1 and success2:
        print("🎉 All tests passed! The JSON parsing fix is working correctly.")
    else:
        print("⚠️ Some tests failed. Please check the implementation.")
