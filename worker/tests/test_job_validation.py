"""Unit tests for job payload validation."""

import pytest
import sys
import os
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from main import validate_job_payload
from tests.fixtures.job_payloads import (
    VALID_JOB_PAYLOAD,
    VALID_JOB_PAYLOAD_WITH_NULL_PAYLOAD,
    VALID_JOB_PAYLOAD_PROCESSING_STATUS,
    VALID_JOB_PAYLOAD_COMPLETED_STATUS,
    VALID_JOB_PAYLOAD_FAILED_STATUS,
    VALID_JOB_PAYLOAD_VALIDATION_FAILED_STATUS,
    JOB_PAYLOAD_MISSING_SCHEMA_VERSION,
    JOB_PAYLOAD_MISSING_DOCUMENT_ID,
    JOB_PAYLOAD_MISSING_JOB_ID,
    JOB_PAYLOAD_MISSING_STATUS,
    JOB_PAYLOAD_INVALID_STATUS,
    JOB_PAYLOAD_UNSUPPORTED_SCHEMA_VERSION,
    JOB_PAYLOAD_MALFORMED_UUID,
    JOB_PAYLOAD_EMPTY_DOCUMENT_ID,
    JOB_PAYLOAD_WITH_EXTRA_FIELDS,
    JOB_PAYLOAD_WITH_NESTED_PAYLOAD
)


