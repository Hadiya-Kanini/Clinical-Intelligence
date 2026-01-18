"""Unit tests for chunking pipeline wiring and contract validation."""

import pytest
from jsonschema import Draft7Validator

from tests.fixtures.schemas import load_chunking_schema


class TestChunkingSchemaLoading:
    """Tests for chunking schema loading."""
    
    def test_load_chunking_schema_success(self):
        schema = load_chunking_schema()
        assert schema is not None
        assert schema.get("$schema") == "http://json-schema.org/draft-07/schema#"
        assert schema.get("title") == "ChunkedText"
    
    def test_schema_has_required_fields(self):
        schema = load_chunking_schema()
        required = schema.get("required", [])
        assert "schema_version" in required
        assert "patient_id" in required
        assert "chunks" in required


class TestChunkedTextContractValidation:
    """Tests for chunked text contract validation."""
    
    def test_valid_minimal_payload(self):
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "chunks": []
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) == 0
    
    def test_valid_full_payload(self):
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "chunking_timestamp": "2026-01-16T21:00:00Z",
            "chunking_config": {
                "chunk_size_target_tokens": 1000,
                "chunk_size_min_tokens": 500,
                "chunk_overlap_tokens": 100
            },
            "source_documents": ["doc-001", "doc-002"],
            "chunks": [
                {
                    "chunk_index": 0,
                    "text": "This is the first chunk of text.",
                    "token_count": 8,
                    "chunk_hash": "abc123def456",
                    "provenance": [
                        {
                            "document_id": "doc-001",
                            "page": 1,
                            "section": "Introduction",
                            "start_offset": 0,
                            "end_offset": 32
                        }
                    ]
                }
            ]
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) == 0
    
    def test_invalid_missing_patient_id(self):
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "chunks": []
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) > 0
        error_messages = [e.message for e in errors]
        assert any("patient_id" in msg for msg in error_messages)
    
    def test_invalid_patient_id_format(self):
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "not-a-uuid",
            "chunks": []
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) > 0
    
    def test_invalid_chunk_missing_provenance(self):
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "chunks": [
                {
                    "chunk_index": 0,
                    "text": "Some text"
                }
            ]
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) > 0
    
    def test_multi_document_chunk_provenance(self):
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "chunks": [
                {
                    "chunk_index": 0,
                    "text": "Text spanning two documents.",
                    "provenance": [
                        {"document_id": "doc-001", "start_offset": 0, "end_offset": 14},
                        {"document_id": "doc-002", "start_offset": 15, "end_offset": 28}
                    ]
                }
            ]
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) == 0


class TestChunkingPipelineIntegration:
    """Tests for chunking pipeline integration."""
    
    def test_pipeline_produces_valid_output(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_chunking_pipeline
        
        merged_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "source_documents": ["doc-001"],
            "merged_segments": [
                {
                    "text": "This is sample text from the first document.",
                    "document_id": "doc-001",
                    "document_location": {"page": 1}
                }
            ]
        }
        
        result = run_chunking_pipeline(merged_payload)
        
        assert result["schema_version"] == "1.0"
        assert result["patient_id"] == "00000000-0000-0000-0000-000000000001"
        assert "chunks" in result
    
    def test_pipeline_output_is_schema_valid(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_chunking_pipeline
        
        merged_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "source_documents": ["doc-001", "doc-002"],
            "merged_segments": [
                {
                    "text": "Content from the first document with some details.",
                    "document_id": "doc-001",
                    "document_location": {"page": 1, "section": "Intro"}
                },
                {
                    "text": "Content from the second document with more information.",
                    "document_id": "doc-002",
                    "document_location": {"page": 1}
                }
            ]
        }
        
        result = run_chunking_pipeline(merged_payload)
        
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        errors = list(validator.iter_errors(result))
        assert len(errors) == 0
    
    def test_pipeline_determinism(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_chunking_pipeline
        
        merged_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "source_documents": ["doc-001"],
            "merged_segments": [
                {
                    "text": "Deterministic test content for chunking.",
                    "document_id": "doc-001"
                }
            ]
        }
        
        result1 = run_chunking_pipeline(merged_payload)
        result2 = run_chunking_pipeline(merged_payload)
        
        assert len(result1["chunks"]) == len(result2["chunks"])
        for c1, c2 in zip(result1["chunks"], result2["chunks"]):
            assert c1["chunk_index"] == c2["chunk_index"]
            assert c1["text"] == c2["text"]
            assert c1["chunk_hash"] == c2["chunk_hash"]
    
    def test_pipeline_short_document(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_chunking_pipeline
        
        merged_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "source_documents": ["doc-001"],
            "merged_segments": [
                {
                    "text": "Short.",
                    "document_id": "doc-001"
                }
            ]
        }
        
        result = run_chunking_pipeline(merged_payload)
        
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        errors = list(validator.iter_errors(result))
        assert len(errors) == 0
    
    def test_pipeline_empty_segments(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_chunking_pipeline
        
        merged_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "source_documents": [],
            "merged_segments": []
        }
        
        result = run_chunking_pipeline(merged_payload)
        
        assert result["chunks"] == []
        
        schema = load_chunking_schema()
        validator = Draft7Validator(schema)
        errors = list(validator.iter_errors(result))
        assert len(errors) == 0


class TestNoPhiLogging:
    """Tests to ensure no PHI is logged by default."""
    
    def test_chunked_output_structure_no_raw_text_in_error(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import validate_chunked_text
        
        invalid_payload = {
            "schema_version": "1.0",
            "chunks": []
        }
        
        try:
            validate_chunked_text(invalid_payload)
        except ValueError as e:
            error_message = str(e)
            assert "patient_id" in error_message
            assert "John Doe" not in error_message
            assert "SSN" not in error_message
