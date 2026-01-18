"""
Unit tests for grounding_validator.py covering grounding edge cases
and deterministic failure behavior.
"""

import pytest
from worker.entity_validation.grounding_validator import (
    validate_grounding,
    validate_grounding_or_raise,
    GroundingValidationError,
    GroundingError,
    GroundingValidationResult,
)


class TestValidateGrounding:
    """Tests for validate_grounding function."""

    def test_valid_grounded_payload_passes(self):
        """Fully grounded payload passes validation."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe",
                    "document_location": {
                        "page": 1,
                        "section": "Header",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                },
                {
                    "entity_group_name": "Diagnosis",
                    "entity_name": "condition",
                    "entity_value": "Hypertension",
                    "source_text": "Diagnosis: Hypertension",
                    "document_location": {
                        "page": 2,
                        "section": "Assessment",
                        "coordinates": {"x": 50, "y": 100, "width": 200, "height": 20}
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is True
        assert len(result.errors) == 0

    def test_schema_version_1_0_skips_grounding_validation(self):
        """Schema version 1.0 does not enforce grounding."""
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe"
                    # No source_text or document_location
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is True
        assert len(result.errors) == 0

    def test_missing_source_text_fails(self):
        """Missing source_text fails validation for v1.1."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "document_location": {
                        "page": 1,
                        "section": "Header",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                    # Missing source_text
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert len(result.errors) == 1
        assert result.errors[0].entity_index == 0
        assert result.errors[0].field_path == "source_text"
        assert result.errors[0].error_type == "missing"

    def test_empty_source_text_fails(self):
        """Empty source_text fails validation."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "   ",  # Whitespace only
                    "document_location": {
                        "page": 1,
                        "section": "Header",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert any(e.field_path == "source_text" and e.error_type == "empty" for e in result.errors)

    def test_missing_document_location_fails(self):
        """Missing document_location fails validation."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe"
                    # Missing document_location
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert any(e.field_path == "document_location" and e.error_type == "missing" for e in result.errors)

    def test_page_null_fails(self):
        """Null page value fails validation."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe",
                    "document_location": {
                        "page": None,
                        "section": "Header",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert any(e.field_path == "document_location.page" for e in result.errors)

    def test_page_zero_fails(self):
        """Page value of 0 fails validation (must be >= 1)."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe",
                    "document_location": {
                        "page": 0,
                        "section": "Header",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert any(e.field_path == "document_location.page" and e.error_type == "invalid_value" for e in result.errors)

    def test_missing_section_fails(self):
        """Missing section fails validation."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe",
                    "document_location": {
                        "page": 1,
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                        # Missing section
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert any(e.field_path == "document_location.section" for e in result.errors)

    def test_missing_coordinates_fails(self):
        """Missing coordinates fails validation."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe",
                    "document_location": {
                        "page": 1,
                        "section": "Header"
                        # Missing coordinates
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert any(e.field_path == "document_location.coordinates" for e in result.errors)

    def test_missing_coordinate_keys_fails(self):
        """Missing individual coordinate keys fail validation."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe",
                    "document_location": {
                        "page": 1,
                        "section": "Header",
                        "coordinates": {"x": 10, "y": 20}  # Missing width and height
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert any(e.field_path == "document_location.coordinates.width" for e in result.errors)
        assert any(e.field_path == "document_location.coordinates.height" for e in result.errors)

    def test_non_numeric_coordinate_fails(self):
        """Non-numeric coordinate values fail validation."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe",
                    "document_location": {
                        "page": 1,
                        "section": "Header",
                        "coordinates": {"x": "ten", "y": 20, "width": 100, "height": 15}
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        assert any(e.field_path == "document_location.coordinates.x" and e.error_type == "invalid_type" for e in result.errors)

    def test_multiple_entities_with_errors(self):
        """Multiple entities with errors are all reported."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe"
                    # Missing source_text and document_location
                },
                {
                    "entity_group_name": "Diagnosis",
                    "entity_name": "condition",
                    "entity_value": "Hypertension",
                    "source_text": "",  # Empty
                    "document_location": {
                        "page": 1,
                        "section": "Assessment",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                },
                {
                    "entity_group_name": "Medication",
                    "entity_name": "drug",
                    "entity_value": "Aspirin",
                    "source_text": "Medication: Aspirin",
                    "document_location": {
                        "page": 0,  # Invalid page
                        "section": "Medications",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        assert result.is_valid is False
        # Check errors from multiple entities
        entity_indices = {e.entity_index for e in result.errors}
        assert 0 in entity_indices
        assert 1 in entity_indices
        assert 2 in entity_indices


class TestValidateGroundingOrRaise:
    """Tests for validate_grounding_or_raise function."""

    def test_valid_payload_does_not_raise(self):
        """Valid payload does not raise exception."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "Patient: John Doe",
                    "document_location": {
                        "page": 1,
                        "section": "Header",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                }
            ]
        }

        # Should not raise
        validate_grounding_or_raise(payload)

    def test_invalid_payload_raises_grounding_error(self):
        """Invalid payload raises GroundingValidationError."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe"
                    # Missing citations
                }
            ]
        }

        with pytest.raises(GroundingValidationError) as exc_info:
            validate_grounding_or_raise(payload)

        assert len(exc_info.value.errors) > 0


class TestPhiSafeErrorMessages:
    """Tests ensuring error messages do not leak PHI."""

    def test_error_message_does_not_contain_entity_value(self):
        """Error messages do not include raw entity values."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "SENSITIVE_PATIENT_NAME_12345"
                    # Missing citations
                }
            ]
        }

        result = validate_grounding(payload)

        # Check that sensitive value is not in any error message
        for error in result.errors:
            assert "SENSITIVE_PATIENT_NAME_12345" not in error.message
            assert "SENSITIVE_PATIENT_NAME_12345" not in str(error)

    def test_error_message_does_not_contain_source_text(self):
        """Error messages do not include raw source text."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe",
                    "source_text": "SENSITIVE_SOURCE_TEXT_WITH_PHI",
                    "document_location": {
                        "page": 0,  # Invalid to trigger error
                        "section": "Header",
                        "coordinates": {"x": 10, "y": 20, "width": 100, "height": 15}
                    }
                }
            ]
        }

        result = validate_grounding(payload)

        # Check that source text is not in any error message
        for error in result.errors:
            assert "SENSITIVE_SOURCE_TEXT_WITH_PHI" not in error.message
            assert "SENSITIVE_SOURCE_TEXT_WITH_PHI" not in str(error)

    def test_exception_message_is_phi_safe(self):
        """Exception message does not contain PHI."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "ssn",
                    "entity_value": "123-45-6789"  # Sensitive value
                }
            ]
        }

        with pytest.raises(GroundingValidationError) as exc_info:
            validate_grounding_or_raise(payload)

        exception_message = str(exc_info.value)
        assert "123-45-6789" not in exception_message

    def test_error_includes_only_index_and_field_path(self):
        """Errors include only entity index and field path, not values."""
        payload = {
            "schema_version": "1.1",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "Demographics",
                    "entity_name": "patient_name",
                    "entity_value": "John Doe"
                }
            ]
        }

        result = validate_grounding(payload)

        for error in result.errors:
            # Should contain index and field path
            assert error.entity_index == 0
            assert error.field_path in ["source_text", "document_location"]
            # Should not contain entity values
            assert "John Doe" not in error.message
            assert "Demographics" not in error.message


