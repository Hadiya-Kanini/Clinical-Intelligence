"""
Unit tests for document chunk retriever.

Tests K clamping, embedding validation, and deterministic ordering.
"""

import pytest
from unittest.mock import MagicMock, patch

from worker.retrieval.document_chunk_retriever import (
    DocumentChunkRetriever,
    RetrievedChunk,
    MIN_K,
    MAX_K,
    DEFAULT_K,
    EMBEDDING_DIMENSIONS,
)


class TestRetrievedChunk:
    """Tests for RetrievedChunk dataclass."""

    def test_retrieved_chunk_creation(self):
        """Test creating a RetrievedChunk."""
        chunk = RetrievedChunk(
            chunk_id="chunk-123",
            document_id="doc-456",
            text_content="Sample text",
            page=1,
            section="Introduction",
            coordinates="10,20",
            score=0.85,
            rank=1
        )
        
        assert chunk.chunk_id == "chunk-123"
        assert chunk.document_id == "doc-456"
        assert chunk.text_content == "Sample text"
        assert chunk.page == 1
        assert chunk.section == "Introduction"
        assert chunk.score == 0.85
        assert chunk.rank == 1


class TestDocumentChunkRetriever:
    """Tests for DocumentChunkRetriever class."""

    def test_init_requires_connection_string(self):
        """Test that initialization requires a connection string."""
        with pytest.raises(ValueError, match="DATABASE_CONNECTION_STRING is required"):
            DocumentChunkRetriever("")
        
        with pytest.raises(ValueError, match="DATABASE_CONNECTION_STRING is required"):
            DocumentChunkRetriever(None)

    def test_init_with_valid_connection_string(self):
        """Test initialization with valid connection string."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        assert retriever._connection_string == "postgresql://localhost/test"

    def test_clamp_k_below_minimum(self):
        """Test K clamping when below minimum."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        
        assert retriever._clamp_k(5) == MIN_K
        assert retriever._clamp_k(1) == MIN_K
        assert retriever._clamp_k(0) == MIN_K

    def test_clamp_k_above_maximum(self):
        """Test K clamping when above maximum."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        
        assert retriever._clamp_k(20) == MAX_K
        assert retriever._clamp_k(100) == MAX_K

    def test_clamp_k_within_range(self):
        """Test K clamping when within valid range."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        
        assert retriever._clamp_k(10) == 10
        assert retriever._clamp_k(12) == 12
        assert retriever._clamp_k(15) == 15

    def test_validate_embedding_empty(self):
        """Test validation fails for empty embedding."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        
        with pytest.raises(ValueError, match="Query embedding is required"):
            retriever._validate_embedding([])
        
        with pytest.raises(ValueError, match="Query embedding is required"):
            retriever._validate_embedding(None)

    def test_validate_embedding_wrong_dimensions(self):
        """Test validation fails for wrong dimensions."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        
        with pytest.raises(ValueError, match=f"must be {EMBEDDING_DIMENSIONS} dimensions"):
            retriever._validate_embedding([0.1] * 512)
        
        with pytest.raises(ValueError, match=f"must be {EMBEDDING_DIMENSIONS} dimensions"):
            retriever._validate_embedding([0.1] * 1024)

    def test_validate_embedding_non_numeric(self):
        """Test validation fails for non-numeric values."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        
        embedding = [0.1] * 767 + ["not a number"]
        
        with pytest.raises(ValueError, match="not numeric"):
            retriever._validate_embedding(embedding)

    def test_validate_embedding_valid(self):
        """Test validation passes for valid embedding."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        
        retriever._validate_embedding([0.1] * EMBEDDING_DIMENSIONS)

    def test_format_embedding(self):
        """Test embedding formatting for PostgreSQL."""
        retriever = DocumentChunkRetriever("postgresql://localhost/test")
        
        result = retriever._format_embedding([0.1, 0.2, 0.3])
        
        assert result == "[0.1,0.2,0.3]"


class TestKClampingConstants:
    """Tests for K clamping constants."""

    def test_min_k_value(self):
        """Test MIN_K is 10."""
        assert MIN_K == 10

    def test_max_k_value(self):
        """Test MAX_K is 15."""
        assert MAX_K == 15

    def test_default_k_value(self):
        """Test DEFAULT_K is 15."""
        assert DEFAULT_K == 15

    def test_embedding_dimensions(self):
        """Test EMBEDDING_DIMENSIONS is 768."""
        assert EMBEDDING_DIMENSIONS == 768
