"""Test data fixtures for job payloads."""

VALID_JOB_PAYLOAD = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "pending",
    "payload": {}
}

VALID_JOB_PAYLOAD_WITH_NULL_PAYLOAD = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "pending",
    "payload": None
}

VALID_JOB_PAYLOAD_PROCESSING_STATUS = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "processing",
    "payload": {}
}

VALID_JOB_PAYLOAD_COMPLETED_STATUS = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "completed",
    "payload": {}
}

VALID_JOB_PAYLOAD_FAILED_STATUS = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "failed",
    "payload": {}
}

VALID_JOB_PAYLOAD_VALIDATION_FAILED_STATUS = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "validation_failed",
    "payload": {}
}

JOB_PAYLOAD_MISSING_SCHEMA_VERSION = {
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "pending"
}

JOB_PAYLOAD_MISSING_DOCUMENT_ID = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "status": "pending"
}

JOB_PAYLOAD_MISSING_JOB_ID = {
    "schema_version": "1.0",
    "document_id": "doc-123",
    "status": "pending"
}

JOB_PAYLOAD_MISSING_STATUS = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123"
}

JOB_PAYLOAD_INVALID_STATUS = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "invalid_status",
    "payload": {}
}

JOB_PAYLOAD_UNSUPPORTED_SCHEMA_VERSION = {
    "schema_version": "2.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "pending",
    "payload": {}
}

JOB_PAYLOAD_MALFORMED_UUID = {
    "schema_version": "1.0",
    "job_id": "not-a-uuid",
    "document_id": "doc-123",
    "status": "pending",
    "payload": {}
}

JOB_PAYLOAD_EMPTY_DOCUMENT_ID = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "",
    "status": "pending",
    "payload": {}
}

JOB_PAYLOAD_WITH_EXTRA_FIELDS = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "pending",
    "payload": {},
    "extra_field_1": "value1",
    "extra_field_2": 123
}

JOB_PAYLOAD_WITH_NESTED_PAYLOAD = {
    "schema_version": "1.0",
    "job_id": "00000000-0000-0000-0000-000000000000",
    "document_id": "doc-123",
    "status": "pending",
    "payload": {
        "nested_field": "value",
        "nested_object": {
            "deep_field": "deep_value"
        },
        "nested_array": [1, 2, 3]
    }
}