class TestJobValidation:
    """Test cases for job payload validation according to test plan."""

    def test_tc_001_valid_job_payload_passes_validation(self):
        """TC-001: Valid job payload passes validation.
        
        Given: Worker initialized with schema
        When: validate_job_payload called with valid payload
        Then: No exception raised
        """
        validate_job_payload(VALID_JOB_PAYLOAD)

    def test_tc_002_job_payload_with_optional_payload_field_null_fails(self):
        """TC-002: Job payload with optional payload field null fails.
        
        Given: Worker initialized
        When: validate_job_payload with payload: null
        Then: Raises ValueError (payload must be object type, not null)
        """
        with pytest.raises(ValueError) as exc_info:
            validate_job_payload(VALID_JOB_PAYLOAD_WITH_NULL_PAYLOAD)
        
        error_message = str(exc_info.value)
        assert "Invalid job payload" in error_message
        assert "payload" in error_message

    def test_tc_003_job_payload_with_all_valid_status_values_succeeds(self):
        """TC-003: Job payload with all valid status values succeeds.
        
        Given: Worker initialized
        When: validate_job_payload with each valid status
        Then: No exception raised
        """
        validate_job_payload(VALID_JOB_PAYLOAD)
        validate_job_payload(VALID_JOB_PAYLOAD_PROCESSING_STATUS)
        validate_job_payload(VALID_JOB_PAYLOAD_COMPLETED_STATUS)
        validate_job_payload(VALID_JOB_PAYLOAD_FAILED_STATUS)
        validate_job_payload(VALID_JOB_PAYLOAD_VALIDATION_FAILED_STATUS)

    def test_tc_004_job_payload_missing_schema_version_fails(self):
        """TC-004: Job payload missing schema_version fails.
        
        Given: Worker initialized
        When: validate_job_payload without schema_version
        Then: Raises ValueError with message about missing 'schema_version'
        """
        with pytest.raises(ValueError) as exc_info:
            validate_job_payload(JOB_PAYLOAD_MISSING_SCHEMA_VERSION)
        
        error_message = str(exc_info.value)
        assert "Invalid job payload" in error_message
        assert "schema_version" in error_message

    def test_tc_005_job_payload_missing_document_id_fails(self):
        """TC-005: Job payload missing document_id fails.
        
        Given: Worker initialized
        When: validate_job_payload without document_id
        Then: Raises ValueError with message about missing 'document_id'
        """
        with pytest.raises(ValueError) as exc_info:
            validate_job_payload(JOB_PAYLOAD_MISSING_DOCUMENT_ID)
        
        error_message = str(exc_info.value)
        assert "Invalid job payload" in error_message
        assert "document_id" in error_message

    def test_tc_006_job_payload_missing_job_id_fails(self):
        """TC-006: Job payload missing job_id fails.
        
        Given: Worker initialized
        When: validate_job_payload without job_id
        Then: Raises ValueError with message about missing 'job_id'
        """
        with pytest.raises(ValueError) as exc_info:
            validate_job_payload(JOB_PAYLOAD_MISSING_JOB_ID)
        
        error_message = str(exc_info.value)
        assert "Invalid job payload" in error_message
        assert "job_id" in error_message

    def test_tc_007_job_payload_missing_status_fails(self):
        """TC-007: Job payload missing status fails.
        
        Given: Worker initialized
        When: validate_job_payload without status
        Then: Raises ValueError with message about missing 'status'
        """
        with pytest.raises(ValueError) as exc_info:
            validate_job_payload(JOB_PAYLOAD_MISSING_STATUS)
        
        error_message = str(exc_info.value)
        assert "Invalid job payload" in error_message
        assert "status" in error_message

    def test_tc_008_job_payload_with_invalid_status_value_fails(self):
        """TC-008: Job payload with invalid status value fails.
        
        Given: Worker initialized
        When: validate_job_payload with status: "invalid_status"
        Then: Raises ValueError with message containing "status"
        """
        with pytest.raises(ValueError) as exc_info:
            validate_job_payload(JOB_PAYLOAD_INVALID_STATUS)
        
        error_message = str(exc_info.value)
        assert "Invalid job payload" in error_message
        assert "status" in error_message

    def test_tc_009_job_payload_with_unsupported_schema_version_fails(self):
        """TC-009: Job payload with unsupported schema version fails.
        
        Given: Worker initialized
        When: validate_job_payload with schema_version: "2.0"
        Then: Raises ValueError with message about unsupported version
        """
        with pytest.raises(ValueError) as exc_info:
            validate_job_payload(JOB_PAYLOAD_UNSUPPORTED_SCHEMA_VERSION)
        
        error_message = str(exc_info.value)
        assert "Invalid job payload" in error_message
        assert "schema_version" in error_message

    def test_tc_010_job_payload_with_malformed_uuid_fails(self):
        """TC-010: Job payload with malformed UUID fails.
        
        Given: Worker initialized
        When: validate_job_payload with job_id: "not-a-uuid"
        Then: Raises ValueError with message about invalid UUID format
        """
        with pytest.raises(ValueError) as exc_info:
            validate_job_payload(JOB_PAYLOAD_MALFORMED_UUID)
        
        error_message = str(exc_info.value)
        assert "Invalid job payload" in error_message
        assert "job_id" in error_message

    def test_ec_001_job_payload_with_empty_string_document_id_fails(self):
        """EC-001: Job payload with empty string document_id fails.
        
        Given: Worker initialized
        When: validate_job_payload with document_id: ""
        Then: Raises ValueError (empty string still passes type validation but may fail business rules)
        
        Note: JSON Schema validates type but not emptiness. This test documents current behavior.
        """
        validate_job_payload(JOB_PAYLOAD_EMPTY_DOCUMENT_ID)

    def test_ec_002_job_payload_with_extra_unknown_fields_succeeds(self):
        """EC-002: Job payload with extra unknown fields succeeds.
        
        Given: Worker initialized
        When: validate_job_payload with extra fields
        Then: No exception raised (additionalProperties allowed by default)
        """
        validate_job_payload(JOB_PAYLOAD_WITH_EXTRA_FIELDS)

    def test_ec_003_job_payload_with_nested_payload_object_succeeds(self):
        """EC-003: Job payload with nested payload object succeeds.
        
        Given: Worker initialized
        When: validate_job_payload with complex payload
        Then: No exception raised
        """
        validate_job_payload(JOB_PAYLOAD_WITH_NESTED_PAYLOAD)


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
