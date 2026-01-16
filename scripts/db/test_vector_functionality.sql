-- Test pgvector functionality
-- This script tests basic vector operations

-- Test 1: Check if vector extension exists
SELECT 'Vector extension check:' as test;
SELECT extname FROM pg_extension WHERE extname = 'vector';

-- Test 2: Test basic vector creation
SELECT 'Basic vector creation test:' as test;
SELECT '[1,2,3]'::vector as test_vector;

-- Test 3: Test vector operations
SELECT 'Vector operations test:' as test;
SELECT '[1,2,3]'::vector <-> '[4,5,6]'::vector as euclidean_distance;

-- Test 4: Check document_chunks table exists
SELECT 'Document chunks table check:' as test;
SELECT EXISTS (
   SELECT FROM information_schema.tables 
   WHERE table_schema = 'public' 
   AND table_name = 'document_chunks'
) as table_exists;

-- Test 5: Check embedding column exists and is vector type
SELECT 'Embedding column check:' as test;
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'document_chunks' 
AND column_name = 'embedding';
