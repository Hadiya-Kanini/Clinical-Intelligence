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

VALID_ENTITY_PAYLOAD_WITH_CONFLICTS = {
    "schema_version": "1.0",
    "document_id": "doc-123",
    "extracted_entities": [
        {
            "entity_group_name": "patient_demographics",
            "entity_name": "dob",
            "entity_value": "1990-01-15",
            "source_text": "DOB: 01/15/1990",
            "document_location": {
                "page": 1,
                "section": "Demographics"
            },
            "conflicts": [
                {
                    "conflicting_value": "1990-01-16",
                    "source_document": "doc-456",
                    "document_location": {
                        "page": 2,
                        "section": "Patient Info"
                    }
                }
            ]
        },
        {
            "entity_group_name": "medications",
            "entity_name": "aspirin",
            "entity_value": "81mg daily",
            "source_text": "Aspirin 81mg PO daily",
            "rationale": "Current medication for cardiac protection"
        }
    ]
}

VALID_ENTITY_PAYLOAD_WITH_GROUNDING = {
    "schema_version": "1.0",
    "document_id": "doc-789",
    "extracted_entities": [
        {
            "entity_group_name": "allergies",
            "entity_name": "penicillin",
            "entity_value": "severe - anaphylaxis",
            "source_text": "ALLERGIES: Penicillin - severe reaction (anaphylaxis)",
            "document_location": {
                "page": 1,
                "section": "Allergies"
            },
            "rationale": "Documented allergy with severity"
        },
        {
            "entity_group_name": "vitals",
            "entity_name": "blood_pressure",
            "entity_value": "120/80 mmHg",
            "source_text": "BP: 120/80",
            "document_location": {
                "page": 3,
                "section": "Vital Signs"
            }
        }
    ]
}

MALFORMED_JSON_RESPONSE = "This is not valid JSON at all"

TRUNCATED_JSON_RESPONSE = '{"schema_version": "1.0", "document_id": "doc-123", "extracted_entities": ['

JSON_WITH_MARKDOWN_WRAPPER = '''```json
{
    "schema_version": "1.0",
    "document_id": "doc-123",
    "extracted_entities": [
        {
            "entity_group_name": "patient_demographics",
            "entity_name": "name",
            "entity_value": "John Smith"
        }
    ]
}
```'''

JSON_WITH_LEADING_TEXT = '''Here is the extracted data:
{
    "schema_version": "1.0",
    "document_id": "doc-123",
    "extracted_entities": []
}'''

ENTITY_PAYLOAD_MISSING_REQUIRED_FIELDS = {
    "schema_version": "1.0",
    "extracted_entities": []
}

ENTITY_PAYLOAD_WITH_PLACEHOLDER_VALUES = {
    "schema_version": "1.0",
    "document_id": "doc-placeholder-test",
    "extracted_entities": [
        {
            "entity_group_name": "patient_demographics",
            "entity_name": "name",
            "entity_value": "Jane Doe"
        },
        {
            "entity_group_name": "patient_demographics",
            "entity_name": "address",
            "entity_value": "N/A"
        },
        {
            "entity_group_name": "medications",
            "entity_name": "medication_name",
            "entity_value": "Unknown"
        },
        {
            "entity_group_name": "allergies",
            "entity_name": "allergen",
            "entity_value": ""
        }
    ]
}

ENTITY_PAYLOAD_WITH_NONSTANDARD_DATES = {
    "schema_version": "1.0",
    "document_id": "doc-date-test",
    "extracted_entities": [
        {
            "entity_group_name": "patient_demographics",
            "entity_name": "dob",
            "entity_value": "01/15/1990"
        },
        {
            "entity_group_name": "medications",
            "entity_name": "start_date",
            "entity_value": "15-Jan-2024"
        },
        {
            "entity_group_name": "procedures",
            "entity_name": "date",
            "entity_value": "2024-03-20"
        }
    ]
}

ENTITY_PAYLOAD_MISSING_CATEGORIES = {
    "schema_version": "1.0",
    "document_id": "doc-partial",
    "extracted_entities": [
        {
            "entity_group_name": "patient_demographics",
            "entity_name": "name",
            "entity_value": "John Smith"
        },
        {
            "entity_group_name": "medications",
            "entity_name": "medication_name",
            "entity_value": "Lisinopril 10mg"
        }
    ]
}

ENTITY_PAYLOAD_WITH_PARTIAL_DATA = {
    "schema_version": "1.0",
    "document_id": "doc-partial-data",
    "extracted_entities": [
        {
            "entity_group_name": "medications",
            "entity_name": "medication_name",
            "entity_value": "Aspirin"
        },
        {
            "entity_group_name": "lab_results",
            "entity_name": "test_name",
            "entity_value": "Hemoglobin A1C"
        },
        {
            "entity_group_name": "lab_results",
            "entity_name": "value",
            "entity_value": "7.2"
        }
    ]
}

ENTITY_PAYLOAD_WITH_ALIAS_CATEGORIES = {
    "schema_version": "1.0",
    "document_id": "doc-alias-test",
    "extracted_entities": [
        {
            "entity_group_name": "labs",
            "entity_name": "test_name",
            "entity_value": "CBC"
        },
        {
            "entity_group_name": "vitals",
            "entity_name": "bp",
            "entity_value": "120/80"
        },
        {
            "entity_group_name": "meds",
            "entity_name": "medication_name",
            "entity_value": "Metformin"
        }
    ]
}

ENTITY_PAYLOAD_WITH_UNKNOWN_CATEGORY = {
    "schema_version": "1.0",
    "document_id": "doc-unknown-cat",
    "extracted_entities": [
        {
            "entity_group_name": "medications",
            "entity_name": "medication_name",
            "entity_value": "Aspirin"
        },
        {
            "entity_group_name": "unknown_category_xyz",
            "entity_name": "some_field",
            "entity_value": "some_value"
        }
    ]
}

ENTITY_PAYLOAD_WITH_DEPRECATED_CATEGORY = {
    "schema_version": "1.0",
    "document_id": "doc-deprecated",
    "extracted_entities": [
        {
            "entity_group_name": "deprecated_test_category",
            "entity_name": "field",
            "entity_value": "value"
        }
    ]
}
