"""
Unit tests for entity extraction response parser and validator tests.

Tests cover:
- JSON parsing from various LLM output formats
- Schema validation
- Conflict detection
- Error handling for malformed responses
- Normalization of placeholder values and non-standard dates
"""

import pytest

from worker.entity_extraction.response_parser import (
    parse_entity_extraction_response,
    validate_entity_response,
    parse_and_validate_response,
    validate_conflicts,
    extract_entity_count,
    extract_conflict_count,
    MalformedResponseError,
    SchemaValidationError,
    EntityExtractionError,
)


class TestParseEntityExtractionResponse:
    """Tests for parse_entity_extraction_response function."""

    def test_parse_valid_json(self):
        """Test parsing valid JSON response."""
        raw_text = '{"schema_version": "1.0", "document_id": "doc-123", "extracted_entities": []}'
        
        result = parse_entity_extraction_response(raw_text)
        
        assert result["schema_version"] == "1.0"
        assert result["document_id"] == "doc-123"
        assert result["extracted_entities"] == []

    def test_parse_json_with_markdown_code_block(self):
        """Test parsing JSON wrapped in markdown code block."""
        raw_text = '''```json
{"schema_version": "1.0", "document_id": "doc-123", "extracted_entities": []}
```'''
        
        result = parse_entity_extraction_response(raw_text)
        
        assert result["schema_version"] == "1.0"

    def test_parse_json_with_generic_code_block(self):
        """Test parsing JSON wrapped in generic code block."""
        raw_text = '''```
{"schema_version": "1.0", "document_id": "doc-123", "extracted_entities": []}
```'''
        
        result = parse_entity_extraction_response(raw_text)
        
        assert result["schema_version"] == "1.0"

    def test_parse_json_with_leading_text(self):
        """Test parsing JSON with leading text."""
        raw_text = '''Here is the extracted data:
{"schema_version": "1.0", "document_id": "doc-123", "extracted_entities": []}'''
        
        result = parse_entity_extraction_response(raw_text)
        
        assert result["schema_version"] == "1.0"

    def test_parse_json_with_trailing_text(self):
        """Test parsing JSON with trailing text."""
        raw_text = '''{"schema_version": "1.0", "document_id": "doc-123", "extracted_entities": []}
That's the extracted data.'''
        
        result = parse_entity_extraction_response(raw_text)
        
        assert result["schema_version"] == "1.0"

    def test_parse_empty_response_raises_error(self):
        """Test that empty response raises MalformedResponseError."""
        with pytest.raises(MalformedResponseError, match="Empty response"):
            parse_entity_extraction_response("")
        
        with pytest.raises(MalformedResponseError, match="Empty response"):
            parse_entity_extraction_response("   ")
        
        with pytest.raises(MalformedResponseError, match="Empty response"):
            parse_entity_extraction_response(None)

    def test_parse_invalid_json_raises_error(self):
        """Test that invalid JSON raises MalformedResponseError."""
        with pytest.raises(MalformedResponseError, match="Could not extract valid JSON from response"):
            parse_entity_extraction_response("This is not JSON at all")

    def test_parse_truncated_json_raises_error(self):
        """Test that truncated JSON raises MalformedResponseError."""
        with pytest.raises(MalformedResponseError, match="Could not extract valid JSON from response"):
            parse_entity_extraction_response('{"schema_version": "1.0", "document_id":')


class TestValidateEntityResponse:
    """Tests for validate_entity_response function."""

    def test_validate_valid_response(self):
        """Test validating a valid response."""
        response = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": []
        }
        
        def mock_validate(payload):
            pass
        
        result = validate_entity_response(response, mock_validate)
        
        assert result == response

    def test_validate_invalid_response_raises_error(self):
        """Test that invalid response raises SchemaValidationError."""
        response = {"invalid": "data"}
        
        def mock_validate(payload):
            raise ValueError("Missing required field")
        
        with pytest.raises(SchemaValidationError, match="failed schema validation"):
            validate_entity_response(response, mock_validate)


class TestValidateConflicts:
    """Tests for validate_conflicts function."""

    def test_validate_response_without_conflicts(self):
        """Test validating response without conflicts."""
        response = {
            "extracted_entities": [
                {"entity_group_name": "medications", "entity_name": "aspirin", "entity_value": "81mg"}
            ]
        }
        
        assert validate_conflicts(response) is True

    def test_validate_response_with_valid_conflicts(self):
        """Test validating response with valid conflicts."""
        response = {
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "dob",
                    "entity_value": "1990-01-15",
                    "conflicts": [
                        {"conflicting_value": "1990-01-16", "source_document": "doc-456"}
                    ]
                }
            ]
        }
        
        assert validate_conflicts(response) is True

    def test_validate_response_with_invalid_conflicts_not_list(self):
        """Test validating response with invalid conflicts (not a list)."""
        response = {
            "extracted_entities": [
                {
                    "entity_group_name": "medications",
                    "entity_name": "aspirin",
                    "entity_value": "81mg",
                    "conflicts": "not a list"
                }
            ]
        }
        
        assert validate_conflicts(response) is False

    def test_validate_response_with_invalid_conflict_missing_value(self):
        """Test validating response with conflict missing conflicting_value."""
        response = {
            "extracted_entities": [
                {
                    "entity_group_name": "medications",
                    "entity_name": "aspirin",
                    "entity_value": "81mg",
                    "conflicts": [
                        {"source_document": "doc-456"}
                    ]
                }
            ]
        }
        
        assert validate_conflicts(response) is False


