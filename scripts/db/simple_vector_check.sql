-- Simple pgvector verification
SELECT 'Vector extension:' as info, extname FROM pg_extension WHERE extname = 'vector';
SELECT 'Document chunks table:' as info, EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'document_chunks') as exists;
SELECT 'Embedding column:' as info, column_name, data_type FROM information_schema.columns WHERE table_name = 'document_chunks' AND column_name = 'embedding';
SELECT 'Vector test:' as info, '[1,2,3]'::vector as test_vector;
