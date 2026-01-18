-- Migration: Make PatientId nullable in documents and document_batches tables
-- This allows documents to be uploaded without pre-existing patient information
-- Patient information will be extracted from documents during processing

BEGIN;

-- Make PatientId nullable in documents table
ALTER TABLE documents 
ALTER COLUMN "PatientId" DROP NOT NULL;

-- Make PatientId nullable in document_batches table
ALTER TABLE document_batches 
ALTER COLUMN "PatientId" DROP NOT NULL;

-- Add comment to document the change
COMMENT ON COLUMN documents."PatientId" IS 'Reference to patient - nullable until extracted from document during processing';
COMMENT ON COLUMN document_batches."PatientId" IS 'Reference to patient - nullable until extracted from documents during processing';

COMMIT;
