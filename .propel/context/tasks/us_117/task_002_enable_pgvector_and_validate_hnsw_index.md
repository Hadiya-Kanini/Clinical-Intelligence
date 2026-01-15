# Task - TASK_002

## Requirement Reference
- User Story: us_117
- Story Location: .propel/context/tasks/us_117/us_117.md
- Acceptance Criteria: 
    - AC-2: Given PostgreSQL is running, When the pgvector extension is enabled via `CREATE EXTENSION vector`, Then the vector data type and cosine similarity operations are available
    - AC-3: Given pgvector is enabled, When an HNSW index is created on a 768-dimensional vector column, Then the index is created successfully with configurable m and ef_construction parameters

## Task Overview
Enable the pgvector extension in the target PostgreSQL database and provide repeatable validation scripts to confirm vector type availability and successful HNSW index creation for 768-dimensional embeddings.
Estimated Effort: 4 hours

## Dependent Tasks
- .propel/context/tasks/us_117/task_001_install_postgresql_15_and_configure_service.md (TASK_001)

## Impacted Components
- scripts/
- Server/README.md

## Implementation Plan
- Add a database bootstrap script to enable pgvector using `CREATE EXTENSION IF NOT EXISTS vector`.
- Add a validation script that:
  - Confirms the `vector` extension is installed/enabled
  - Creates a small validation table with a `vector(768)` column
  - Attempts HNSW index creation with parameters (defaults m=16, ef_construction=64) and validates success
  - Includes a negative-path validation for invalid HNSW parameters and prints recommended defaults
- Document how/when to run the scripts and how this relates to EF Core migrations that rely on `vector(768)`.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | scripts/db/enable_pgvector.sql | Enables pgvector extension in the target database using `CREATE EXTENSION IF NOT EXISTS vector` |
| CREATE | scripts/db/validate_pgvector_hnsw.sql | Validates vector type availability and HNSW index creation for `vector(768)` with configurable parameters |
| MODIFY | Server/README.md | Document pgvector enablement + HNSW validation steps and prerequisites for vector-based migrations |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://github.com/pgvector/pgvector
- https://github.com/pgvector/pgvector#hnsw

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify `enable_pgvector.sql` succeeds on a clean database and `SELECT extname FROM pg_extension` includes `vector`.
- Verify `validate_pgvector_hnsw.sql` successfully creates a `vector(768)` column and an HNSW index.
- Verify invalid HNSW parameter inputs produce a clear error and the script outputs recommended parameter defaults.

## Implementation Checklist
- [x] Add `enable_pgvector.sql` for idempotent extension enablement
- [x] Add `validate_pgvector_hnsw.sql` for vector type + cosine ops + HNSW index validation
- [x] Ensure validation uses a 768-dimensional vector column and parameterized HNSW settings
- [x] Add negative-path validation output for invalid parameters with recommended defaults (m=16, ef_construction=64)
- [x] Update `Server/README.md` with pgvector enablement + validation workflow
