# Contract Migration: Entity Schema v1.1 - Grounding Required

## Overview

This migration introduces `schema_version: "1.1"` to the entity extraction contract, enforcing 100% grounding for extracted entities. This change supports FR-051, FR-056, NFR-006, and TR-008.

## What Changed

### Schema Version Enum

- **Before**: `schema_version` only accepted `"1.0"`
- **After**: `schema_version` accepts `"1.0"` or `"1.1"`

### Conditional Validation (v1.1)

When `schema_version` is `"1.1"`, the following fields become **required** for each extracted entity:

| Field | Requirement |
|---|---|
| `source_text` | Non-empty string (`minLength: 1`) |
| `document_location.page` | Integer >= 1 |
| `document_location.section` | Non-empty string (`minLength: 1`) |
| `document_location.coordinates.x` | Number (required) |
| `document_location.coordinates.y` | Number (required) |
| `document_location.coordinates.width` | Number (required) |
| `document_location.coordinates.height` | Number (required) |

### Backward Compatibility

- `schema_version: "1.0"` behavior is **unchanged**
- Existing consumers that only use v1.0 require no modifications
- v1.1 is opt-in and should only be used when grounding is enforced

## Compatibility Notes

### Breaking Changes

- **None for v1.0 consumers**: Existing payloads with `schema_version: "1.0"` continue to validate
- **v1.1 is stricter**: Payloads using `schema_version: "1.1"` will fail validation if any entity lacks required citation fields

### Non-Breaking Changes

- Adding `"1.1"` to the enum is additive
- New definitions (`GroundedExtractedEntity`, `GroundedDocumentLocation`) do not affect v1.0 validation

## Required Rollout Steps

### 1. Worker Updates

1. Update `_load_entity_schema()` to accept `"1.1"` as a valid version
2. Implement `grounding_validator.py` to enforce citation requirements beyond schema validation
3. Wire grounding validation into the worker flow after JSON Schema validation
4. Update prompt builder to instruct LLM to include citation fields
5. **Emit `schema_version: "1.1"`** once grounding is enforced

### 2. Backend Updates

1. Update entity payload validation to handle v1.1
2. Implement rejection logic for ungrounded entities when `schema_version: "1.1"`
3. Persist citation metadata (`EntityCitation` table) for grounded entities
4. Update Patient 360 API to include citation details in response

### 3. Testing Requirements

- Validate that v1.0 payloads continue to pass
- Validate that v1.1 payloads missing `source_text` fail
- Validate that v1.1 payloads missing `document_location.coordinates` fail
- Validate that v1.1 payloads with empty `source_text` fail
- Validate that fully grounded v1.1 payloads pass

## Rollback Plan

If issues are discovered:

1. Worker can revert to emitting `schema_version: "1.0"`
2. Backend can accept both versions and only enforce grounding for v1.1
3. No database migrations are required for rollback

## Timeline

| Phase | Action | Owner |
|---|---|---|
| Phase 1 | Schema update deployed | Contracts |
| Phase 2 | Worker grounding validator implemented | Worker |
| Phase 3 | Backend citation persistence enabled | Backend |
| Phase 4 | Worker emits v1.1 in production | Worker |
| Phase 5 | Backend enforces grounding rejection | Backend |

## Related Requirements

- **FR-051**: Every extracted entity MUST cite source text from the document
- **FR-056**: Prevent hallucination by rejecting entities without valid source citations
- **NFR-006**: 100% grounding - no uncited entities displayed/stored as final
- **TR-008**: Persist and surface page/section/coordinates + source text for every entity

## Contact

For questions about this migration, refer to the US_069 user story documentation.
