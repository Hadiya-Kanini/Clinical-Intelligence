# Text Extraction Contract Migration Notes - v1

## Version: 1.0

## Date: 2026-01-16

## Type: Initial Release (Major)

## Changes
- Introduced first versioned extracted text schema (1.0)
- Defined required fields: schema_version, document_id, segments
- Established segment structure with text content and optional positional metadata
- Added document_location object supporting page, section, and coordinates
- Created JSON Schema draft-07 compliant structure

## Impact
- No breaking changes (initial release)
- All extracted text messages must include schema_version field set to "1.0"
- AI Worker must produce messages conforming to this schema after text extraction
- Backend API must consume and validate messages against this schema
- Positional metadata (page, section, coordinates) is optional to support various document types

## Migration Steps
1. Review extracted text schema at contracts/text_extraction/v1/extracted_text.schema.json
2. AI Worker: Implement text extraction producing segments with positional metadata
3. Backend API: Implement consumer that maps segments to DocumentChunk rows
4. Validate page/section/coordinates preservation through the pipeline
5. Test with PDF documents (page metadata expected) and DOCX documents (section metadata best-effort)

## Related Changes
- Job schema v1.1 adds optional payload.storage_path and payload.mime_type for extraction routing
