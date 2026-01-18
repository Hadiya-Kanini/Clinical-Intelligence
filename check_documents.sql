-- Check Documents table structure and existing records
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'documents' 
ORDER BY ordinal_position;

-- Check if there are any existing documents
SELECT COUNT(*) as document_count FROM "documents";
