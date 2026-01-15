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