class TestGroundingErrorDataclass:
    """Tests for GroundingError dataclass."""

    def test_to_dict(self):
        """to_dict returns correct dictionary."""
        error = GroundingError(
            entity_index=3,
            field_path="source_text",
            error_type="missing",
            message="source_text is required"
        )

        result = error.to_dict()

        assert result == {
            "entity_index": 3,
            "field_path": "source_text",
            "error_type": "missing",
            "message": "source_text is required"
        }

    def test_str_representation(self):
        """String representation is PHI-safe and informative."""
        error = GroundingError(
            entity_index=5,
            field_path="document_location.page",
            error_type="invalid_value",
            message="must be >= 1"
        )

        result = str(error)

        assert result == "extracted_entities[5].document_location.page: must be >= 1"


class TestGroundingValidationResult:
    """Tests for GroundingValidationResult dataclass."""

    def test_to_dict_valid(self):
        """to_dict for valid result."""
        result = GroundingValidationResult(is_valid=True)

        dict_result = result.to_dict()

        assert dict_result == {
            "is_valid": True,
            "errors": [],
            "error_count": 0
        }

    def test_to_dict_with_errors(self):
        """to_dict includes error details."""
        errors = [
            GroundingError(0, "source_text", "missing", "required"),
            GroundingError(1, "document_location", "missing", "required")
        ]
        result = GroundingValidationResult(is_valid=False, errors=errors)

        dict_result = result.to_dict()

        assert dict_result["is_valid"] is False
        assert dict_result["error_count"] == 2
        assert len(dict_result["errors"]) == 2
