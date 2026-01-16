-- Validate pgvector setup
-- Check if vector extension is installed
SELECT extname FROM pg_extension WHERE extname = 'vector';

-- Check document_chunks table structure
\d document_chunks

-- Test vector functionality
SELECT '[1,2,3]'::vector as test_vector;

-- Check if we can create a vector index
CREATE INDEX IF NOT EXISTS test_vector_index ON document_chunks USING ivfflat (embedding vector_cosine_ops);

-- List all indexes on document_chunks
SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'document_chunks';