class TestExtractEntityCount:
    """Tests for extract_entity_count function."""

    def test_count_empty_entities(self):
        """Test counting empty entities list."""
        response = {"extracted_entities": []}
        
        assert extract_entity_count(response) == 0

    def test_count_multiple_entities(self):
        """Test counting multiple entities."""
        response = {
            "extracted_entities": [
                {"entity_group_name": "medications", "entity_name": "aspirin", "entity_value": "81mg"},
                {"entity_group_name": "allergies", "entity_name": "penicillin", "entity_value": "severe"},
                {"entity_group_name": "vitals", "entity_name": "bp", "entity_value": "120/80"},
            ]
        }
        
        assert extract_entity_count(response) == 3

    def test_count_missing_entities_key(self):
        """Test counting when extracted_entities key is missing."""
        response = {}
        
        assert extract_entity_count(response) == 0


class TestExtractConflictCount:
    """Tests for extract_conflict_count function."""

    def test_count_no_conflicts(self):
        """Test counting when no conflicts exist."""
        response = {
            "extracted_entities": [
                {"entity_group_name": "medications", "entity_name": "aspirin", "entity_value": "81mg"}
            ]
        }
        
        assert extract_conflict_count(response) == 0

    def test_count_single_conflict(self):
        """Test counting single conflict."""
        response = {
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "dob",
                    "entity_value": "1990-01-15",
                    "conflicts": [
                        {"conflicting_value": "1990-01-16"}
                    ]
                }
            ]
        }
        
        assert extract_conflict_count(response) == 1

    def test_count_multiple_conflicts(self):
        """Test counting multiple conflicts across entities."""
        response = {
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "dob",
                    "entity_value": "1990-01-15",
                    "conflicts": [
                        {"conflicting_value": "1990-01-16"},
                        {"conflicting_value": "1990-01-17"}
                    ]
                },
                {
                    "entity_group_name": "medications",
                    "entity_name": "aspirin",
                    "entity_value": "81mg",
                    "conflicts": [
                        {"conflicting_value": "100mg"}
                    ]
                }
            ]
        }
        
        assert extract_conflict_count(response) == 3


class TestEntityExtractionError:
    """Tests for EntityExtractionError class."""

    def test_error_with_message_only(self):
        """Test error with message only."""
        error = EntityExtractionError("Test error")
        
        assert str(error) == "Test error"
        assert error.details is None

    def test_error_with_details(self):
        """Test error with details."""
        error = EntityExtractionError("Test error", details="Additional info")
        
        assert str(error) == "Test error"
        assert error.details == "Additional info"

    def test_malformed_response_error_is_subclass(self):
        """Test MalformedResponseError is subclass of EntityExtractionError."""
        error = MalformedResponseError("Bad JSON")
        
        assert isinstance(error, EntityExtractionError)

    def test_schema_validation_error_is_subclass(self):
        """Test SchemaValidationError is subclass of EntityExtractionError."""
        error = SchemaValidationError("Invalid schema")
        
        assert isinstance(error, EntityExtractionError)


class TestNormalization:
    """Tests for entity normalization functionality."""

    def test_placeholder_values_are_filtered(self):
        """Test that placeholder values like N/A are filtered out."""
        from worker.entity_extraction.normalization import normalize_payload
        
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-test",
            "extracted_entities": [
                {"entity_group_name": "patient_demographics", "entity_name": "name", "entity_value": "Jane Doe"},
                {"entity_group_name": "patient_demographics", "entity_name": "address", "entity_value": "N/A"},
                {"entity_group_name": "medications", "entity_name": "medication_name", "entity_value": "Unknown"},
            ]
        }
        
        result = normalize_payload(payload, remove_placeholders=True)
        
        assert len(result["extracted_entities"]) == 1
        assert result["extracted_entities"][0]["entity_name"] == "name"

    def test_date_normalization_mm_dd_yyyy(self):
        """Test date normalization from MM/DD/YYYY format."""
        from worker.entity_extraction.normalization import normalize_date
        
        assert normalize_date("01/15/1990") == "1990-01-15"
        assert normalize_date("12/31/2024") == "2024-12-31"

    def test_date_normalization_dd_mmm_yyyy(self):
        """Test date normalization from DD-MMM-YYYY format."""
        from worker.entity_extraction.normalization import normalize_date
        
        assert normalize_date("15-Jan-2024") == "2024-01-15"

    def test_date_normalization_already_iso(self):
        """Test that ISO dates are preserved."""
        from worker.entity_extraction.normalization import normalize_date
        
        assert normalize_date("2024-03-20") == "2024-03-20"

    def test_date_normalization_ambiguous_unchanged(self):
        """Test that ambiguous dates are left unchanged."""
        from worker.entity_extraction.normalization import normalize_date
        
        assert normalize_date("some random text") == "some random text"

    def test_missing_categories_detection(self):
        """Test detection of missing core categories."""
        from worker.entity_extraction.normalization import get_missing_core_categories
        
        payload = {
            "extracted_entities": [
                {"entity_group_name": "patient_demographics", "entity_name": "name", "entity_value": "John"},
                {"entity_group_name": "medications", "entity_name": "med", "entity_value": "Aspirin"},
            ]
        }
        
        missing = get_missing_core_categories(payload)
        
        assert "allergies" in missing
        assert "diagnoses" in missing
        assert "patient_demographics" not in missing
        assert "medications" not in missing

    def test_normalization_preserves_partial_data(self):
        """Test that partial data is preserved without fabrication."""
        from worker.entity_extraction.normalization import normalize_payload
        
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-test",
            "extracted_entities": [
                {"entity_group_name": "medications", "entity_name": "medication_name", "entity_value": "Aspirin"},
            ]
        }
        
        result = normalize_payload(payload)
        
        assert len(result["extracted_entities"]) == 1
        assert result["extracted_entities"][0]["entity_value"] == "Aspirin"
