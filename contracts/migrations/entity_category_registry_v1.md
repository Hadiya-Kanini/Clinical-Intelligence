# Contract Migration: Entity Category Registry v1

## Overview

This migration introduces the entity category registry contract, enabling extraction categories to be defined as data rather than code.

## New Artifacts

| File | Description |
|------|-------------|
| `contracts/entities/v1/entity_categories.schema.json` | JSON schema for validating the category registry structure |
| `contracts/entities/v1/entity_categories.json` | Initial category registry with 10 core categories |

## Schema Version

- **Registry Schema Version**: `1.0`
- **Compatible Entity Schema Version**: `1.0`

## Core Categories Included

1. `patient_demographics` - Patient identifying information (FR-039)
2. `allergies` - Known allergies and reactions (FR-040)
3. `medications` - Current and past medications (FR-041)
4. `diagnoses` - Medical conditions and ICD codes (FR-042)
5. `procedures` - Surgical and medical procedures (FR-043)
6. `lab_results` - Laboratory test results (FR-044)
7. `vital_signs` - Blood pressure, heart rate, etc. (FR-045)
8. `social_history` - Smoking, alcohol, occupation (FR-046)
9. `clinical_notes` - Provider notes and assessments (FR-047)
10. `document_metadata` - Document type, date, provider (FR-048)

## Backward Compatibility

- Existing `entity_group_name` values in extraction payloads remain valid
- Aliases are provided for common alternate names (e.g., `labs` → `lab_results`)
- No breaking changes to `entity.schema.json`

## Consumer Impact

### AI Worker

- **Required**: Load and validate `entity_categories.json` at startup
- **Required**: Use `category_id` values for `entity_group_name` in extraction output
- **Required**: Fail deterministically if registry is missing or invalid

### Backend API

- **Optional**: Validate persisted `Category` values against registry
- **Optional**: Use `display_name` for API responses

### Frontend

- **Optional**: Use `display_name` for user-facing labels
- **Optional**: Filter by `status` to hide deprecated categories

## Migration Steps

1. Deploy new contract files to repository
2. Update worker to load registry at startup
3. Update prompt builder to use registry-driven categories
4. No database migration required

## Rollback

If issues occur:
1. Worker can fall back to hardcoded category list
2. Registry files can be removed without breaking existing functionality
3. No data migration needed for rollback
