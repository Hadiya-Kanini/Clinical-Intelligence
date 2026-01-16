-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create document_chunks table
CREATE TABLE IF NOT EXISTS document_chunks (
    "Id" uuid NOT NULL,
    "DocumentId" uuid NOT NULL,
    "Page" integer,
    "Section" character varying(100),
    "Coordinates" character varying(100),
    "TextContent" text NOT NULL,
    "Embedding" vector(768),
    "TokenCount" integer,
    "ChunkHash" character varying(64),
    CONSTRAINT "PK_document_chunks" PRIMARY KEY ("Id")
);

-- Create entity_citations table
CREATE TABLE IF NOT EXISTS entity_citations (
    "Id" uuid NOT NULL,
    "ExtractedEntityId" uuid NOT NULL,
    "DocumentChunkId" uuid NOT NULL,
    "Page" integer,
    "Section" character varying(100),
    "Coordinates" character varying(100),
    "CitedText" text,
    CONSTRAINT "PK_entity_citations" PRIMARY KEY ("Id")
);

-- Create vector_query_logs table
CREATE TABLE IF NOT EXISTS vector_query_logs (
    "Id" uuid NOT NULL,
    "UserId" uuid,
    "PatientId" uuid,
    "QueryText" text NOT NULL,
    "ResultCount" integer NOT NULL,
    "ResponseTimeMs" integer,
    "Timestamp" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "QueryHash" character varying(64),
    CONSTRAINT "PK_vector_query_logs" PRIMARY KEY ("Id")
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "ix_document_chunks_document_id" ON document_chunks ("DocumentId");
CREATE INDEX IF NOT EXISTS "ix_document_chunks_chunk_hash" ON document_chunks ("ChunkHash");
CREATE INDEX IF NOT EXISTS "ix_entity_citations_extracted_entity_id" ON entity_citations ("ExtractedEntityId");
CREATE INDEX IF NOT EXISTS "ix_entity_citations_document_chunk_id" ON entity_citations ("DocumentChunkId");
CREATE INDEX IF NOT EXISTS "ix_vector_query_logs_user_id" ON vector_query_logs ("UserId");
CREATE INDEX IF NOT EXISTS "ix_vector_query_logs_patient_id" ON vector_query_logs ("PatientId");
CREATE INDEX IF NOT EXISTS "ix_vector_query_logs_timestamp" ON vector_query_logs ("Timestamp");
CREATE INDEX IF NOT EXISTS "ix_vector_query_logs_query_hash" ON vector_query_logs ("QueryHash");

-- Add foreign key constraints
ALTER TABLE document_chunks 
ADD CONSTRAINT IF NOT EXISTS "FK_document_chunks_documents_DocumentId" 
FOREIGN KEY ("DocumentId") REFERENCES documents("Id") ON DELETE CASCADE;

ALTER TABLE entity_citations 
ADD CONSTRAINT IF NOT EXISTS "FK_entity_citations_extracted_entities_ExtractedEntityId" 
FOREIGN KEY ("ExtractedEntityId") REFERENCES extracted_entities("Id") ON DELETE CASCADE;

ALTER TABLE entity_citations 
ADD CONSTRAINT IF NOT EXISTS "FK_entity_citations_document_chunks_DocumentChunkId" 
FOREIGN KEY ("DocumentChunkId") REFERENCES document_chunks("Id") ON DELETE CASCADE;

ALTER TABLE vector_query_logs 
ADD CONSTRAINT IF NOT EXISTS "FK_vector_query_logs_users_UserId" 
FOREIGN KEY ("UserId") REFERENCES users("Id") ON DELETE SET NULL;

ALTER TABLE vector_query_logs 
ADD CONSTRAINT IF NOT EXISTS "FK_vector_query_logs_erd_patients_PatientId" 
FOREIGN KEY ("PatientId") REFERENCES erd_patients("Id") ON DELETE SET NULL;