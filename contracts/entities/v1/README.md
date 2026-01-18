# Entity Extraction Result Contract v1

This document defines version 1 of the entity extraction output schema validated/produced by the AI Worker.

## Schema Location

The canonical JSON schema is located at `contracts/entities/v1/entity.schema.json`.

## Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `schema_version` | string | Yes | The version of the entity output schema. Use `"1.0"` for backward compatibility or `"1.1"` for grounded entities with required citations. |
| `document_id` | string | Yes | Identifier for the document processed. |
| `extracted_entities` | array | Yes | List of extracted entities. |
| `additional_entities` | object | No | Extension point for additional extracted data not yet standardized. |

## Schema Versions

### Version 1.0 (Backward Compatible)

- Citation fields (`document_location`, `source_text`) are **optional**
- Suitable for development, testing, or scenarios where grounding is not enforced
- Existing consumers continue to work without modification

### Version 1.1 (Grounded Entities - Production)

- Citation fields are **required** for every extracted entity
- Enforces FR-051 (100% grounding), FR-056 (reject ungrounded), TR-008 (citation fields)
- **Required fields per entity:**
  - `source_text` (non-empty string) - The cited text from the source document
  - `document_location.page` (integer >= 1) - Page number in source document
  - `document_location.section` (non-empty string) - Section identifier
  - `document_location.coordinates` - Object with `x`, `y`, `width`, `height` (all numbers)

### Valid Citation Definition

A citation is considered valid when it includes:

| Field | Type | Requirement |
|---|---|---|
| `source_text` | string | Non-empty (`minLength: 1`) |
| `document_location.page` | integer | >= 1 |
| `document_location.section` | string | Non-empty (`minLength: 1`) |
| `document_location.coordinates.x` | number | Required |
| `document_location.coordinates.y` | number | Required |
| `document_location.coordinates.width` | number | Required |
| `document_location.coordinates.height` | number | Required |

## Consumer Responsibilities

### Worker

1. **Emit `schema_version: "1.1"`** once grounding enforcement is enabled for production
2. Ensure every extracted entity includes valid citation fields
3. Validate output against the schema before publishing

### Backend

1. **Reject/avoid persistence** if citations are missing when `schema_version: "1.1"` (FR-056)
2. Store citation metadata for Patient 360 source references
3. Validate incoming payloads match expected schema version

### Frontend

1. Display source citations for each entity
2. Provide clickable reference links using `document_location` metadata

## Error Handling

### Consumer / Validator

- **Missing Required Fields**: If an entity payload is missing one or more required fields, it is considered invalid.
- **Unknown Schema Version**: If the `schema_version` is not recognized, the payload must be rejected deterministically so that retries can be handled safely.
- **Missing Citations (v1.1)**: If `schema_version` is `"1.1"` and any entity lacks required citation fields, the payload must be rejected.

### Versioning Rules

- The directory name (`v1`) indicates the major version.
- The in-schema `schema_version` follows semantic versioning. Breaking changes require a new major version directory.
- `1.0` → `1.1` is a **non-breaking** change for consumers that don't validate citations
- `1.1` is a **stricter** validation mode that enforces grounding requirements

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

## Entity Category Registry

The `entity_categories.json` file defines extraction categories as data, enabling extensibility without code changes.

### Category Registry Structure

| Field | Type | Required | Description |
|---|---|---|---|
| `schema_version` | string | Yes | Registry schema version (currently `"1.0"`). |
| `categories` | array | Yes | List of category definitions. |

### Category Definition

| Field | Type | Required | Description |
|---|---|---|---|
| `category_id` | string | Yes | Stable identifier used as `entity_group_name`. Must be unique. |
| `display_name` | string | Yes | Human-readable display name. |
| `description` | string | No | Description of the category. |
| `status` | string | Yes | `active` or `deprecated`. |
| `aliases` | array | No | Alternate IDs that resolve to this canonical category. |
| `recommended_entity_names` | array | No | Suggested `entity_name` values for this category. |

### Extensibility Rules

1. **Adding Categories**: Add a new entry to `categories[]` with a unique `category_id`. No code changes required.
2. **Deprecating Categories**: Set `status` to `deprecated`. Deprecated categories are still accepted but flagged.
3. **Aliases**: Use `aliases` to support legacy or alternate category names that resolve to the canonical ID.

### Conflict Handling

- Registry load **must fail deterministically** if:
  - Two categories have the same `category_id`
  - An alias equals another category's `category_id`
  - An alias is used by multiple categories

### Consumer Responsibilities

- **Worker**: Load and validate the registry at startup. Use `category_id` values for `entity_group_name`.
- **Backend**: Validate that persisted `Category` values match known `category_id` values.
- **UI**: Use `display_name` for user-facing labels.
