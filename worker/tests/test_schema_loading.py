"""Unit tests for schema loading functions."""

import pytest
import json
import sys
import os
from unittest.mock import patch, mock_open, MagicMock
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from main import _load_job_schema, _load_entity_schema
from tests.fixtures.schemas import MOCK_JOB_SCHEMA, MOCK_ENTITY_SCHEMA, INVALID_JSON_SCHEMA


class TestSchemaLoading:
    """Test cases for schema loading functions according to test plan."""

    def test_es_001_schema_file_not_found_raises_clear_error(self):
        """ES-001: Schema file not found raises clear error.
        
        Given: Schema file missing
        When: _load_job_schema called
        Then: Raises FileNotFoundError with message about file path
        """
        with patch('builtins.open', side_effect=FileNotFoundError):
            with patch('os.path.join', return_value='/nonexistent/path/job.schema.json'):
                with pytest.raises(FileNotFoundError) as exc_info:
                    _load_job_schema()
                
                error_message = str(exc_info.value)
                assert "Job schema file not found at" in error_message
                assert "/nonexistent/path/job.schema.json" in error_message

    def test_es_002_invalid_json_in_schema_file_raises_clear_error(self):
        """ES-002: Invalid JSON in schema file raises clear error.
        
        Given: Schema file has malformed JSON
        When: _load_job_schema called
        Then: Raises ValueError with message about invalid JSON
        """
        with patch('builtins.open', mock_open(read_data=INVALID_JSON_SCHEMA)):
            with patch('os.path.join', return_value='/test/path/job.schema.json'):
                with pytest.raises(ValueError) as exc_info:
                    _load_job_schema()
                
                error_message = str(exc_info.value)
                assert "Invalid JSON in job schema file" in error_message

    def test_es_003_entity_schema_with_unknown_version_fails(self):
        """ES-003: Entity schema with unknown version fails.
        
        Given: Worker initialized
        When: _load_entity_schema("2.0") called
        Then: Raises ValueError with message about unknown version
        """
        with pytest.raises(ValueError) as exc_info:
            _load_entity_schema("2.0")
        
        error_message = str(exc_info.value)
        assert "Unknown entity schema version: 2.0" in error_message

    def test_load_job_schema_success(self):
        """Test successful loading of job schema."""
        mock_schema_json = json.dumps(MOCK_JOB_SCHEMA)
        
        with patch('builtins.open', mock_open(read_data=mock_schema_json)):
            schema = _load_job_schema()
            assert schema == MOCK_JOB_SCHEMA
            assert schema["title"] == "Job"
            assert "schema_version" in schema["properties"]

    def test_load_entity_schema_v1_success(self):
        """Test successful loading of entity schema version 1.0."""
        mock_schema_json = json.dumps(MOCK_ENTITY_SCHEMA)
        
        with patch('builtins.open', mock_open(read_data=mock_schema_json)):
            schema = _load_entity_schema("1.0")
            assert schema == MOCK_ENTITY_SCHEMA
            assert schema["title"] == "EntityExtractionResult"
            assert "extracted_entities" in schema["properties"]

    def test_load_job_schema_file_permission_error(self):
        """Test handling of permission errors when loading job schema."""
        with patch('builtins.open', side_effect=PermissionError("Permission denied")):
            with patch('os.path.join', return_value='/test/path/job.schema.json'):
                with pytest.raises(RuntimeError) as exc_info:
                    _load_job_schema()
                
                error_message = str(exc_info.value)
                assert "Unexpected error loading job schema" in error_message

    def test_load_entity_schema_file_not_found(self):
        """Test handling of missing entity schema file."""
        with patch('builtins.open', side_effect=FileNotFoundError):
            with patch('os.path.join', return_value='/nonexistent/path/entity.schema.json'):
                with pytest.raises(FileNotFoundError) as exc_info:
                    _load_entity_schema("1.0")
                
                error_message = str(exc_info.value)
                assert "Entity schema file not found at" in error_message
                assert "/nonexistent/path/entity.schema.json" in error_message

    def test_load_entity_schema_invalid_json(self):
        """Test handling of invalid JSON in entity schema file."""
        with patch('builtins.open', mock_open(read_data=INVALID_JSON_SCHEMA)):
            with patch('os.path.join', return_value='/test/path/entity.schema.json'):
                with pytest.raises(ValueError) as exc_info:
                    _load_entity_schema("1.0")
                
                error_message = str(exc_info.value)
                assert "Invalid JSON in entity schema file" in error_message

    def test_load_entity_schema_file_permission_error(self):
        """Test handling of permission errors when loading entity schema."""
        with patch('builtins.open', side_effect=PermissionError("Permission denied")):
            with patch('os.path.join', return_value='/test/path/entity.schema.json'):
                with pytest.raises(RuntimeError) as exc_info:
                    _load_entity_schema("1.0")
                
                error_message = str(exc_info.value)
                assert "Unexpected error loading entity schema" in error_message


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
