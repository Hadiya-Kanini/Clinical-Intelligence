# Job Contract Migration Notes - v1

## Version: 1.0

## Date: 2026-01-13

## Type: Initial Release (Major)

## Changes
- Introduced first versioned job schema (1.0)
- Defined required fields: schema_version, job_id, document_id, status
- Established status lifecycle values: pending, processing, completed, failed, validation_failed
- Created JSON Schema draft-07 compliant structure
- Added optional payload field for extensibility

## Impact
- No breaking changes (initial release)
- All job messages must include schema_version field set to "1.0"
- Backend API must produce messages conforming to this schema
- AI Worker must consume and validate messages against this schema
- Status transitions must follow defined lifecycle

## Migration Steps
1. Review job schema at contracts/jobs/v1/job.schema.json
2. Backend API: Implement job message producer with required fields
3. AI Worker: Implement job message consumer with schema validation
4. Configure RabbitMQ queue for job message exchange
5. Implement retry logic for failed jobs with status tracking
6. Test end-to-end job processing workflow

---

## Version: 1.1

## Date: 2026-01-16

## Type: Backward-Compatible (Minor)

## Changes
- Added optional payload.storage_path field for document extraction routing
- Added optional payload.mime_type field for extraction strategy selection (PDF vs DOCX)
- Added optional payload.patient_id field for patient-level multi-document merge operations
- Added optional payload.document_ids array for multi-document processing
- Added optional payload.patient_identifiers object for MRN or name+DOB matching
- Extended schema_version enum to include "1.1"

## Impact
- Backward-compatible: existing v1.0 producers continue to work
- New producers can use v1.1 features for text extraction and patient merge workflows
- AI Worker must handle both v1.0 and v1.1 payloads
- Backend API can optionally populate new fields when enqueueing extraction or merge jobs

## Migration Steps
1. Review updated job schema at contracts/jobs/v1/job.schema.json
2. Backend API: Update job producer to include storage_path and mime_type for extraction jobs
3. AI Worker: Implement extraction routing based on payload.mime_type
4. For patient merge: Backend API populates patient_id or patient_identifiers + document_ids
5. AI Worker: Implement multi-document merge when document_ids is present
6. Test backward compatibility with existing v1.0 job payloads
