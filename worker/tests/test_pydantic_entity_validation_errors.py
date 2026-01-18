"""
Unit tests for Pydantic-based entity validation and error normalization.

Tests cover:
- Valid payload validation success
- Missing required fields produce normalized errors
- Unknown schema versions fail deterministically
- Partial validation failure (one invalid entity fails whole payload)
- PHI-safe error messages (no raw LLM output in errors)
"""

import pytest

from worker.entity_schemas.registry import (
    get_entity_schema,
    get_supported_versions,
    UnsupportedSchemaVersionError,
)
from worker.entity_validation.entity_validator import (
    validate_entity_payload_with_pydantic,
    normalize_validation_errors,
    validate_or_raise,
    EntityValidationError,
    EntityValidationResult,
    ValidationErrorDetail,
)
from worker.tests.fixtures.entity_payloads import (
    VALID_ENTITY_PAYLOAD,
    VALID_ENTITY_PAYLOAD_WITH_MULTIPLE_ENTITIES,
    VALID_ENTITY_PAYLOAD_WITH_CONFLICTS,
    VALID_ENTITY_PAYLOAD_WITH_GROUNDING,
    ENTITY_PAYLOAD_MISSING_SCHEMA_VERSION,
    ENTITY_PAYLOAD_UNKNOWN_SCHEMA_VERSION,
    ENTITY_PAYLOAD_MISSING_REQUIRED_FIELDS,
)


class TestSchemaRegistry:
    """Tests for schema version registry."""
    
    def test_get_supported_versions_returns_list(self):
        """Supported versions returns a non-empty list."""
        versions = get_supported_versions()
        assert isinstance(versions, list)
        assert len(versions) > 0
        assert "1.0" in versions
    
    def test_get_entity_schema_v1_returns_model(self):
        """Schema version 1.0 resolves to a Pydantic model."""
        schema_class = get_entity_schema("1.0")
        assert schema_class is not None
        assert hasattr(schema_class, "model_validate")
    
    def test_get_entity_schema_unknown_version_raises(self):
        """Unknown schema version raises UnsupportedSchemaVersionError."""
        with pytest.raises(UnsupportedSchemaVersionError) as exc_info:
            get_entity_schema("99.0")
        
        assert "99.0" in str(exc_info.value)
        assert "1.0" in str(exc_info.value)
    
    def test_get_entity_schema_empty_version_raises(self):
        """Empty schema version raises UnsupportedSchemaVersionError."""
        with pytest.raises(UnsupportedSchemaVersionError):
            get_entity_schema("")


class TestValidEntityPayloads:
    """Tests for valid entity payload validation."""
    
    def test_valid_payload_passes_validation(self):
        """Valid entity payload passes Pydantic validation."""
        result = validate_entity_payload_with_pydantic(VALID_ENTITY_PAYLOAD)
        
        assert result.is_valid is True
        assert result.error_message is None
        assert len(result.error_details) == 0
        assert result.validated_payload is not None
    
    def test_valid_payload_with_multiple_entities(self):
        """Payload with multiple entities validates successfully."""
        result = validate_entity_payload_with_pydantic(
            VALID_ENTITY_PAYLOAD_WITH_MULTIPLE_ENTITIES
        )
        
        assert result.is_valid is True
        assert result.validated_payload is not None
        assert len(result.validated_payload["extracted_entities"]) == 3
    
    def test_valid_payload_with_conflicts(self):
        """Payload with conflict data validates successfully."""
        result = validate_entity_payload_with_pydantic(
            VALID_ENTITY_PAYLOAD_WITH_CONFLICTS
        )
        
        assert result.is_valid is True
    
    def test_valid_payload_with_grounding(self):
        """Payload with grounding data validates successfully."""
        result = validate_entity_payload_with_pydantic(
            VALID_ENTITY_PAYLOAD_WITH_GROUNDING
        )
        
        assert result.is_valid is True
    
    def test_empty_extracted_entities_is_valid(self):
        """Payload with empty extracted_entities list is valid."""
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": []
        }
        result = validate_entity_payload_with_pydantic(payload)
        
        assert result.is_valid is True


