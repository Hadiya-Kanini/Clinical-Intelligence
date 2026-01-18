# Embeddings Contract Migration Notes - v1

## Version: 1.0

## Date: 2026-01-16

## Type: Initial Release (Major)

## Changes
- Introduced first versioned embedding result schema (1.0)
- Defined required fields: schema_version, patient_id, results
- Established result item structure with chunk_index, status, embedding, normalized
- Added 768-dimensional embedding vector constraint
- Included provenance array for chunk-to-document traceability
- Added partial failure support with error_code and error_message fields
- Created JSON Schema draft-07 compliant structure with conditional validation

## Impact
- No breaking changes (initial release)
- All embedding result messages must include schema_version field set to "1.0"
- AI Worker must produce messages conforming to this schema after embedding generation
- Downstream storage must consume and validate messages against this schema
- Successful results require embedding (768 floats) and normalized (boolean) fields
- Failed results should include error_code and error_message for debugging

## Migration Steps
1. Review embedding result schema at contracts/embeddings/v1/embedding_result.schema.json
2. AI Worker: Implement embedding generation producing results with status field
3. AI Worker: Apply L2 normalization and set normalized=true for 768-d vectors
4. AI Worker: Handle API errors with retry and produce failed results for permanent failures
5. Storage: Validate incoming results against schema before processing
6. Storage: Handle mixed success/failed results in batch processing
7. Test with various chunk batches including partial failure scenarios

## Required Actions
- **AI Worker**: Implement embedding generation step consuming chunked text
- **Storage Layer**: Validate incoming embedding results against this schema
- **Storage Layer**: Prepare to handle partial failures (some chunks failed)
- **Monitoring**: Set up alerts for high failure rates in embedding results

## Related Changes
- Chunking contract v1.0 provides input format for embedding generation
- Database schema should include pgvector column for 768-d vectors
