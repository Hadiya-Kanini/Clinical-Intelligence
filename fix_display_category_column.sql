-- Add missing DisplayCategory column to extracted_entities table
-- This column is needed for the entity storage functionality

ALTER TABLE extracted_entities 
ADD COLUMN "DisplayCategory" VARCHAR(255);

-- Add comment to describe the column
COMMENT ON COLUMN extracted_entities."DisplayCategory" IS 'Mapped category name for frontend display';
