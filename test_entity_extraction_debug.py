#!/usr/bin/env python3
"""
Debug script to test entity extraction and see raw Gemini response
"""

import os
import sys

# Add worker directory to path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'worker'))

def test_entity_extraction():
    """Test entity extraction with debug logging"""
    try:
        from worker.main import run_entity_extraction_pipeline
        from worker.entity_extraction.models import ChunkWithProvenance
        
        # Create a simple test job payload
        test_job = {
            "schema_version": "1.0",
            "job_id": "test-job-123",
            "document_id": "test-doc-456",
            "status": "pending",
            "payload": {
                "storage_path": "test/path.pdf",
                "mime_type": "application/pdf",
                "document_id": "test-doc-456"
            }
        }
        
        # Mock text extraction result for testing
        from unittest.mock import patch
        
        def mock_extract_text(job_payload):
            return {
                "segments": [
                    {
                        "text": "Patient: Olivia Type, DOB: 1985-03-15, MRN: 104. Allergies: Penicillin. Medications: Lisinopril 10mg daily.",
                        "page": 1,
                        "section": "Summary"
                    }
                ],
                "patient_id": "test-patient-789"
            }
        
        # Run the pipeline with mocked text extraction
        with patch('worker.main.extract_text_from_job', side_effect=mock_extract_text):
            result = run_entity_extraction_pipeline(test_job)
            
        print("✅ Entity extraction test completed")
        print(f"Result: {result}")
        return True
        
    except Exception as e:
        print(f"❌ Entity extraction test failed: {e}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    print("Testing entity extraction with debug logging...")
    test_entity_extraction()
