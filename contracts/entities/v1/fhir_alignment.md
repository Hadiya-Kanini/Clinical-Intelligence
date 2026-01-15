# FHIR Alignment Mapping Matrix v1

This document defines the canonical mapping between the Clinical Intelligence Platform's patient-centric domain model and HL7 FHIR R4 resources. It serves as the authoritative reference for ensuring database schema and API design remain compatible with future FHIR integration.

## Baseline FHIR Version

- **Target Version**: FHIR R4 (4.0.1)
- **Compatibility Approach**: Design for R4 baseline with forward-compatible extension strategy
- **Upgrade Path**: New FHIR versions will be supported via versioned resource links (see FhirResourceLink)

---

## Patient-Centric Aggregates

### Aggregate Boundaries

| Aggregate Root | Responsibility | Child Entities |
|----------------|----------------|----------------|
| Patient | Demographics, identity matching, consent | Identifiers, ContactPoints, Addresses |
| Encounter | Visit/episode context, timing, location | Participants, Diagnoses (encounter-level) |
| Observation | Labs, vitals, measurements | Components, ReferenceRanges |
| MedicationStatement | Medication history, dosage | Dosages, ReasonCodes |
| Condition | Diagnoses, problems, health concerns | Stages, Evidence |
| Procedure | Clinical procedures performed | Performers, FocalDevices |
| DocumentReference | Ingested document metadata, linking | Content, Context |

### Cross-Aggregate Relationships

```
Patient (1) ──────┬──── (*) Encounter
                  ├──── (*) Observation
                  ├──── (*) MedicationStatement
                  ├──── (*) Condition
                  ├──── (*) Procedure
                  └──── (*) DocumentReference

Encounter (1) ────┬──── (*) Observation (context)
                  ├──── (*) Procedure (context)
                  └──── (*) Condition (encounter diagnosis)
```

---

## Entity-to-FHIR Resource Mapping Matrix

### Patient

| Internal Field | FHIR Resource | FHIR Element Path | Cardinality | Transformation Notes |
|----------------|---------------|-------------------|-------------|---------------------|
| Id | Patient | Patient.id | 1..1 | UUID → FHIR logical id |
| Mrn | Patient | Patient.identifier[mrn] | 0..1 | System: platform MRN namespace |
| GivenName | Patient | Patient.name.given | 0..* | First element of given array |
| FamilyName | Patient | Patient.name.family | 0..1 | Direct mapping |
| DateOfBirth | Patient | Patient.birthDate | 0..1 | Date only (no time) |
| Gender | Patient | Patient.gender | 0..1 | Map to FHIR AdministrativeGender |
| Address | Patient | Patient.address | 0..* | Structured address components |
| Phone | Patient | Patient.telecom[phone] | 0..* | System: phone |
| Email | Patient | Patient.telecom[email] | 0..* | System: email |
| IsActive | Patient | Patient.active | 0..1 | Boolean mapping |
| CreatedAt | Patient | Patient.meta.lastUpdated | 0..1 | Timestamp |
| Extensions | Patient | Patient.extension | 0..* | See Extension Strategy |

### Encounter

| Internal Field | FHIR Resource | FHIR Element Path | Cardinality | Transformation Notes |
|----------------|---------------|-------------------|-------------|---------------------|
| Id | Encounter | Encounter.id | 1..1 | UUID → FHIR logical id |
| PatientId | Encounter | Encounter.subject | 1..1 | Reference(Patient) |
| Status | Encounter | Encounter.status | 1..1 | Map to EncounterStatus |
| Class | Encounter | Encounter.class | 1..1 | Map to ActEncounterCode |
| Type | Encounter | Encounter.type | 0..* | CodeableConcept |
| StartDate | Encounter | Encounter.period.start | 0..1 | DateTime |
| EndDate | Encounter | Encounter.period.end | 0..1 | DateTime |
| ReasonCode | Encounter | Encounter.reasonCode | 0..* | CodeableConcept |
| Location | Encounter | Encounter.location | 0..* | Reference(Location) |
| Extensions | Encounter | Encounter.extension | 0..* | See Extension Strategy |

### Observation

| Internal Field | FHIR Resource | FHIR Element Path | Cardinality | Transformation Notes |
|----------------|---------------|-------------------|-------------|---------------------|
| Id | Observation | Observation.id | 1..1 | UUID → FHIR logical id |
| PatientId | Observation | Observation.subject | 1..1 | Reference(Patient) |
| EncounterId | Observation | Observation.encounter | 0..1 | Reference(Encounter) |
| Status | Observation | Observation.status | 1..1 | Map to ObservationStatus |
| Category | Observation | Observation.category | 0..* | vital-signs, laboratory, etc. |
| Code | Observation | Observation.code | 1..1 | CodeableConcept (LOINC preferred) |
| Value | Observation | Observation.value[x] | 0..1 | Quantity, String, CodeableConcept |
| Unit | Observation | Observation.valueQuantity.unit | 0..1 | UCUM preferred |
| EffectiveDate | Observation | Observation.effective[x] | 0..1 | DateTime or Period |
| Interpretation | Observation | Observation.interpretation | 0..* | High, Low, Normal, etc. |
| ReferenceRange | Observation | Observation.referenceRange | 0..* | Low/High bounds |
| Extensions | Observation | Observation.extension | 0..* | See Extension Strategy |

