-- Create missing database constraints for pgvector integration
-- Run this script in PostgreSQL to fix the constraint error

-- 1. Ensure pgvector extension is installed
CREATE EXTENSION IF NOT EXISTS vector;

-- 2. Create the missing unique constraint for document_chunks table
-- This constraint is required for the ON CONFLICT clause in DocumentChunkStore
ALTER TABLE document_chunks 
ADD CONSTRAINT unique_document_chunk 
UNIQUE ("DocumentId", "ChunkHash");

-- 3. Create HNSW index for fast similarity search (if not exists)
CREATE INDEX IF NOT EXISTS idx_document_chunks_embedding 
ON document_chunks USING hnsw ("Embedding" vector_cosine_ops)
WITH (m = 16, ef_construction = 64);

-- 4. Verify the constraint was created
SELECT conname, contype 
FROM pg_constraint 
WHERE conrelid = 'document_chunks'::regclass 
AND contype = 'u';

-- 5. Check pgvector extension
SELECT extname, extversion 
FROM pg_extension 
WHERE extname = 'vector';
