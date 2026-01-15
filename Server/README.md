# Backend API (.NET)

This directory contains the ASP.NET Core backend API. It handles business logic and data persistence, exposing endpoints for the Web UI and interacting with the AI Worker.

## PostgreSQL Installation and Configuration

### Prerequisites

- **PostgreSQL 15+** (required for pgvector HNSW index support)
- **pgvector extension** (for vector similarity search)
- Minimum **10GB** available disk space

### Installation Steps

#### 1. Install PostgreSQL 15+

Download and install PostgreSQL 15 or later from [postgresql.org](https://www.postgresql.org/download/).

During installation:
- Note the superuser (postgres) password
- Keep the default port (5432) or configure as needed
- Ensure the PostgreSQL service is set to start automatically

#### 2. Install pgvector Extension

For Windows, pgvector must be compiled or installed via a pre-built package:

```powershell
# Option 1: Use pgvector installer (if available for your PostgreSQL version)
# Option 2: Compile from source following https://github.com/pgvector/pgvector

# After installation, verify pgvector is available:
psql -U postgres -c "CREATE EXTENSION vector;" -d postgres
```

#### 3. Validate Prerequisites

Run the pre-flight validation script from the repository root:

```powershell
.\scripts\db\validate_postgresql_prereqs.ps1 -Password "your_postgres_password"
```

This validates:
- PostgreSQL version >= 15
- PostgreSQL service is running
- Port 5432 is accessible
- Credentials are valid
- Disk space >= 10GB

#### 4. Bootstrap Database and User

Run the bootstrap script to create the application database and user:

```powershell
.\scripts\db\install_postgresql.ps1 -AdminPassword "postgres_password" -AppPassword "app_user_password"
```

This creates:
- Database: `ClinicalIntelligence`
- User: `clinicalintelligence_app` (least-privilege)

#### 5. Enable pgvector Extension

Connect to the application database and run:

```powershell
psql -U postgres -d ClinicalIntelligence -f .\scripts\db\enable_pgvector.sql
```

#### 6. Validate pgvector and HNSW Index

Run the validation script to confirm vector operations work:

```powershell
psql -U postgres -d ClinicalIntelligence -f .\scripts\db\validate_pgvector_hnsw.sql
```

This validates:
- `vector(768)` column creation
- HNSW index creation with parameters (m=16, ef_construction=64)
- Cosine similarity operations

### Environment Variables

Set the connection string in your `.env` file:

```
DATABASE_CONNECTION_STRING="Host=localhost;Port=5432;Database=ClinicalIntelligence;Username=clinicalintelligence_app;Password=your_app_password"
```

### Troubleshooting

| Issue | Solution |
|-------|----------|
| PostgreSQL service not running | `Start-Service postgresql*` |
| Connection refused on port 5432 | Check firewall settings and `postgresql.conf` listen_addresses |
| Authentication failed | Verify `pg_hba.conf` allows md5/scram-sha-256 authentication |
| pgvector not found | Ensure pgvector is installed for your PostgreSQL version |

## EF Core Tooling Setup

### Installing dotnet-ef CLI

The `dotnet-ef` CLI tool is required for migration management. Install it globally:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.8
```

To update an existing installation:

```powershell
dotnet tool update --global dotnet-ef --version 8.0.8
```

Verify installation:

```powershell
dotnet ef --version
# Expected output: Entity Framework Core .NET Command-line Tools 8.0.8
```

> **Note:** The dotnet-ef version should match the EF Core package versions in the project (8.0.8) to avoid tooling/runtime mismatches.

### Package Dependencies

The API project includes the following EF Core packages (all aligned to 8.0.8):

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore` | Core EF functionality |
| `Microsoft.EntityFrameworkCore.Design` | Design-time services for migrations |
| `Microsoft.EntityFrameworkCore.Tools` | MSBuild integration |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL provider |

## Database Migrations (EF Core)

The API uses EF Core migrations to version and apply database schema changes.

Migrations are stored under `Server/ClinicalIntelligence.Api/Migrations`.

**Do not apply schema changes by manually editing the database.** All schema changes must be captured as EF Core migrations and committed.

### Create a New Migration

Run from the repository root:

```powershell
dotnet ef migrations add <MigrationName> `
    --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
    --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
    --output-dir Migrations
```

### Apply Migrations

Run from the repository root:

```powershell
dotnet ef database update `
    --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
    --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
```

In Development, the API also applies pending migrations automatically on startup.

### List Migrations

View all applied and pending migrations with timestamps:

```powershell
dotnet ef migrations list `
    --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
    --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
```

### Configure the Connection String

Provide either:

- `ConnectionStrings:DefaultConnection` (recommended)
- `DATABASE_CONNECTION_STRING`

Example (PostgreSQL):

```
DATABASE_CONNECTION_STRING="Host=localhost;Port=5432;Database=ClinicalIntelligence;Username=clinicalintelligence_app;Password=your_password_here"
```

In Development, if neither is provided, the API will default to a local SQLite database at `Data Source=clinicalintelligence.db`.

In non-Development environments, the API will fail fast at startup if no connection string is provided.

## Migration Conflict Resolution

### Branch Conflicts

When multiple developers create migrations on different branches, conflicts may occur. Follow this process:

1. **Identify the conflict**: After merging, if the `ApplicationDbContextModelSnapshot.cs` has merge conflicts or multiple migrations target the same model state, you have a conflict.

2. **Remove the conflicting migration** (on your branch):
   ```powershell
   dotnet ef migrations remove `
       --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
       --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
   ```

3. **Pull the latest changes** from the target branch to get the other developer's migration.

4. **Re-scaffold your migration** with a new name:
   ```powershell
   dotnet ef migrations add <NewMigrationName> `
       --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
       --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
       --output-dir Migrations
   ```

5. **Review the generated migration** to ensure it only contains your intended changes.

### Snapshot Conflicts

If `ApplicationDbContextModelSnapshot.cs` has merge conflicts:

1. Accept the incoming changes (from the target branch).
2. Remove your migration using `dotnet ef migrations remove`.
3. Re-create your migration.

## Migration Failure Recovery

### Partial Migration Execution

If a migration fails mid-execution:

1. **Check the `__EFMigrationsHistory` table** to see which migrations were applied:
   ```sql
   SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC;
   ```

2. **Review PostgreSQL logs** for the specific error:
   - Windows: `C:\Program Files\PostgreSQL\15\data\log\`
   - Check for constraint violations, syntax errors, or timeout issues.

3. **Manual rollback** (if needed): EF Core does not automatically rollback failed migrations. You may need to:
   - Manually revert database changes using SQL scripts.
   - Or restore from a backup taken before the migration.

4. **Fix the migration** and re-apply:
   ```powershell
   dotnet ef database update `
       --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
       --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
   ```

### Common Failure Scenarios

| Issue | Cause | Resolution |
|-------|-------|------------|
| Connection refused | PostgreSQL not running | `Start-Service postgresql*` |
| Authentication failed | Invalid credentials | Verify connection string credentials |
| Permission denied | Insufficient privileges | Ensure user has DDL permissions |
| Timeout during migration | Large data migration | Increase command timeout or batch operations |
| Duplicate key violation | Data constraint conflict | Review and clean conflicting data |

### Safe Retry Guidance

1. **Always backup before migrations** in production environments.
2. **Test migrations locally** against a copy of production data when possible.
3. **Use transactions** where supported (PostgreSQL supports transactional DDL).
4. **Never manually edit `__EFMigrationsHistory`** unless absolutely necessary and with full understanding of consequences.

## US_119 Baseline Schema Migration Strategy

### Strategy Decision: Non-Destructive Transition

The US_119 baseline schema introduces 16 ERD tables alongside the existing FHIR-aligned tables. This non-destructive approach was chosen because:

1. **Existing migrations may be applied**: The FHIR-aligned tables (`patients`, `encounters`, `observations`, etc.) may already exist in deployed environments.
2. **Parallel operation**: Both schema sets can coexist, allowing gradual migration of functionality.
3. **Rollback safety**: The new ERD tables can be dropped without affecting existing FHIR tables.

### ERD Tables (16 tables)

| Table Name | Description |
|------------|-------------|
| `users` | User accounts for authentication |
| `sessions` | Session tracking for authenticated users |
| `password_reset_tokens` | Secure password reset tokens |
| `erd_patients` | Patient records per ERD specification |
| `document_batches` | Grouped document uploads |
| `documents` | Uploaded clinical documents |
| `processing_jobs` | Document processing status tracking |
| `document_chunks` | Text segments with embeddings (vector(768)) |
| `extracted_entities` | AI-extracted clinical entities |
| `entity_citations` | Source citations for extracted entities |
| `conflicts` | Data conflict tracking |
| `conflict_resolutions` | Conflict resolution records |
| `billing_code_catalog_items` | ICD/CPT code reference catalog |
| `code_suggestions` | AI-suggested billing codes |
| `audit_log_events` | Security and compliance audit trail |
| `vector_query_logs` | AI assistant query logging |

### Naming Conventions

- **Table names**: snake_case, plural (e.g., `users`, `sessions`, `document_chunks`)
- **Column names**: PascalCase (EF Core convention)
- **Index names**: `ix_{table}_{column}` pattern
- **Foreign key names**: `FK_{table}_{related_table}_{column}` pattern

### pgvector Extension Handling

**Strategy**: Prerequisite script for production, auto-create for development.

- **Development**: The migration includes `HasPostgresExtension("vector")` which creates the extension automatically.
- **Production**: Run `scripts/db/enable_pgvector.sql` before applying migrations if the database user lacks `CREATE EXTENSION` privileges.

```powershell
# For restricted environments, run as superuser first:
psql -U postgres -d ClinicalIntelligence -f .\scripts\db\enable_pgvector.sql

# Then apply migrations as application user:
dotnet ef database update `
    --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
    --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
```

### Idempotent Script Generation

For deployment pipelines that require idempotent SQL scripts:

```powershell
dotnet ef migrations script --idempotent `
    --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
    --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj `
    --output deploy_schema.sql
```

The generated script uses `IF NOT EXISTS` checks and migration history validation to ensure safe re-application.

### Schema Validation Commands

Verify tables exist after migration:

```sql
-- List all ERD tables
\dt users
\dt sessions
\dt password_reset_tokens
\dt erd_patients
\dt document_batches
\dt documents
\dt processing_jobs
\dt document_chunks
\dt extracted_entities
\dt entity_citations
\dt conflicts
\dt conflict_resolutions
\dt billing_code_catalog_items
\dt code_suggestions
\dt audit_log_events
\dt vector_query_logs

-- Verify vector column type
\d document_chunks
-- Should show: Embedding | vector(768)

-- Verify unique email constraint
\d users
-- Should show: ix_users_email UNIQUE
```

### Rollback Guidance

If migration fails partially:

1. Check `__EFMigrationsHistory` for applied migrations
2. Review PostgreSQL logs for specific errors
3. For ERD tables, the Down migration drops all 16 tables in reverse dependency order
4. Manual rollback: `dotnet ef database update <previous_migration_name>`

## US_121 Index Build and Maintenance Guidance

### Index Inventory (US_121)

| Index Name | Table | Column(s) | Type | Purpose |
|------------|-------|-----------|------|---------|
| `ix_users_email` | users | Email | B-tree (unique) | Authentication email lookups (AC-1) |
| `ix_documents_patient_id_uploaded_at` | documents | PatientId, UploadedAt | B-tree (composite) | Document listing with date filtering (AC-2) |
| `ix_processing_jobs_status` | processing_jobs | Status | B-tree | Job queue queries by status (AC-3) |
| `ix_extracted_entities_patient_id` | extracted_entities | PatientId | B-tree | Entity aggregation for Patient 360 (AC-4) |
| `ix_audit_log_events_timestamp` | audit_log_events | Timestamp | B-tree | Audit log date range queries (AC-5) |
| `ix_document_chunks_embedding_hnsw` | document_chunks | Embedding | HNSW (pgvector) | Vector similarity search (AC-6) |

### Index Build Progress Monitoring

For long-running index builds (especially on large tables), monitor progress using:

```sql
-- Monitor CREATE INDEX progress
SELECT 
    p.pid,
    p.datname,
    p.phase,
    p.lockers_total,
    p.lockers_done,
    p.blocks_total,
    p.blocks_done,
    p.tuples_total,
    p.tuples_done,
    ROUND(100.0 * p.blocks_done / NULLIF(p.blocks_total, 0), 2) AS blocks_pct,
    ROUND(100.0 * p.tuples_done / NULLIF(p.tuples_total, 0), 2) AS tuples_pct
FROM pg_stat_progress_create_index p
JOIN pg_stat_activity a ON p.pid = a.pid;
```

**Expected phases:**
1. `initializing` - Setting up index build
2. `waiting for writers before build` - Waiting for concurrent transactions
3. `building index` - Main index construction (longest phase)
4. `waiting for writers before validation` - Final consistency check
5. `index validation: scanning index` - Validating index entries
6. `index validation: sorting tuples` - Sorting for validation
7. `index validation: scanning table` - Final table scan
8. `waiting for old snapshots` - Waiting for old transactions to complete

### Production Index Build (CONCURRENTLY)

For production deployments with large tables, use `CREATE INDEX CONCURRENTLY` to avoid blocking writes:

```sql
-- Production-safe HNSW index creation (run outside EF migration)
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_document_chunks_embedding_hnsw 
ON document_chunks USING hnsw ("Embedding" vector_cosine_ops) 
WITH (m = 16, ef_construction = 64);
```

**Important:** `CREATE INDEX CONCURRENTLY` cannot run inside a transaction. If using EF migrations:
1. Apply the migration without the HNSW index
2. Run the HNSW index creation separately via psql or a deployment script

**Runtime factors affecting index build:**
- Table size (row count and data volume)
- I/O throughput (SSD vs HDD)
- Available CPU cores
- `maintenance_work_mem` setting (increase for faster builds)

```sql
-- Temporarily increase memory for index build (session-level)
SET maintenance_work_mem = '1GB';
```

### HNSW Index Parameter Tuning

The HNSW index uses two build-time parameters:

| Parameter | Default | Description |
|-----------|---------|-------------|
| `m` | 16 | Maximum number of connections per layer. Higher = better recall, more memory |
| `ef_construction` | 64 | Size of dynamic candidate list during build. Higher = better quality, slower build |

**Tuning guidance:**

| Scenario | m | ef_construction | Notes |
|----------|---|-----------------|-------|
| Default (balanced) | 16 | 64 | Good recall/performance balance |
| High recall | 32 | 128 | Better accuracy, 2x memory |
| Memory constrained | 8 | 32 | Reduced memory, lower recall |
| OOM during build | 8 | 32 | Reduce parameters or increase memory |

**Query-time parameter (`ef_search`):**

```sql
-- Increase search quality (default: 40)
SET hnsw.ef_search = 100;

-- Check current setting
SHOW hnsw.ef_search;
```

Higher `ef_search` improves recall but increases query latency. Tune based on your accuracy/latency requirements.

### Index Health and Usage Monitoring

Monitor index usage to identify unused or underperforming indexes:

```sql
-- Index usage statistics
SELECT 
    schemaname,
    relname AS table_name,
    indexrelname AS index_name,
    idx_scan AS index_scans,
    idx_tup_read AS tuples_read,
    idx_tup_fetch AS tuples_fetched,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY idx_scan DESC;

-- Identify unused indexes (0 scans since last stats reset)
SELECT 
    relname AS table_name,
    indexrelname AS index_name,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes
WHERE idx_scan = 0 AND schemaname = 'public'
ORDER BY pg_relation_size(indexrelid) DESC;
```

### Index Bloat and REINDEX Guidance

Over time, indexes can become bloated due to updates and deletes. Monitor and maintain:

```sql
-- Estimate index bloat (simplified)
SELECT 
    nspname AS schema,
    relname AS index_name,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size,
    pg_size_pretty(pg_relation_size(indrelid)) AS table_size
FROM pg_stat_user_indexes
JOIN pg_index ON indexrelid = pg_index.indexrelid
WHERE schemaname = 'public'
ORDER BY pg_relation_size(pg_stat_user_indexes.indexrelid) DESC;
```

**REINDEX guidance:**

```sql
-- Rebuild a specific index (blocks writes briefly)
REINDEX INDEX ix_documents_patient_id_uploaded_at;

-- Rebuild all indexes on a table
REINDEX TABLE documents;

-- Production-safe rebuild (PostgreSQL 12+)
REINDEX INDEX CONCURRENTLY ix_documents_patient_id_uploaded_at;
```

**When to REINDEX:**
- Index bloat exceeds 30-40% of expected size
- Query performance degrades over time
- After large bulk delete operations
- During scheduled maintenance windows

### ANALYZE Guidance

If the query planner ignores indexes (uses sequential scans when index scans are expected):

```sql
-- Update table statistics
ANALYZE documents;
ANALYZE document_chunks;
ANALYZE extracted_entities;
ANALYZE audit_log_events;
ANALYZE processing_jobs;

-- Update all tables
ANALYZE;

-- Check when tables were last analyzed
SELECT 
    relname AS table_name,
    last_analyze,
    last_autoanalyze,
    n_live_tup AS live_rows,
    n_dead_tup AS dead_rows
FROM pg_stat_user_tables
WHERE schemaname = 'public'
ORDER BY relname;
```

**When to run ANALYZE:**
- After bulk data loads
- After large UPDATE/DELETE operations
- When query plans unexpectedly change
- If `n_dead_tup` is high relative to `n_live_tup`

### Validation Scripts Reference

The repository includes validation scripts for pgvector and HNSW indexes:

```powershell
# Validate pgvector extension and HNSW index functionality
psql -U postgres -d ClinicalIntelligence -f .\scripts\db\validate_pgvector_hnsw.sql
```

## Secret rotation

Secrets are loaded at startup from the configured sources (environment variables and, in Development, user-secrets). To rotate a secret:

1. Update the value in the configuration source (e.g., environment variable or user-secrets).
2. Restart the API process.