### MedicationStatement

| Internal Field | FHIR Resource | FHIR Element Path | Cardinality | Transformation Notes |
|----------------|---------------|-------------------|-------------|---------------------|
| Id | MedicationStatement | MedicationStatement.id | 1..1 | UUID → FHIR logical id |
| PatientId | MedicationStatement | MedicationStatement.subject | 1..1 | Reference(Patient) |
| Status | MedicationStatement | MedicationStatement.status | 1..1 | active, completed, etc. |
| MedicationCode | MedicationStatement | MedicationStatement.medication[x] | 1..1 | CodeableConcept (RxNorm) |
| MedicationName | MedicationStatement | MedicationStatement.medicationCodeableConcept.text | 0..1 | Display name |
| Dosage | MedicationStatement | MedicationStatement.dosage | 0..* | Dosage structure |
| EffectiveDate | MedicationStatement | MedicationStatement.effective[x] | 0..1 | DateTime or Period |
| ReasonCode | MedicationStatement | MedicationStatement.reasonCode | 0..* | Why medication taken |
| Extensions | MedicationStatement | MedicationStatement.extension | 0..* | See Extension Strategy |

### Condition

| Internal Field | FHIR Resource | FHIR Element Path | Cardinality | Transformation Notes |
|----------------|---------------|-------------------|-------------|---------------------|
| Id | Condition | Condition.id | 1..1 | UUID → FHIR logical id |
| PatientId | Condition | Condition.subject | 1..1 | Reference(Patient) |
| EncounterId | Condition | Condition.encounter | 0..1 | Reference(Encounter) |
| ClinicalStatus | Condition | Condition.clinicalStatus | 0..1 | active, resolved, etc. |
| VerificationStatus | Condition | Condition.verificationStatus | 0..1 | confirmed, provisional, etc. |
| Category | Condition | Condition.category | 0..* | problem-list-item, encounter-diagnosis |
| Code | Condition | Condition.code | 0..1 | CodeableConcept (ICD-10, SNOMED) |
| OnsetDate | Condition | Condition.onset[x] | 0..1 | DateTime or Period |
| AbatementDate | Condition | Condition.abatement[x] | 0..1 | When resolved |
| Severity | Condition | Condition.severity | 0..1 | Mild, Moderate, Severe |
| Extensions | Condition | Condition.extension | 0..* | See Extension Strategy |

### Procedure

| Internal Field | FHIR Resource | FHIR Element Path | Cardinality | Transformation Notes |
|----------------|---------------|-------------------|-------------|---------------------|
| Id | Procedure | Procedure.id | 1..1 | UUID → FHIR logical id |
| PatientId | Procedure | Procedure.subject | 1..1 | Reference(Patient) |
| EncounterId | Procedure | Procedure.encounter | 0..1 | Reference(Encounter) |
| Status | Procedure | Procedure.status | 1..1 | completed, in-progress, etc. |
| Code | Procedure | Procedure.code | 0..1 | CodeableConcept (CPT, SNOMED) |
| Category | Procedure | Procedure.category | 0..1 | Surgical, diagnostic, etc. |
| PerformedDate | Procedure | Procedure.performed[x] | 0..1 | DateTime or Period |
| ReasonCode | Procedure | Procedure.reasonCode | 0..* | Why performed |
| BodySite | Procedure | Procedure.bodySite | 0..* | Anatomical location |
| Outcome | Procedure | Procedure.outcome | 0..1 | Result of procedure |
| Extensions | Procedure | Procedure.extension | 0..* | See Extension Strategy |

### DocumentReference

| Internal Field | FHIR Resource | FHIR Element Path | Cardinality | Transformation Notes |
|----------------|---------------|-------------------|-------------|---------------------|
| Id | DocumentReference | DocumentReference.id | 1..1 | UUID → FHIR logical id |
| PatientId | DocumentReference | DocumentReference.subject | 0..1 | Reference(Patient) |
| Status | DocumentReference | DocumentReference.status | 1..1 | current, superseded |
| Type | DocumentReference | DocumentReference.type | 0..1 | Document type code |
| Category | DocumentReference | DocumentReference.category | 0..* | Clinical note, report, etc. |
| Date | DocumentReference | DocumentReference.date | 0..1 | When created |
| Description | DocumentReference | DocumentReference.description | 0..1 | Human-readable description |
| ContentType | DocumentReference | DocumentReference.content.attachment.contentType | 0..1 | MIME type |
| ContentUrl | DocumentReference | DocumentReference.content.attachment.url | 0..1 | Storage reference |
| Extensions | DocumentReference | DocumentReference.extension | 0..* | See Extension Strategy |

