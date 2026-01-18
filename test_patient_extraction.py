#!/usr/bin/env python3
"""
Test patient demographics extraction from sample PDF
"""
import sys
import os
sys.path.append(os.path.join(os.path.dirname(__file__), 'worker'))

from entity_extraction.patient_extractor import extract_patient_from_text

def test_extraction():
    """Test patient extraction from the extracted PDF text"""
    print("=" * 80)
    print("Testing Patient Demographics Extraction")
    print("=" * 80)
    
    # Read the extracted text
    with open('sample_pdf_extracted_text.txt', 'r', encoding='utf-8') as f:
        text = f.read()
    
    print(f"\n📄 Document text length: {len(text)} characters")
    
    # Extract patient demographics
    print("\n🔍 Extracting patient demographics...")
    demographics = extract_patient_from_text(text)
    
    # Display results
    print("\n✅ Extraction Results:")
    print("-" * 80)
    print(f"MRN:              {demographics.get('mrn', 'Not found')}")
    print(f"Patient Name:     {demographics.get('name', 'Not found')}")
    print(f"Date of Birth:    {demographics.get('dob', 'Not found')}")
    print(f"Gender:           {demographics.get('gender', 'Not found')}")
    print(f"Phone:            {demographics.get('phone', 'Not found')}")
    print(f"Age:              {demographics.get('age', 'Not found')}")
    print(f"Valid:            {demographics.get('is_valid', False)}")
    
    if demographics.get('validation_errors'):
        print(f"\n⚠️ Validation Errors:")
        for error in demographics['validation_errors']:
            print(f"  - {error}")
    
    print("\n" + "=" * 80)
    
    if demographics.get('is_valid'):
        print("✅ Patient demographics extracted successfully!")
    else:
        print("⚠️ Patient demographics extraction incomplete")
    
    return demographics

if __name__ == "__main__":
    test_extraction()
