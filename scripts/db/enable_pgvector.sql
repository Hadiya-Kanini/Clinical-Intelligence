-- ============================================
-- Enable pgvector Extension
-- ============================================
-- This script enables the pgvector extension for vector similarity search.
-- Prerequisites: PostgreSQL 15+ with pgvector extension installed.
-- Run as database owner or superuser.
-- ============================================

-- Enable the vector extension (idempotent)
CREATE EXTENSION IF NOT EXISTS vector;

-- Verify extension is enabled
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'vector') THEN
        RAISE NOTICE 'pgvector extension is enabled successfully.';
    ELSE
        RAISE EXCEPTION 'pgvector extension failed to enable. Ensure pgvector is installed.';
    END IF;
END $$;

-- Display extension version
SELECT extname, extversion 
FROM pg_extension 
WHERE extname = 'vector';