class TestInvalidEntityPayloads:
    """Tests for invalid entity payload validation."""
    
    def test_missing_schema_version_fails(self):
        """Missing schema_version produces clear error."""
        result = validate_entity_payload_with_pydantic(
            ENTITY_PAYLOAD_MISSING_SCHEMA_VERSION
        )
        
        assert result.is_valid is False
        assert "schema_version" in result.error_message.lower()
        assert len(result.error_details) > 0
        assert any("schema_version" in d.field_path for d in result.error_details)
    
    def test_unknown_schema_version_fails_deterministically(self):
        """Unknown schema version fails with clear error message."""
        result = validate_entity_payload_with_pydantic(
            ENTITY_PAYLOAD_UNKNOWN_SCHEMA_VERSION
        )
        
        assert result.is_valid is False
        assert "2.0" in result.error_message
        assert "unsupported" in result.error_message.lower() or "Unsupported" in result.error_message
    
    def test_missing_document_id_fails(self):
        """Missing document_id produces validation error."""
        result = validate_entity_payload_with_pydantic(
            ENTITY_PAYLOAD_MISSING_REQUIRED_FIELDS
        )
        
        assert result.is_valid is False
        assert len(result.error_details) > 0
    
    def test_invalid_entity_fails_whole_payload(self):
        """Payload with one invalid entity fails entire validation."""
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "name",
                    "entity_value": "Jane Doe"
                },
                {
                    "entity_group_name": "",
                    "entity_name": "dob",
                    "entity_value": "1990-01-01"
                }
            ]
        }
        result = validate_entity_payload_with_pydantic(payload)
        
        assert result.is_valid is False
        assert result.error_message is not None
    
    def test_missing_entity_value_fails(self):
        """Entity missing required entity_value fails validation."""
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "medications",
                    "entity_name": "aspirin"
                }
            ]
        }
        result = validate_entity_payload_with_pydantic(payload)
        
        assert result.is_valid is False


class TestErrorNormalization:
    """Tests for PHI-safe error normalization."""
    
    def test_error_details_have_field_path(self):
        """Error details include field path."""
        payload = {
            "schema_version": "1.0",
            "extracted_entities": []
        }
        result = validate_entity_payload_with_pydantic(payload)
        
        assert result.is_valid is False
        assert len(result.error_details) > 0
        assert all(d.field_path for d in result.error_details)
    
    def test_error_details_have_error_type(self):
        """Error details include error type."""
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "test",
                    "entity_name": "test"
                }
            ]
        }
        result = validate_entity_payload_with_pydantic(payload)
        
        assert result.is_valid is False
        assert all(d.error_type for d in result.error_details)
    
    def test_error_messages_are_phi_safe(self):
        """Error messages do not contain raw entity values."""
        sensitive_value = "SENSITIVE_PHI_DATA_12345"
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "",
                    "entity_name": "test",
                    "entity_value": sensitive_value
                }
            ]
        }
        result = validate_entity_payload_with_pydantic(payload)
        
        assert result.is_valid is False
        assert sensitive_value not in result.error_message
        for detail in result.error_details:
            assert sensitive_value not in detail.message
    
    def test_error_result_to_dict(self):
        """EntityValidationResult.to_dict() produces serializable output."""
        result = EntityValidationResult(
            is_valid=False,
            error_message="Test error",
            error_details=[
                ValidationErrorDetail(
                    field_path="test.field",
                    error_type="missing",
                    message="Field is required"
                )
            ]
        )
        
        result_dict = result.to_dict()
        
        assert result_dict["is_valid"] is False
        assert result_dict["error_message"] == "Test error"
        assert len(result_dict["error_details"]) == 1
        assert result_dict["error_details"][0]["field_path"] == "test.field"


class TestValidateOrRaise:
    """Tests for validate_or_raise function."""
    
    def test_valid_payload_returns_dict(self):
        """Valid payload returns validated dictionary."""
        result = validate_or_raise(VALID_ENTITY_PAYLOAD)
        
        assert isinstance(result, dict)
        assert result["schema_version"] == "1.0"
        assert result["document_id"] == "doc-123"
    
    def test_invalid_payload_raises_exception(self):
        """Invalid payload raises EntityValidationError."""
        with pytest.raises(EntityValidationError) as exc_info:
            validate_or_raise(ENTITY_PAYLOAD_MISSING_SCHEMA_VERSION)
        
        error = exc_info.value
        assert error.error_message is not None
        assert len(error.error_details) > 0
    
    def test_exception_converts_to_result(self):
        """EntityValidationError.to_result() produces EntityValidationResult."""
        try:
            validate_or_raise(ENTITY_PAYLOAD_UNKNOWN_SCHEMA_VERSION)
        except EntityValidationError as e:
            result = e.to_result()
            
            assert result.is_valid is False
            assert result.error_message is not None
