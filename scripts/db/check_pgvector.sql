-- Check if pgvector extension is installed
SELECT extname FROM pg_extension WHERE extname = 'vector';

-- Check document_chunks table structure
\d document_chunks

-- Test vector functionality
SELECT '[1,2,3]'::vector as test_vector;

-- Show all tables with vector columns
SELECT table_name, column_name, data_type 
FROM information_schema.columns 
WHERE data_type = 'vector' OR data_type LIKE 'vector%';
