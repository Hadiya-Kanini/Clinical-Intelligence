"""
Unit tests for Gemini entity extraction client.

Tests single-call behavior, retry logic, and safe error handling.
"""

import pytest
from unittest.mock import MagicMock, patch, PropertyMock

from worker.entity_extraction.gemini_client import (
    GeminiClient,
    GeminiClientError,
    GeminiRateLimitError,
    GeminiTimeoutError,
)
from worker.entity_extraction.extractor import (
    extract_entities_single_call,
    create_extraction_input,
)
from worker.entity_extraction.models import ChunkWithProvenance, ExtractionInput


class TestGeminiClient:
    """Tests for GeminiClient class."""

    def test_init_requires_api_key(self):
        """Test that initialization requires an API key."""
        with pytest.raises(ValueError, match="GEMINI_API_KEY is required"):
            GeminiClient("")
        
        with pytest.raises(ValueError, match="GEMINI_API_KEY is required"):
            GeminiClient(None)
        
        with pytest.raises(ValueError, match="GEMINI_API_KEY is required"):
            GeminiClient("   ")

    @patch("worker.entity_extraction.gemini_client.genai")
    def test_init_with_valid_api_key(self, mock_genai):
        """Test initialization with valid API key."""
        client = GeminiClient("test-api-key")
        
        assert client._api_key == "test-api-key"
        assert client._model_name == "gemini-2.5-flash"
        assert client._timeout == 60
        assert client._max_retries == 3

    @patch("worker.entity_extraction.gemini_client.genai")
    def test_init_with_custom_settings(self, mock_genai):
        """Test initialization with custom settings."""
        client = GeminiClient(
            api_key="test-api-key",
            model="custom-model",
            timeout=120,
            max_retries=5
        )
        
        assert client._model_name == "custom-model"
        assert client._timeout == 120
        assert client._max_retries == 5

    @patch("worker.entity_extraction.gemini_client.genai")
    def test_generate_content_requires_prompt(self, mock_genai):
        """Test that generate_content requires a prompt."""
        client = GeminiClient("test-api-key")
        
        with pytest.raises(ValueError, match="Prompt is required"):
            client.generate_content("")
        
        with pytest.raises(ValueError, match="Prompt is required"):
            client.generate_content(None)

    @patch("worker.entity_extraction.gemini_client.genai")
    def test_model_name_property(self, mock_genai):
        """Test model_name property."""
        client = GeminiClient("test-api-key", model="test-model")
        
        assert client.model_name == "test-model"

    @patch("worker.entity_extraction.gemini_client.genai")
    def test_max_retries_property(self, mock_genai):
        """Test max_retries property."""
        client = GeminiClient("test-api-key", max_retries=5)
        
        assert client.max_retries == 5


class TestExtractEntitiesSingleCall:
    """Tests for extract_entities_single_call function."""

    def test_requires_extraction_input(self):
        """Test that extraction_input is required."""
        mock_client = MagicMock()
        
        with pytest.raises(ValueError, match="extraction_input is required"):
            extract_entities_single_call(None, mock_client)

    def test_requires_document_id(self):
        """Test that document_id is required."""
        mock_client = MagicMock()
        extraction_input = ExtractionInput(
            document_id="",
            chunks=[ChunkWithProvenance(text="test", document_id="doc-1")]
        )
        
        with pytest.raises(ValueError, match="document_id is required"):
            extract_entities_single_call(extraction_input, mock_client)

    def test_requires_chunks(self):
        """Test that at least one chunk is required."""
        mock_client = MagicMock()
        extraction_input = ExtractionInput(
            document_id="doc-123",
            chunks=[]
        )
        
        with pytest.raises(ValueError, match="At least one chunk is required"):
            extract_entities_single_call(extraction_input, mock_client)

    def test_requires_gemini_client(self):
        """Test that gemini_client is required."""
        extraction_input = ExtractionInput(
            document_id="doc-123",
            chunks=[ChunkWithProvenance(text="test", document_id="doc-123")]
        )
        
        with pytest.raises(ValueError, match="gemini_client is required"):
            extract_entities_single_call(extraction_input, None)

    def test_makes_exactly_one_api_call(self):
        """Test that exactly one API call is made per extraction."""
        mock_client = MagicMock()
        mock_client.generate_content.return_value = '{"schema_version": "1.0"}'
        
        extraction_input = ExtractionInput(
            document_id="doc-123",
            chunks=[
                ChunkWithProvenance(text="chunk 1", document_id="doc-123"),
                ChunkWithProvenance(text="chunk 2", document_id="doc-123"),
                ChunkWithProvenance(text="chunk 3", document_id="doc-123"),
            ]
        )
        
        extract_entities_single_call(extraction_input, mock_client)
        
        assert mock_client.generate_content.call_count == 1

    def test_returns_raw_response(self):
        """Test that raw response is returned."""
        mock_client = MagicMock()
        expected_response = '{"schema_version": "1.0", "extracted_entities": []}'
        mock_client.generate_content.return_value = expected_response
        
        extraction_input = ExtractionInput(
            document_id="doc-123",
            chunks=[ChunkWithProvenance(text="test", document_id="doc-123")]
        )
        
        result = extract_entities_single_call(extraction_input, mock_client)
        
        assert result == expected_response


class TestCreateExtractionInput:
    """Tests for create_extraction_input function."""

    def test_creates_extraction_input(self):
        """Test creating an ExtractionInput."""
        chunks = [
            ChunkWithProvenance(text="chunk 1", document_id="doc-123"),
            ChunkWithProvenance(text="chunk 2", document_id="doc-123"),
        ]
        
        result = create_extraction_input(
            document_id="doc-123",
            chunks=chunks,
            patient_id="patient-456"
        )
        
        assert result.document_id == "doc-123"
        assert len(result.chunks) == 2
        assert result.patient_id == "patient-456"

    def test_creates_extraction_input_without_patient_id(self):
        """Test creating an ExtractionInput without patient_id."""
        chunks = [ChunkWithProvenance(text="chunk 1", document_id="doc-123")]
        
        result = create_extraction_input(
            document_id="doc-123",
            chunks=chunks
        )
        
        assert result.document_id == "doc-123"
        assert result.patient_id is None


class TestGeminiClientErrors:
    """Tests for Gemini client error classes."""

    def test_gemini_client_error(self):
        """Test GeminiClientError."""
        error = GeminiClientError("Test error")
        assert str(error) == "Test error"

    def test_gemini_rate_limit_error(self):
        """Test GeminiRateLimitError is subclass of GeminiClientError."""
        error = GeminiRateLimitError("Rate limited")
        assert isinstance(error, GeminiClientError)

    def test_gemini_timeout_error(self):
        """Test GeminiTimeoutError is subclass of GeminiClientError."""
        error = GeminiTimeoutError("Timeout")
        assert isinstance(error, GeminiClientError)
