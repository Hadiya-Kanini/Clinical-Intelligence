"""Unit tests for embedding pipeline wiring and contract validation."""

import pytest
from jsonschema import Draft7Validator
from typing import List

from tests.fixtures.schemas import load_embeddings_schema


class MockGeminiClient:
    """Mock Gemini client for testing."""
    
    def __init__(self, embeddings: List[List[float]] = None):
        self._embeddings = embeddings or [[0.1] * 768]
        self._call_count = 0
        self.model = "text-embedding-004"
        self.output_dimensions = 768
    
    def embed_content(self, text: str) -> List[float]:
        idx = min(self._call_count, len(self._embeddings) - 1)
        self._call_count += 1
        return self._embeddings[idx]


class MockRateLimiter:
    """Mock rate limiter for testing."""
    
    def __init__(self):
        self.acquire_count = 0
    
    def acquire(self):
        self.acquire_count += 1
    
    def reset(self):
        self.acquire_count = 0


class TestEmbeddingsSchemaLoading:
    """Tests for embeddings schema loading."""
    
    def test_load_embeddings_schema_success(self):
        schema = load_embeddings_schema()
        assert schema is not None
        assert schema.get("$schema") == "http://json-schema.org/draft-07/schema#"
        assert schema.get("title") == "EmbeddingResult"
    
    def test_schema_has_required_fields(self):
        schema = load_embeddings_schema()
        required = schema.get("required", [])
        assert "schema_version" in required
        assert "patient_id" in required
        assert "results" in required


class TestEmbeddingResultContractValidation:
    """Tests for embedding result contract validation."""
    
    def test_valid_minimal_payload(self):
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "results": []
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) == 0
    
    def test_valid_success_result(self):
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "results": [
                {
                    "chunk_index": 0,
                    "status": "success",
                    "embedding": [0.1] * 768,
                    "normalized": True,
                    "embedding_model": "text-embedding-004",
                    "embedding_dimensions": 768,
                    "document_id": "doc-001"
                }
            ]
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) == 0
    
    def test_valid_failed_result(self):
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "results": [
                {
                    "chunk_index": 0,
                    "status": "failed",
                    "error_code": "RATE_LIMIT_EXCEEDED",
                    "error_message": "API rate limit exceeded"
                }
            ]
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) == 0
    
    def test_valid_mixed_results(self):
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "results": [
                {
                    "chunk_index": 0,
                    "status": "success",
                    "embedding": [0.1] * 768,
                    "normalized": True
                },
                {
                    "chunk_index": 1,
                    "status": "failed",
                    "error_code": "API_ERROR",
                    "error_message": "Temporary failure"
                }
            ]
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) == 0
    
    def test_invalid_missing_patient_id(self):
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "results": []
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) > 0
    
    def test_invalid_embedding_wrong_length(self):
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        
        payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "results": [
                {
                    "chunk_index": 0,
                    "status": "success",
                    "embedding": [0.1] * 100,
                    "normalized": True
                }
            ]
        }
        
        errors = list(validator.iter_errors(payload))
        assert len(errors) > 0


