"""
Unit tests for document chunk store.

Tests dedupe behavior, validation, and error handling.
"""

import pytest
from unittest.mock import MagicMock, patch

from worker.storage.document_chunk_store import DocumentChunkStore, ChunkRecord


class TestChunkRecord:
    """Tests for ChunkRecord dataclass."""

    def test_chunk_record_creation_with_required_fields(self):
        """Test creating a ChunkRecord with required fields."""
        record = ChunkRecord(
            document_id="doc-123",
            text_content="Sample text",
            embedding=[0.1] * 768,
            chunk_hash="abc123"
        )
        
        assert record.document_id == "doc-123"
        assert record.text_content == "Sample text"
        assert len(record.embedding) == 768
        assert record.chunk_hash == "abc123"
        assert record.page is None
        assert record.section is None

    def test_chunk_record_creation_with_all_fields(self):
        """Test creating a ChunkRecord with all fields."""
        record = ChunkRecord(
            document_id="doc-123",
            text_content="Sample text",
            embedding=[0.1] * 768,
            chunk_hash="abc123",
            page=1,
            section="Introduction",
            coordinates="10,20,100,50",
            token_count=50,
            chunk_id="chunk-456"
        )
        
        assert record.page == 1
        assert record.section == "Introduction"
        assert record.coordinates == "10,20,100,50"
        assert record.token_count == 50
        assert record.chunk_id == "chunk-456"


class TestDocumentChunkStore:
    """Tests for DocumentChunkStore class."""

    def test_init_requires_connection_string(self):
        """Test that initialization requires a connection string."""
        with pytest.raises(ValueError, match="DATABASE_CONNECTION_STRING is required"):
            DocumentChunkStore("")
        
        with pytest.raises(ValueError, match="DATABASE_CONNECTION_STRING is required"):
            DocumentChunkStore(None)
        
        with pytest.raises(ValueError, match="DATABASE_CONNECTION_STRING is required"):
            DocumentChunkStore("   ")

    def test_init_with_valid_connection_string(self):
        """Test initialization with valid connection string."""
        store = DocumentChunkStore("postgresql://localhost/test")
        assert store._connection_string == "postgresql://localhost/test"

    def test_validate_chunk_missing_document_id(self):
        """Test validation fails for missing document_id."""
        store = DocumentChunkStore("postgresql://localhost/test")
        chunk = ChunkRecord(
            document_id="",
            text_content="text",
            embedding=[0.1] * 768,
            chunk_hash="hash"
        )
        
        with pytest.raises(ValueError, match="document_id is required"):
            store._validate_chunk(chunk)

    def test_validate_chunk_missing_text_content(self):
        """Test validation fails for missing text_content."""
        store = DocumentChunkStore("postgresql://localhost/test")
        chunk = ChunkRecord(
            document_id="doc-123",
            text_content="",
            embedding=[0.1] * 768,
            chunk_hash="hash"
        )
        
        with pytest.raises(ValueError, match="text_content is required"):
            store._validate_chunk(chunk)

    def test_validate_chunk_missing_chunk_hash(self):
        """Test validation fails for missing chunk_hash."""
        store = DocumentChunkStore("postgresql://localhost/test")
        chunk = ChunkRecord(
            document_id="doc-123",
            text_content="text",
            embedding=[0.1] * 768,
            chunk_hash=""
        )
        
        with pytest.raises(ValueError, match="chunk_hash is required"):
            store._validate_chunk(chunk)

    def test_validate_chunk_missing_embedding(self):
        """Test validation fails for missing embedding."""
        store = DocumentChunkStore("postgresql://localhost/test")
        chunk = ChunkRecord(
            document_id="doc-123",
            text_content="text",
            embedding=[],
            chunk_hash="hash"
        )
        
        with pytest.raises(ValueError, match="embedding is required"):
            store._validate_chunk(chunk)

    def test_validate_chunk_wrong_embedding_dimensions(self):
        """Test validation fails for wrong embedding dimensions."""
        store = DocumentChunkStore("postgresql://localhost/test")
        chunk = ChunkRecord(
            document_id="doc-123",
            text_content="text",
            embedding=[0.1] * 512,
            chunk_hash="hash"
        )
        
        with pytest.raises(ValueError, match="Embedding must be 768 dimensions"):
            store._validate_chunk(chunk)

    def test_format_embedding(self):
        """Test embedding formatting for PostgreSQL."""
        store = DocumentChunkStore("postgresql://localhost/test")
        embedding = [0.1, 0.2, 0.3]
        
        result = store._format_embedding(embedding)
        
        assert result == "[0.1,0.2,0.3]"

    def test_persist_chunks_empty_list(self):
        """Test persisting empty list returns 0."""
        store = DocumentChunkStore("postgresql://localhost/test")
        
        result = store.persist_chunks([])
        
        assert result == 0
