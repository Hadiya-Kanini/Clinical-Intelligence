# Domain Model Migration Notes v1

This document provides migration notes and compatibility guidance for the patient-centric domain model. It ensures schema changes remain auditable, intentional, and backward-compatible.

## Contract Information

- **Contract**: Patient-Centric Domain Model
- **Version**: 1.0.0
- **Change Type**: Initial Release
- **FHIR Baseline**: R4 (4.0.1)

---

## [1.0.0] - Initial Release

### Description

Initial implementation of the patient-centric domain model aligned with HL7 FHIR R4. Establishes core entities for clinical data management with future EHR integration readiness.

### Entities Introduced

| Entity | Purpose | FHIR Resource |
|--------|---------|---------------|
| Patient | Demographics, identity matching | Patient |
| Encounter | Visit/episode context | Encounter |
| Observation | Labs, vitals, measurements | Observation |
| MedicationStatement | Medication history | MedicationStatement |
| Condition | Diagnoses, problems | Condition |
| Procedure | Clinical procedures | Procedure |
| DocumentReference | Ingested document metadata | DocumentReference |
| FhirResourceLink | External FHIR resource linking | N/A (platform-specific) |

### Relationships Established

```
Patient (1) ──┬── (*) Encounter
              ├── (*) Observation
              ├── (*) MedicationStatement
              ├── (*) Condition
              ├── (*) Procedure
              ├── (*) DocumentReference
              └── (*) FhirResourceLink

Encounter (1) ─┬── (*) Observation (optional context)
               ├── (*) Procedure (optional context)
               └── (*) Condition (encounter diagnosis)
```

### Impact

- **New Consumers**: No migration required; adopt entities as defined
- **Existing Consumers**: N/A (initial release)
- **Database**: New tables created via EF Core migration

### Required Actions

1. Apply EF Core migration `AddPatientCentricDomainModel`
2. Verify foreign key constraints are created correctly
3. Validate indexes for patient identity matching (MRN, Name + DOB)

---

## Schema Evolution Guidelines

### Adding New Fields

1. **Optional Fields**: Add as nullable columns; no migration impact
2. **Required Fields**: Provide default value or backfill strategy
3. **FHIR-Mapped Fields**: Update `fhir_alignment.md` with new mappings

### Adding New Entities

1. Create entity class following existing patterns
2. Add DbSet to ApplicationDbContext
3. Configure relationships in OnModelCreating
4. Update `fhir_alignment.md` with FHIR mapping
5. Generate EF Core migration
6. Add migration note to this document

### Modifying Relationships

1. Document the change rationale
2. Assess cascade behavior impact
3. Plan data migration if needed
4. Update `fhir_alignment.md` relationship section
5. Add migration note with rollback strategy

### Breaking Changes

Breaking changes require:

1. New major version directory (`v2/`)
2. Deprecation notice in current version
3. Migration path documentation
4. Consumer notification timeline

---

## Extension Field Guidelines

### Purpose

Extension fields (`Extensions` JSON column) store:

- Data not mapping to standard FHIR elements
- Source-specific metadata
- Extraction confidence scores
- Unmapped clinical data

### Schema

```json
{
  "extensions": [
    {
      "url": "https://clinicalintelligence.platform/fhir/StructureDefinition/{name}",
      "valueType": "string",
      "value": "example"
    }
  ],
  "unmappedData": {
    "fieldName": "originalValue",
    "_metadata": {
      "source": "document-uuid",
      "extractedAt": "2024-01-01T00:00:00Z",
      "confidence": 0.95
    }
  }
}
```

### Promotion to Standard Fields

When extension data becomes standardized:

1. Add new column to entity
2. Migrate data from extension JSON
3. Update `fhir_alignment.md` mapping
4. Remove from extension (optional, for cleanup)
5. Document in migration notes

---

## FHIR Version Compatibility

### Current State

- **Baseline**: FHIR R4 (4.0.1)
- **Storage**: Internal UUIDs with optional FHIR resource links
- **Export**: R4-compatible output format

### Future R5 Support

When R5 integration is required:

1. Add R5-specific FhirResourceLink entries
2. Dual-write during transition period
3. Version-specific export endpoints
4. Gradual migration with compatibility layer

### Multi-Version Storage

The `FhirResourceLink` entity supports multiple FHIR versions per internal entity:

```
Patient (internal)
  └── FhirResourceLink (R4, external-system-a)
  └── FhirResourceLink (R5, external-system-b)
```

---

## Rollback Procedures

### Migration Rollback

```bash
dotnet ef migrations remove
dotnet ef database update <previous-migration>
```

### Data Rollback

1. Restore from backup (preferred)
2. Execute reverse data migration script
3. Verify referential integrity

### Emergency Procedures

1. Stop application services
2. Execute rollback migration
3. Verify database state
4. Restart services with previous code version
5. Document incident and root cause

---

## Audit Trail

| Date | Version | Author | Change Summary |
|------|---------|--------|----------------|
| 2024-01-01 | 1.0.0 | System | Initial patient-centric domain model |

---

## Related Documents

- `contracts/entities/v1/fhir_alignment.md` - FHIR mapping matrix
- `contracts/entities/v1/entity.schema.json` - Entity extraction schema
- `contracts/entities/v1/README.md` - Entity contract overview
