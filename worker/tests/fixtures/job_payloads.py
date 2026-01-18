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

# v1.1 payloads with text extraction fields
VALID_JOB_PAYLOAD_V11_PDF_EXTRACTION = {
    "schema_version": "1.1",
    "job_id": "11111111-1111-1111-1111-111111111111",
    "document_id": "doc-pdf-001",
    "status": "pending",
    "payload": {
        "storage_path": "/documents/patient-123/report.pdf",
        "mime_type": "application/pdf"
    }
}

VALID_JOB_PAYLOAD_V11_DOCX_EXTRACTION = {
    "schema_version": "1.1",
    "job_id": "22222222-2222-2222-2222-222222222222",
    "document_id": "doc-docx-001",
    "status": "pending",
    "payload": {
        "storage_path": "/documents/patient-456/notes.docx",
        "mime_type": "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    }
}

# v1.1 payloads with patient merge fields
VALID_JOB_PAYLOAD_V11_PATIENT_MERGE = {
    "schema_version": "1.1",
    "job_id": "33333333-3333-3333-3333-333333333333",
    "document_id": "doc-merge-001",
    "status": "pending",
    "payload": {
        "patient_id": "44444444-4444-4444-4444-444444444444",
        "document_ids": ["doc-001", "doc-002", "doc-003"]
    }
}

VALID_JOB_PAYLOAD_V11_PATIENT_MERGE_WITH_IDENTIFIERS = {
    "schema_version": "1.1",
    "job_id": "55555555-5555-5555-5555-555555555555",
    "document_id": "doc-merge-002",
    "status": "pending",
    "payload": {
        "document_ids": ["doc-004", "doc-005"],
        "patient_identifiers": {
            "mrn": "MRN-12345",
            "name": "John Doe",
            "dob": "1980-05-15"
        }
    }
}

# v1.1 payload with all extraction and merge fields
VALID_JOB_PAYLOAD_V11_FULL = {
    "schema_version": "1.1",
    "job_id": "66666666-6666-6666-6666-666666666666",
    "document_id": "doc-full-001",
    "status": "processing",
    "payload": {
        "storage_path": "/documents/patient-789/comprehensive.pdf",
        "mime_type": "application/pdf",
        "patient_id": "77777777-7777-7777-7777-777777777777",
        "document_ids": ["doc-full-001", "doc-full-002"],
        "patient_identifiers": {
            "mrn": "MRN-67890"
        }
    }
}
