# Entity Extraction Result Contract v1

This document defines version 1 of the entity extraction output schema validated/produced by the AI Worker.

## Schema Location

The canonical JSON schema is located at `contracts/entities/v1/entity.schema.json`.

## Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `schema_version` | string | Yes | The version of the entity output schema. For v1, this must be `"1.0"`. |
| `document_id` | string | Yes | Identifier for the document processed. |
| `extracted_entities` | array | Yes | List of extracted entities. |
| `additional_entities` | object | No | Extension point for additional extracted data not yet standardized. |

## Error Handling

### Consumer / Validator

- **Missing Required Fields**: If an entity payload is missing one or more required fields, it is considered invalid.
- **Unknown Schema Version**: If the `schema_version` is not recognized, the payload must be rejected deterministically so that retries can be handled safely.

### Versioning Rules

- The directory name (`v1`) indicates the major version.
- The in-schema `schema_version` follows semantic versioning. Breaking changes require a new major version directory.
