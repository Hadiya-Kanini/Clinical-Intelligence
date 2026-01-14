import pytest
import json
import os
import tempfile
from unittest.mock import patch, mock_open
from main import validate_job_payload, validate_entity_payload, _load_job_schema, _load_entity_schema


class TestJobValidation:
    """Test cases for job payload validation."""

    def test_validate_job_payload_valid_payload_succeeds(self):
        """Test that a valid job payload passes validation."""
        valid_payload = {
            "schema_version": "1.0",
            "job_id": "00000000-0000-0000-0000-000000000000",
            "document_id": "doc-123",
            "status": "pending",
            "payload": {}
        }

        # Should not raise any exception
        validate_job_payload(valid_payload)

    def test_validate_job_payload_missing_required_field_fails(self):
        """Test that missing required fields cause validation failure."""
        invalid_payload = {
            "job_id": "00000000-0000-0000-0000-000000000000",
            "document_id": "doc-123"
            # Missing required fields: schema_version, status, payload
        }

        with pytest.raises(ValueError, match="Invalid job payload"):
            validate_job_payload(invalid_payload)

    def test_validate_job_payload_invalid_status_fails(self):
        """Test that invalid status values cause validation failure."""
        invalid_payload = {
            "schema_version": "1.0",
            "job_id": "00000000-0000-0000-0000-000000000000",
            "document_id": "doc-123",
            "status": "invalid_status",
            "payload": {}
        }

        with pytest.raises(ValueError, match="Invalid job payload"):
            validate_job_payload(invalid_payload)


class TestEntityValidation:
    """Test cases for entity payload validation."""

    def test_validate_entity_payload_valid_payload_succeeds(self):
        """Test that a valid entity payload passes validation."""
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

        # Should not raise any exception
        validate_entity_payload(valid_payload)

    def test_validate_entity_payload_missing_schema_version_fails(self):
        """Test that missing schema_version causes validation failure."""
        invalid_payload = {
            "document_id": "doc-123",
            "extracted_entities": []
        }

        with pytest.raises(ValueError, match="Invalid entity payload: missing required field 'schema_version'"):
            validate_entity_payload(invalid_payload)

    def test_validate_entity_payload_unknown_schema_version_fails(self):
        """Test that unknown schema versions cause validation failure."""
        invalid_payload = {
            "schema_version": "2.0",
            "document_id": "doc-123",
            "extracted_entities": []
        }

        with pytest.raises(ValueError, match="Unknown entity schema version: 2.0"):
            validate_entity_payload(invalid_payload)


class TestSchemaLoading:
    """Test cases for schema loading functions."""

    def test_load_job_schema_file_not_found(self):
        """Test handling of missing job schema file."""
        with patch('builtins.open', side_effect=FileNotFoundError):
            with patch('os.path.join', return_value='/nonexistent/path/job.schema.json'):
                with pytest.raises(FileNotFoundError, match="Job schema file not found at"):
                    _load_job_schema()

    def test_load_job_schema_invalid_json(self):
        """Test handling of invalid JSON in job schema file."""
        invalid_json = "{ invalid json content"
        
        with patch('builtins.open', mock_open(read_data=invalid_json)):
            with patch('os.path.join', return_value='/test/path/job.schema.json'):
                with pytest.raises(ValueError, match="Invalid JSON in job schema file"):
                    _load_job_schema()

    def test_load_entity_schema_file_not_found(self):
        """Test handling of missing entity schema file."""
        with patch('builtins.open', side_effect=FileNotFoundError):
            with patch('os.path.join', return_value='/nonexistent/path/entity.schema.json'):
                with pytest.raises(FileNotFoundError, match="Entity schema file not found at"):
                    _load_entity_schema("1.0")

    def test_load_entity_schema_invalid_json(self):
        """Test handling of invalid JSON in entity schema file."""
        invalid_json = "{ invalid json content"
        
        with patch('builtins.open', mock_open(read_data=invalid_json)):
            with patch('os.path.join', return_value='/test/path/entity.schema.json'):
                with pytest.raises(ValueError, match="Invalid JSON in entity schema file"):
                    _load_entity_schema("1.0")

    def test_load_entity_schema_unknown_version(self):
        """Test handling of unknown schema version."""
        with pytest.raises(ValueError, match="Unknown entity schema version: 2.0"):
            _load_entity_schema("2.0")


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
