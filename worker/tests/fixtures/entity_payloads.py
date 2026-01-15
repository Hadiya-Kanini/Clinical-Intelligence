"""Test data fixtures for entity payloads."""

VALID_ENTITY_PAYLOAD = {
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

VALID_ENTITY_PAYLOAD_WITH_MULTIPLE_ENTITIES = {
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

ENTITY_PAYLOAD_MISSING_SCHEMA_VERSION = {
    "document_id": "doc-123",
    "extracted_entities": []
}

ENTITY_PAYLOAD_UNKNOWN_SCHEMA_VERSION = {
    "schema_version": "2.0",
    "document_id": "doc-123",
    "extracted_entities": []
}
