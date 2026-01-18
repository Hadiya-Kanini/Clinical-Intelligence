"""Unit tests for embedding generation with rate limiting, retry, and normalization."""

import math
import pytest
from unittest.mock import Mock, MagicMock, patch
from typing import List

from embeddings.rate_limiter import RateLimiter
from embeddings.gemini_embeddings_client import GeminiEmbeddingsClient
from embeddings.embedding_generation import (
    generate_embeddings,
    normalize_embedding,
    EmbeddingResultItem,
    EmbeddingBatchResult,
    TransientAPIError,
    PermanentAPIError,
)


class TestNormalizeEmbedding:
    """Tests for L2 normalization."""
    
    def test_normalize_unit_vector(self):
        embedding = [1.0, 0.0, 0.0]
        normalized = normalize_embedding(embedding)
        assert abs(normalized[0] - 1.0) < 1e-6
        assert abs(normalized[1]) < 1e-6
        assert abs(normalized[2]) < 1e-6
    
    def test_normalize_non_unit_vector(self):
        embedding = [3.0, 4.0]
        normalized = normalize_embedding(embedding)
        norm = math.sqrt(sum(x * x for x in normalized))
        assert abs(norm - 1.0) < 1e-6
    
    def test_normalize_768_dimensional(self):
        embedding = [0.1] * 768
        normalized = normalize_embedding(embedding)
        assert len(normalized) == 768
        norm = math.sqrt(sum(x * x for x in normalized))
        assert abs(norm - 1.0) < 1e-6
    
    def test_normalize_zero_vector(self):
        embedding = [0.0, 0.0, 0.0]
        normalized = normalize_embedding(embedding)
        assert normalized == [0.0, 0.0, 0.0]


class TestRateLimiter:
    """Tests for rate limiter."""
    
    def test_first_request_no_wait(self):
        wait_times = []
        
        def mock_sleeper(seconds):
            wait_times.append(seconds)
        
        limiter = RateLimiter(rpm_limit=15, sleeper=mock_sleeper)
        limiter.acquire()
        
        assert len(wait_times) == 0
    
    def test_second_request_waits(self):
        current_time = [0.0]
        wait_times = []
        
        def mock_clock():
            return current_time[0]
        
        def mock_sleeper(seconds):
            wait_times.append(seconds)
            current_time[0] += seconds
        
        limiter = RateLimiter(rpm_limit=15, clock=mock_clock, sleeper=mock_sleeper)
        
        limiter.acquire()
        current_time[0] += 1.0
        limiter.acquire()
        
        assert len(wait_times) == 1
        assert wait_times[0] > 0
    
    def test_respects_rpm_limit(self):
        current_time = [0.0]
        request_times = []
        
        def mock_clock():
            return current_time[0]
        
        def mock_sleeper(seconds):
            current_time[0] += seconds
        
        limiter = RateLimiter(rpm_limit=15, clock=mock_clock, sleeper=mock_sleeper)
        
        for _ in range(16):
            limiter.acquire()
            request_times.append(current_time[0])
        
        time_span = request_times[-1] - request_times[0]
        assert time_span >= 60.0
    
    def test_interval_calculation(self):
        limiter = RateLimiter(rpm_limit=15)
        assert abs(limiter.interval_seconds - 4.0) < 0.01
        
        limiter2 = RateLimiter(rpm_limit=60)
        assert abs(limiter2.interval_seconds - 1.0) < 0.01
    
    def test_reset(self):
        wait_times = []
        
        def mock_sleeper(seconds):
            wait_times.append(seconds)
        
        limiter = RateLimiter(rpm_limit=15, sleeper=mock_sleeper)
        limiter.acquire()
        limiter.reset()
        limiter.acquire()
        
        assert len(wait_times) == 0


class TestGeminiEmbeddingsClient:
    """Tests for Gemini embeddings client."""
    
    def test_client_properties(self):
        with patch('embeddings.gemini_embeddings_client.genai'):
            client = GeminiEmbeddingsClient(
                api_key="test-key",
                model="text-embedding-004",
                output_dimensions=768
            )
            assert client.model == "text-embedding-004"
            assert client.output_dimensions == 768


