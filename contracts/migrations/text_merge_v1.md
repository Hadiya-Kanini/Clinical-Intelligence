# Text Merge Contract Migration Notes - v1

## Version: 1.0

## Date: 2026-01-16

## Type: Initial Release (Major)

## Changes
- Introduced first versioned merged text schema (1.0)
- Defined required fields: schema_version, patient_id, source_documents, merged_segments
- Established segment structure with document_id provenance and positional metadata
- Added is_document_boundary flag for cross-document chunk handling
- Created JSON Schema draft-07 compliant structure

## Impact
- No breaking changes (initial release)
- All merged text messages must include schema_version field set to "1.0"
- AI Worker must produce messages conforming to this schema after multi-document merge
- Downstream chunking must consume and validate messages against this schema
- Document provenance (document_id) is required for each segment

## Migration Steps
1. Review merged text schema at contracts/text_merge/v1/merged_text.schema.json
2. AI Worker: Implement multi-document merge producing segments with document_id
3. AI Worker: Set is_document_boundary=true for first segment of each document
4. Downstream chunking: Handle cross-document chunks using provenance metadata
5. Test with multiple documents per patient to validate merge ordering

## Related Changes
- Job schema v1.1 adds optional payload.patient_id, payload.document_ids, and payload.patient_identifiers for merge orchestration
