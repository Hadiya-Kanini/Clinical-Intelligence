# Task - TASK_003

## Requirement Reference
- User Story: us_119
- Story Location: .propel/context/tasks/us_119/us_119.md
- Acceptance Criteria: 
    - AC-2: Given tables are created, When schema is inspected, Then all columns match ERD specifications with correct PostgreSQL data types (uuid, varchar, text, timestamp, jsonb, vector(768))
    - AC-3: Given DOCUMENT_CHUNK table exists, When a 768-dimensional embedding vector is stored, Then the vector is persisted and retrievable correctly

## Task Overview
Enable pgvector support and configure the `DOCUMENT_CHUNK.embedding` column as `vector(768)` (including EF Core mappings) so embeddings can be stored and retrieved reliably.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_119/task_002_backend_define_core_entities_and_generate_baseline_migration_for_16_tables.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Migrations/*
- scripts/db/enable_pgvector.sql

## Implementation Plan
- Confirm the chosen pgvector strategy from TASK_001 (auto-create extension in migration vs fail fast with prerequisite guidance).
- Ensure the Postgres database has the `vector` extension available (align with `scripts/db/enable_pgvector.sql`).
- Configure EF Core mapping for the embedding column:
  - Use the appropriate Npgsql/pgvector integration for `vector(768)`.
  - Ensure migrations create the `vector(768)` column in `DOCUMENT_CHUNK`.
- Add a minimal persistence validation path:
  - Save a 768-length vector to `DOCUMENT_CHUNK.embedding` and read it back.
  - Validate round-trip shape and expected serialization/mapping behavior.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Add any required pgvector-related package(s) compatible with EF Core 8 + Npgsql 8 if not already present |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Configure mapping for `DocumentChunk.Embedding` to PostgreSQL `vector(768)` |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/* | Add migration updates to enable pgvector extension (per strategy) and set `DOCUMENT_CHUNK.embedding` to `vector(768)` |
| MODIFY | scripts/db/enable_pgvector.sql | Ensure script aligns with the chosen strategy and can be used as a prerequisite for environments that do not allow CREATE EXTENSION in migrations |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.npgsql.org/efcore/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify `DOCUMENT_CHUNK.embedding` is created as `vector(768)` in PostgreSQL.
- Verify a 768-dimensional vector can be written and read back without truncation or type errors.

## Implementation Checklist
- [x] Confirm and document pgvector enablement approach (migration-managed vs prerequisite script)
- [x] Add/verify required packages for pgvector + Npgsql EF Core integration
- [x] Configure EF Core mapping for `DocumentChunk.Embedding` to `vector(768)`
- [x] Update or generate migration changes to create the `vector(768)` column
- [x] Apply migrations to a local Postgres instance with pgvector enabled
- [x] Validate a 768-length embedding persists and round-trips correctly