class MockGeminiClient:
    """Mock Gemini client for testing."""
    
    def __init__(self, embeddings: List[List[float]] = None, should_fail: bool = False, fail_type: str = "transient"):
        self._embeddings = embeddings or [[0.1] * 768]
        self._call_count = 0
        self._should_fail = should_fail
        self._fail_type = fail_type
        self.model = "text-embedding-004"
        self.output_dimensions = 768
    
    def embed_content(self, text: str) -> List[float]:
        if self._should_fail:
            if self._fail_type == "transient":
                raise Exception("429 Rate limit exceeded")
            else:
                raise Exception("Invalid API key")
        
        idx = min(self._call_count, len(self._embeddings) - 1)
        self._call_count += 1
        return self._embeddings[idx]


class TestGenerateEmbeddings:
    """Tests for embedding generation."""
    
    def _create_mock_rate_limiter(self):
        return RateLimiter(rpm_limit=1000, sleeper=lambda x: None)
    
    def test_success_generates_768_length_vectors(self):
        client = MockGeminiClient(embeddings=[[0.5] * 768])
        rate_limiter = self._create_mock_rate_limiter()
        
        chunks = [
            {"chunk_index": 0, "text": "Test text", "provenance": [{"document_id": "doc-001"}]}
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert len(result.results) == 1
        assert result.results[0].status == "success"
        assert len(result.results[0].embedding) == 768
    
    def test_normalization_applied(self):
        raw_embedding = [3.0, 4.0] + [0.0] * 766
        client = MockGeminiClient(embeddings=[raw_embedding])
        rate_limiter = self._create_mock_rate_limiter()
        
        chunks = [
            {"chunk_index": 0, "text": "Test", "provenance": [{"document_id": "doc-001"}]}
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert result.results[0].normalized is True
        embedding = result.results[0].embedding
        norm = math.sqrt(sum(x * x for x in embedding))
        assert abs(norm - 1.0) < 1e-6
    
    def test_metadata_preserved(self):
        client = MockGeminiClient()
        rate_limiter = self._create_mock_rate_limiter()
        
        chunks = [
            {
                "chunk_index": 5,
                "text": "Test text",
                "token_count": 10,
                "chunk_hash": "abc123",
                "provenance": [
                    {"document_id": "doc-001", "page": 1, "section": "Intro"}
                ]
            }
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter,
            source_documents=["doc-001"]
        )
        
        item = result.results[0]
        assert item.chunk_index == 5
        assert item.token_count == 10
        assert item.chunk_hash == "abc123"
        assert item.document_id == "doc-001"
        assert len(item.provenance) == 1
        assert item.provenance[0].page == 1
    
    def test_batch_processing(self):
        client = MockGeminiClient(embeddings=[[0.1] * 768, [0.2] * 768, [0.3] * 768])
        rate_limiter = self._create_mock_rate_limiter()
        
        chunks = [
            {"chunk_index": i, "text": f"Text {i}", "provenance": [{"document_id": "doc-001"}]}
            for i in range(3)
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert len(result.results) == 3
        for i, item in enumerate(result.results):
            assert item.chunk_index == i
            assert item.status == "success"
    
    def test_embedding_config_included(self):
        client = MockGeminiClient()
        rate_limiter = self._create_mock_rate_limiter()
        
        chunks = [
            {"chunk_index": 0, "text": "Test", "provenance": [{"document_id": "doc-001"}]}
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert result.embedding_config is not None
        assert result.embedding_config["embedding_model"] == "text-embedding-004"
        assert result.embedding_config["embedding_dimensions"] == 768
    
    def test_to_dict_structure(self):
        client = MockGeminiClient()
        rate_limiter = self._create_mock_rate_limiter()
        
        chunks = [
            {"chunk_index": 0, "text": "Test", "provenance": [{"document_id": "doc-001"}]}
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        result_dict = result.to_dict()
        assert "schema_version" in result_dict
        assert "patient_id" in result_dict
        assert "results" in result_dict
        assert result_dict["schema_version"] == "1.0"


class TestRateLimitingBehavior:
    """Tests for rate limiting during embedding generation."""
    
    def test_rate_limiter_called_for_each_chunk(self):
        acquire_count = [0]
        
        class CountingRateLimiter(RateLimiter):
            def acquire(self):
                acquire_count[0] += 1
        
        client = MockGeminiClient()
        rate_limiter = CountingRateLimiter(rpm_limit=1000, sleeper=lambda x: None)
        
        chunks = [
            {"chunk_index": i, "text": f"Text {i}", "provenance": [{"document_id": "doc-001"}]}
            for i in range(5)
        ]
        
        generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert acquire_count[0] == 5


class TestRetryBehavior:
    """Tests for retry behavior on transient errors."""
    
    def test_permanent_error_no_retry(self):
        call_count = [0]
        
        class FailingClient:
            model = "text-embedding-004"
            output_dimensions = 768
            
            def embed_content(self, text):
                call_count[0] += 1
                raise Exception("Invalid API key - permanent error")
        
        client = FailingClient()
        rate_limiter = RateLimiter(rpm_limit=1000, sleeper=lambda x: None)
        
        chunks = [
            {"chunk_index": 0, "text": "Test", "provenance": [{"document_id": "doc-001"}]}
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter,
            max_retries=3
        )
        
        assert result.results[0].status == "failed"
        assert call_count[0] == 1
    
    def test_failed_result_has_error_info(self):
        class FailingClient:
            model = "text-embedding-004"
            output_dimensions = 768
            
            def embed_content(self, text):
                raise Exception("API error occurred")
        
        client = FailingClient()
        rate_limiter = RateLimiter(rpm_limit=1000, sleeper=lambda x: None)
        
        chunks = [
            {"chunk_index": 0, "text": "Test", "provenance": [{"document_id": "doc-001"}]}
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert result.results[0].status == "failed"
        assert result.results[0].error_code is not None
        assert result.results[0].error_message is not None


class TestPartialFailures:
    """Tests for handling partial failures in batch."""
    
    def test_mixed_success_and_failure(self):
        call_count = [0]
        
        class MixedClient:
            model = "text-embedding-004"
            output_dimensions = 768
            
            def embed_content(self, text):
                call_count[0] += 1
                if call_count[0] == 2:
                    raise Exception("Temporary failure")
                return [0.1] * 768
        
        client = MixedClient()
        rate_limiter = RateLimiter(rpm_limit=1000, sleeper=lambda x: None)
        
        chunks = [
            {"chunk_index": i, "text": f"Text {i}", "provenance": [{"document_id": "doc-001"}]}
            for i in range(3)
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert len(result.results) == 3
        assert result.results[0].status == "success"
        assert result.results[1].status == "failed"
        assert result.results[2].status == "success"


class TestEdgeCases:
    """Tests for edge cases."""
    
    def test_empty_chunks_list(self):
        client = MockGeminiClient()
        rate_limiter = RateLimiter(rpm_limit=1000, sleeper=lambda x: None)
        
        result = generate_embeddings(
            chunks=[],
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert result.results == []
        assert result.schema_version == "1.0"
    
    def test_single_chunk(self):
        client = MockGeminiClient()
        rate_limiter = RateLimiter(rpm_limit=1000, sleeper=lambda x: None)
        
        chunks = [
            {"chunk_index": 0, "text": "Single chunk", "provenance": [{"document_id": "doc-001"}]}
        ]
        
        result = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter
        )
        
        assert len(result.results) == 1
        assert result.results[0].status == "success"
    
    def test_include_text_content_option(self):
        client = MockGeminiClient()
        rate_limiter = RateLimiter(rpm_limit=1000, sleeper=lambda x: None)
        
        chunks = [
            {"chunk_index": 0, "text": "Test content", "provenance": [{"document_id": "doc-001"}]}
        ]
        
        result_without = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter,
            include_text_content=False
        )
        
        result_with = generate_embeddings(
            chunks=chunks,
            patient_id="00000000-0000-0000-0000-000000000001",
            client=client,
            rate_limiter=rate_limiter,
            include_text_content=True
        )
        
        assert result_without.results[0].text_content is None
        assert result_with.results[0].text_content == "Test content"
