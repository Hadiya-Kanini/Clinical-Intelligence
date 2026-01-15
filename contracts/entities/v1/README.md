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

## FHIR Alignment

The patient-centric domain model is designed for future FHIR compatibility. See `fhir_alignment.md` for:

- **Mapping Matrix**: Internal entity/field to FHIR resource/element mappings
- **Relationship Mapping**: How internal relationships map to FHIR references
- **Extension Strategy**: Handling data that doesn't map cleanly to FHIR
- **Version Evolution**: Support for multiple FHIR versions (R4 baseline, R5 future)

### Relationship to entity.schema.json

The `entity.schema.json` defines the extraction output contract used by the AI Worker. The FHIR alignment document maps these extracted entities to their corresponding FHIR resources for future integration:

| entity.schema.json Field | FHIR Alignment Target |
|--------------------------|----------------------|
| `entity_group_name` | Maps to FHIR resource category |
| `entity_name` | Maps to FHIR element path |
| `entity_value` | Maps to FHIR element value |
| `additional_entities` | Stored in extension fields |

### Version Management

1. **Schema Version**: `entity.schema.json` version for extraction contract
2. **FHIR Version**: Target FHIR version (R4 baseline) in `fhir_alignment.md`
3. **Domain Model Version**: Migration notes in `contracts/migrations/domain_model_v1.md`
