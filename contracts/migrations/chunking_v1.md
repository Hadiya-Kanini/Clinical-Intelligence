# Chunking Contract Migration Notes - v1

## Version: 1.0

## Date: 2026-01-16

## Type: Initial Release (Major)

## Changes
- Introduced first versioned chunked text schema (1.0)
- Defined required fields: schema_version, patient_id, chunks
- Established chunk structure with chunk_index, text, token_count, chunk_hash
- Added provenance array supporting multi-document chunks
- Included chunking_config for TR-005 compliance documentation
- Created JSON Schema draft-07 compliant structure

## Impact
- No breaking changes (initial release)
- All chunked text messages must include schema_version field set to "1.0"
- AI Worker must produce messages conforming to this schema after semantic chunking
- Downstream embedding generation must consume and validate messages against this schema
- Document provenance (document_id) is required for each chunk's provenance entries

## Migration Steps
1. Review chunked text schema at contracts/chunking/v1/chunked_text.schema.json
2. AI Worker: Implement semantic chunking producing chunks with provenance
3. AI Worker: Set chunk_index sequentially starting from 0
4. AI Worker: Compute token_count using consistent tokenizer
5. Downstream embedding: Handle chunks with multiple provenance entries (cross-document)
6. Test with multi-document patient data to validate provenance preservation

## Required Actions
- **AI Worker**: Implement chunking step consuming merged text and producing chunked text
- **Embedding Service**: Validate incoming chunks against this schema before processing
- **Storage Layer**: Prepare to receive provenance arrays with multiple entries per chunk

## Related Changes
- Text merge contract v1.0 provides input format for chunking
- Embeddings contract v1.0 (upcoming) will consume chunked text output
