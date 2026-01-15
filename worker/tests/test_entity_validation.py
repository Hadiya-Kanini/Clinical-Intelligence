"""Unit tests for entity payload validation."""

import pytest
import sys
import os
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from main import validate_entity_payload


class TestEntityValidation:
    """Test cases for entity payload validation."""

    def test_valid_entity_payload_succeeds(self):
        """Test that a valid entity payload passes validation.
        
        Given: Worker initialized
        When: validate_entity_payload called with valid payload
        Then: No exception raised
        """
        valid_payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "name",
                    "entity_value": "Jane Doe"
                }
            ]
        }
        
        validate_entity_payload(valid_payload)

    def test_valid_entity_payload_with_multiple_entities_succeeds(self):
        """Test that entity payload with multiple entities passes validation.
        
        Given: Worker initialized
        When: validate_entity_payload called with multiple entities
        Then: No exception raised
        """
        valid_payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "name",
                    "entity_value": "Jane Doe"
                },
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "dob",
                    "entity_value": "1980-01-01"
                },
                {
                    "entity_group_name": "medications",
                    "entity_name": "medication_name",
                    "entity_value": "Aspirin"
                }
            ]
        }
        
        validate_entity_payload(valid_payload)

    def test_valid_entity_payload_with_optional_fields_succeeds(self):
        """Test that entity payload with optional fields passes validation.
        
        Given: Worker initialized
        When: validate_entity_payload called with optional fields
        Then: No exception raised
        """
        valid_payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "name",
                    "entity_value": "Jane Doe",
                    "rationale": "Found in patient header section",
                    "source_text": "Patient Name: Jane Doe"
                }
            ]
        }
        
        validate_entity_payload(valid_payload)

    def test_entity_payload_missing_schema_version_fails(self):
        """Test that missing schema_version causes validation failure.
        
        Given: Worker initialized
        When: validate_entity_payload without schema_version
        Then: Raises ValueError with message about missing 'schema_version'
        """
        invalid_payload = {
            "document_id": "doc-123",
            "extracted_entities": []
        }
        
        with pytest.raises(ValueError) as exc_info:
            validate_entity_payload(invalid_payload)
        
        error_message = str(exc_info.value)
        assert "Invalid entity payload: missing required field 'schema_version'" in error_message

    def test_entity_payload_missing_document_id_fails(self):
        """Test that missing document_id causes validation failure.
        
        Given: Worker initialized
        When: validate_entity_payload without document_id
        Then: Raises ValueError with message about missing 'document_id'
        """
        invalid_payload = {
            "schema_version": "1.0",
            "extracted_entities": []
        }
        
        with pytest.raises(ValueError) as exc_info:
            validate_entity_payload(invalid_payload)
        
        error_message = str(exc_info.value)
        assert "Invalid entity payload" in error_message
        assert "document_id" in error_message

    def test_entity_payload_missing_extracted_entities_fails(self):
        """Test that missing extracted_entities causes validation failure.
        
        Given: Worker initialized
        When: validate_entity_payload without extracted_entities
        Then: Raises ValueError with message about missing 'extracted_entities'
        """
        invalid_payload = {
            "schema_version": "1.0",
            "document_id": "doc-123"
        }
        
        with pytest.raises(ValueError) as exc_info:
            validate_entity_payload(invalid_payload)
        
        error_message = str(exc_info.value)
        assert "Invalid entity payload" in error_message
        assert "extracted_entities" in error_message

    def test_entity_payload_with_unknown_schema_version_fails(self):
        """Test that unknown schema version causes validation failure.
        
        Given: Worker initialized
        When: validate_entity_payload with schema_version: "2.0"
        Then: Raises ValueError with message about unknown version
        """
        invalid_payload = {
            "schema_version": "2.0",
            "document_id": "doc-123",
            "extracted_entities": []
        }
        
        with pytest.raises(ValueError) as exc_info:
            validate_entity_payload(invalid_payload)
        
        error_message = str(exc_info.value)
        assert "Unknown entity schema version: 2.0" in error_message

    def test_entity_payload_with_invalid_entity_structure_fails(self):
        """Test that invalid entity structure causes validation failure.
        
        Given: Worker initialized
        When: validate_entity_payload with missing required entity fields
        Then: Raises ValueError
        """
        invalid_payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics"
                }
            ]
        }
        
        with pytest.raises(ValueError) as exc_info:
            validate_entity_payload(invalid_payload)
        
        error_message = str(exc_info.value)
        assert "Invalid entity payload" in error_message

    def test_entity_payload_with_empty_extracted_entities_succeeds(self):
        """Test that entity payload with empty extracted_entities array passes validation.
        
        Given: Worker initialized
        When: validate_entity_payload with empty extracted_entities array
        Then: No exception raised
        """
        valid_payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": []
        }
        
        validate_entity_payload(valid_payload)

    def test_entity_payload_with_additional_properties_succeeds(self):
        """Test that entity payload with additional properties passes validation.
        
        Given: Worker initialized
        When: validate_entity_payload with additional_entities field
        Then: No exception raised
        """
        valid_payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "patient_demographics",
                    "entity_name": "name",
                    "entity_value": "Jane Doe"
                }
            ],
            "additional_entities": {
                "custom_field": "custom_value"
            }
        }
        
        validate_entity_payload(valid_payload)


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