---

## Relationship Mapping (FHIR References)

| Internal Relationship | FHIR Reference Pattern | Direction |
|-----------------------|------------------------|-----------|
| Observation.PatientId → Patient.Id | Observation.subject → Reference(Patient) | Child → Parent |
| Observation.EncounterId → Encounter.Id | Observation.encounter → Reference(Encounter) | Child → Context |
| Encounter.PatientId → Patient.Id | Encounter.subject → Reference(Patient) | Child → Parent |
| MedicationStatement.PatientId → Patient.Id | MedicationStatement.subject → Reference(Patient) | Child → Parent |
| Condition.PatientId → Patient.Id | Condition.subject → Reference(Patient) | Child → Parent |
| Condition.EncounterId → Encounter.Id | Condition.encounter → Reference(Encounter) | Child → Context |
| Procedure.PatientId → Patient.Id | Procedure.subject → Reference(Patient) | Child → Parent |
| Procedure.EncounterId → Encounter.Id | Procedure.encounter → Reference(Encounter) | Child → Context |
| DocumentReference.PatientId → Patient.Id | DocumentReference.subject → Reference(Patient) | Child → Parent |

---

## Extension Strategy for Unmappable Data

### Approach

Data that does not map cleanly to standard FHIR elements is stored using a structured JSON extension field on each entity. This approach:

1. **Preserves Data Integrity**: No data loss during ingestion
2. **Maintains FHIR Compatibility**: Extensions follow FHIR extensibility patterns
3. **Supports Future Mapping**: Extensions can be promoted to standard fields

### Extension Field Schema

```json
{
  "extensions": [
    {
      "url": "https://clinicalintelligence.platform/fhir/StructureDefinition/{extension-name}",
      "valueType": "string|integer|boolean|dateTime|Quantity|CodeableConcept",
      "value": "<extension-value>"
    }
  ],
  "unmappedData": {
    "<field-name>": "<original-value>",
    "_metadata": {
      "source": "<source-document-id>",
      "extractedAt": "<timestamp>",
      "confidence": 0.95
    }
  }
}
```

### PHI Handling Constraints

- Extension data containing PHI must be encrypted at rest
- Access to extension data follows the same RBAC rules as parent entity
- Audit logging applies to extension field access
- Extension data is included in patient data exports

---

## FHIR Version Evolution Strategy

### Multi-Version Support

The platform supports storing references to multiple FHIR versions via the `FhirResourceLink` entity:

| Field | Description |
|-------|-------------|
| InternalEntityType | Platform entity type (Patient, Observation, etc.) |
| InternalEntityId | Platform entity UUID |
| FhirResourceType | FHIR resource type name |
| FhirResourceId | External FHIR logical id |
| FhirVersion | FHIR version (R4, R5, etc.) |
| SourceSystem | Origin system identifier |
| LastSyncedAt | Last synchronization timestamp |

### Version Upgrade Path

1. **R4 Baseline**: All current mappings target FHIR R4
2. **R5 Preparation**: Extension fields capture R5-specific elements
3. **Dual-Write**: When R5 integration begins, write to both versions
4. **Migration**: Gradual migration with version-specific export endpoints

### Compatibility Rules

- Breaking FHIR changes require new FhirResourceLink entries
- Non-breaking additions use extension promotion
- Deprecated FHIR elements are preserved in unmappedData

---

## Intentional Gaps (Phase 1)

The following FHIR resources are **not** mapped in Phase 1:

| FHIR Resource | Reason | Future Phase |
|---------------|--------|--------------|
| Practitioner | No provider management in Phase 1 | Phase 2 |
| Organization | No organization hierarchy | Phase 2 |
| Location | Simplified encounter context | Phase 2 |
| AllergyIntolerance | Not in initial extraction scope | Phase 2 |
| Immunization | Not in initial extraction scope | Phase 2 |
| CarePlan | No care planning features | Phase 3 |
| Goal | No goal tracking features | Phase 3 |

---

## Validation Checklist

- [x] Phase 1 categories covered: demographics, encounters, observations, medications, diagnoses, procedures
- [x] Relationship mappings consistent with FHIR references
- [x] Extension strategy documented for unmappable data
- [x] FHIR version evolution approach defined
- [x] Multi-version storage strategy specified
- [x] Intentional gaps documented with rationale