class TestEmbeddingPipelineIntegration:
    """Tests for embedding pipeline integration."""
    
    def test_pipeline_produces_valid_output(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_embedding_pipeline
        
        chunked_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "source_documents": ["doc-001"],
            "chunks": [
                {
                    "chunk_index": 0,
                    "text": "Test chunk content.",
                    "token_count": 5,
                    "provenance": [{"document_id": "doc-001", "page": 1}]
                }
            ]
        }
        
        mock_client = MockGeminiClient()
        mock_limiter = MockRateLimiter()
        
        result = run_embedding_pipeline(
            chunked_payload,
            gemini_client=mock_client,
            rate_limiter=mock_limiter
        )
        
        assert result["schema_version"] == "1.0"
        assert result["patient_id"] == "00000000-0000-0000-0000-000000000001"
        assert "results" in result
    
    def test_pipeline_output_is_schema_valid(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_embedding_pipeline
        
        chunked_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "source_documents": ["doc-001"],
            "chunks": [
                {
                    "chunk_index": 0,
                    "text": "First chunk.",
                    "token_count": 3,
                    "provenance": [{"document_id": "doc-001"}]
                },
                {
                    "chunk_index": 1,
                    "text": "Second chunk.",
                    "token_count": 3,
                    "provenance": [{"document_id": "doc-001"}]
                }
            ]
        }
        
        mock_client = MockGeminiClient(embeddings=[[0.2] * 768, [0.3] * 768])
        mock_limiter = MockRateLimiter()
        
        result = run_embedding_pipeline(
            chunked_payload,
            gemini_client=mock_client,
            rate_limiter=mock_limiter
        )
        
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        errors = list(validator.iter_errors(result))
        assert len(errors) == 0
    
    def test_pipeline_preserves_metadata(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_embedding_pipeline
        
        chunked_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "source_documents": ["doc-001"],
            "chunks": [
                {
                    "chunk_index": 5,
                    "text": "Test content.",
                    "token_count": 10,
                    "chunk_hash": "abc123",
                    "provenance": [
                        {"document_id": "doc-001", "page": 2, "section": "Methods"}
                    ]
                }
            ]
        }
        
        mock_client = MockGeminiClient()
        mock_limiter = MockRateLimiter()
        
        result = run_embedding_pipeline(
            chunked_payload,
            gemini_client=mock_client,
            rate_limiter=mock_limiter
        )
        
        item = result["results"][0]
        assert item["chunk_index"] == 5
        assert item["token_count"] == 10
        assert item["chunk_hash"] == "abc123"
        assert item["document_id"] == "doc-001"
    
    def test_pipeline_determinism(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_embedding_pipeline
        
        chunked_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "chunks": [
                {
                    "chunk_index": 0,
                    "text": "Deterministic test.",
                    "provenance": [{"document_id": "doc-001"}]
                }
            ]
        }
        
        mock_client1 = MockGeminiClient(embeddings=[[0.5] * 768])
        mock_client2 = MockGeminiClient(embeddings=[[0.5] * 768])
        mock_limiter = MockRateLimiter()
        
        result1 = run_embedding_pipeline(
            chunked_payload,
            gemini_client=mock_client1,
            rate_limiter=mock_limiter
        )
        result2 = run_embedding_pipeline(
            chunked_payload,
            gemini_client=mock_client2,
            rate_limiter=mock_limiter
        )
        
        assert len(result1["results"]) == len(result2["results"])
        for r1, r2 in zip(result1["results"], result2["results"]):
            assert r1["chunk_index"] == r2["chunk_index"]
            assert r1["status"] == r2["status"]
            if r1["status"] == "success":
                assert r1["embedding"] == r2["embedding"]
    
    def test_pipeline_empty_chunks(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_embedding_pipeline
        
        chunked_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "chunks": []
        }
        
        mock_client = MockGeminiClient()
        mock_limiter = MockRateLimiter()
        
        result = run_embedding_pipeline(
            chunked_payload,
            gemini_client=mock_client,
            rate_limiter=mock_limiter
        )
        
        assert result["results"] == []
        
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        errors = list(validator.iter_errors(result))
        assert len(errors) == 0
    
    def test_pipeline_single_chunk(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_embedding_pipeline
        
        chunked_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "chunks": [
                {
                    "chunk_index": 0,
                    "text": "Single chunk.",
                    "provenance": [{"document_id": "doc-001"}]
                }
            ]
        }
        
        mock_client = MockGeminiClient()
        mock_limiter = MockRateLimiter()
        
        result = run_embedding_pipeline(
            chunked_payload,
            gemini_client=mock_client,
            rate_limiter=mock_limiter
        )
        
        assert len(result["results"]) == 1
        assert result["results"][0]["status"] == "success"
        
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        errors = list(validator.iter_errors(result))
        assert len(errors) == 0


class TestPartialFailureHandling:
    """Tests for partial failure handling in embedding pipeline."""
    
    def test_partial_failure_still_valid(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import run_embedding_pipeline
        
        call_count = [0]
        
        class PartialFailClient:
            model = "text-embedding-004"
            output_dimensions = 768
            
            def embed_content(self, text):
                call_count[0] += 1
                if call_count[0] == 2:
                    raise Exception("Simulated failure")
                return [0.1] * 768
        
        chunked_payload = {
            "schema_version": "1.0",
            "patient_id": "00000000-0000-0000-0000-000000000001",
            "chunks": [
                {"chunk_index": 0, "text": "Chunk 0", "provenance": [{"document_id": "doc-001"}]},
                {"chunk_index": 1, "text": "Chunk 1", "provenance": [{"document_id": "doc-001"}]},
                {"chunk_index": 2, "text": "Chunk 2", "provenance": [{"document_id": "doc-001"}]}
            ]
        }
        
        mock_client = PartialFailClient()
        mock_limiter = MockRateLimiter()
        
        result = run_embedding_pipeline(
            chunked_payload,
            gemini_client=mock_client,
            rate_limiter=mock_limiter
        )
        
        assert len(result["results"]) == 3
        assert result["results"][0]["status"] == "success"
        assert result["results"][1]["status"] == "failed"
        assert result["results"][2]["status"] == "success"
        
        schema = load_embeddings_schema()
        validator = Draft7Validator(schema)
        errors = list(validator.iter_errors(result))
        assert len(errors) == 0


class TestNoPhiLogging:
    """Tests to ensure no PHI is logged by default."""
    
    def test_embedding_output_structure_no_raw_text_in_error(self):
        import sys
        import os
        sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
        
        from main import validate_embedding_result
        
        invalid_payload = {
            "schema_version": "1.0",
            "results": []
        }
        
        try:
            validate_embedding_result(invalid_payload)
        except ValueError as e:
            error_message = str(e)
            assert "patient_id" in error_message
            assert "John Doe" not in error_message
            assert "SSN" not in error_message
