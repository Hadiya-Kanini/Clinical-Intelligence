-- ============================================
-- Validate pgvector and HNSW Index Creation
-- ============================================
-- This script validates:
-- 1. pgvector extension is installed
-- 2. vector(768) column creation works
-- 3. HNSW index creation with configurable parameters
-- 4. Cosine similarity operations work
-- ============================================

-- Configuration: HNSW index parameters
-- Recommended defaults: m=16, ef_construction=64
-- Adjust based on your use case:
--   - Higher m = better recall, more memory, slower builds
--   - Higher ef_construction = better index quality, slower builds
\set hnsw_m 16
\set hnsw_ef_construction 64

-- ============================================
-- Step 1: Verify pgvector extension
-- ============================================
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'vector') THEN
        RAISE EXCEPTION 'pgvector extension is not enabled. Run enable_pgvector.sql first.';
    END IF;
    RAISE NOTICE 'Step 1: pgvector extension verified.';
END $$;

-- ============================================
-- Step 2: Create validation table with vector(768) column
-- ============================================
DROP TABLE IF EXISTS _pgvector_validation_test CASCADE;

CREATE TABLE _pgvector_validation_test (
    id SERIAL PRIMARY KEY,
    embedding vector(768) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

DO $$
BEGIN
    RAISE NOTICE 'Step 2: Created validation table with vector(768) column.';
END $$;

-- ============================================
-- Step 3: Insert test vectors
-- ============================================
-- Insert sample 768-dimensional vectors for testing
INSERT INTO _pgvector_validation_test (embedding)
SELECT 
    (SELECT array_agg(random())::vector(768) FROM generate_series(1, 768))
FROM generate_series(1, 10);

DO $$
DECLARE
    row_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO row_count FROM _pgvector_validation_test;
    RAISE NOTICE 'Step 3: Inserted % test vectors.', row_count;
END $$;

-- ============================================
-- Step 4: Create HNSW index with configurable parameters
-- ============================================
-- Using cosine distance operator for semantic similarity
CREATE INDEX idx_pgvector_validation_hnsw 
ON _pgvector_validation_test 
USING hnsw (embedding vector_cosine_ops)
WITH (m = 16, ef_construction = 64);

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE indexname = 'idx_pgvector_validation_hnsw'
    ) THEN
        RAISE NOTICE 'Step 4: HNSW index created successfully (m=16, ef_construction=64).';
    ELSE
        RAISE EXCEPTION 'HNSW index creation failed.';
    END IF;
END $$;

-- ============================================
-- Step 5: Validate cosine similarity operations
-- ============================================
DO $$
DECLARE
    query_vector vector(768);
    result_count INTEGER;
BEGIN
    -- Generate a random query vector
    SELECT array_agg(random())::vector(768) INTO query_vector FROM generate_series(1, 768);
    
    -- Test cosine distance query (1 - cosine_similarity)
    SELECT COUNT(*) INTO result_count
    FROM _pgvector_validation_test
    ORDER BY embedding <=> query_vector
    LIMIT 5;
    
    IF result_count > 0 THEN
        RAISE NOTICE 'Step 5: Cosine similarity search works. Found % nearest neighbors.', result_count;
    ELSE
        RAISE EXCEPTION 'Cosine similarity search returned no results.';
    END IF;
END $$;

-- ============================================
-- Step 6: Display index information
-- ============================================
SELECT 
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = '_pgvector_validation_test';

-- ============================================
-- Step 7: Cleanup validation table
-- ============================================
DROP TABLE IF EXISTS _pgvector_validation_test CASCADE;

DO $$
BEGIN
    RAISE NOTICE 'Step 7: Cleanup complete. Validation table removed.';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'pgvector HNSW Validation: ALL CHECKS PASSED';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Recommended HNSW parameters:';
    RAISE NOTICE '  m = 16 (connections per layer)';
    RAISE NOTICE '  ef_construction = 64 (build-time search depth)';
    RAISE NOTICE '  ef_search = 40 (query-time search depth, set at runtime)';
    RAISE NOTICE '';
    RAISE NOTICE 'Example index creation for your tables:';
    RAISE NOTICE 'CREATE INDEX idx_your_table_embedding';
    RAISE NOTICE 'ON your_table USING hnsw (embedding vector_cosine_ops)';
    RAISE NOTICE 'WITH (m = 16, ef_construction = 64);';
END $$;

-- ============================================
-- Negative Path Validation: Invalid Parameters
-- ============================================
-- Uncomment to test invalid parameter handling:
-- 
-- The following would fail with clear errors:
-- 
-- Invalid m (must be >= 2):
-- CREATE INDEX idx_invalid_m ON _test USING hnsw (embedding vector_cosine_ops) WITH (m = 1);
-- Error: m must be greater than or equal to 2
--
-- Invalid ef_construction (must be >= 1):
-- CREATE INDEX idx_invalid_ef ON _test USING hnsw (embedding vector_cosine_ops) WITH (ef_construction = 0);
-- Error: ef_construction must be greater than or equal to 1
--
-- Recommended defaults if you encounter issues:
--   m = 16 (balanced recall/performance)
--   ef_construction = 64 (good index quality)
