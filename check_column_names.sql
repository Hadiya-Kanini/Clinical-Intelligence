-- Check actual column names in extracted_entities table
SELECT column_name 
FROM information_schema.columns 
WHERE table_name = 'extracted_entities' 
ORDER BY ordinal_position;
