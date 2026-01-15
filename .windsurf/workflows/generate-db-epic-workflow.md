---
description: Generates EP-DB-001 Database Infrastructure & Schema Initialization epic from models.md ERD and design.md requirements. Inserts into epics.md after EP-TECH.
auto_execution_mode: 1
---

# Database Epic Generator

As an expert Database Architect and Infrastructure Engineer, generate EP-DB-001: Database Infrastructure & Schema Initialization epic by analyzing the ERD in models.md and database requirements in design.md.

## Input Parameter: $ARGUMENTS (Optional)
**Accepts:** File path to models.md or empty for default
**Default:** `.propel/context/docs/models.md`

## Output
- Artifact generation: `.propel/context/docs/epics.md` (updated with EP-DB-001)
- Print:
  - Epic Summary (EP-DB-001 details)
  - Tables covered (16 tables)
  - Requirements mapped (DR-XXX, TR-XXX, NFR-XXX)
  - Validation status

## Execution Flow

### 1. Read Source Documents
- Read `.propel/context/docs/models.md` - Extract ERD with 16 tables
- Read `.propel/context/docs/design.md` - Extract database requirements
- Read `.propel/context/docs/epics.md` - Locate insertion point

### 2. Extract Database Requirements from models.md

**Extract from ERD (Logical Data Model section):**
- Count total tables (should be 16)
- List all table names:
  - USER, SESSION, PASSWORD_RESET_TOKEN
  - PATIENT, DOCUMENT_BATCH, DOCUMENT
  - PROCESSING_JOB, DOCUMENT_CHUNK
  - EXTRACTED_ENTITY, ENTITY_CITATION
  - CONFLICT, CONFLICT_RESOLUTION
  - BILLING_CODE_CATALOG_ITEM, CODE_SUGGESTION
  - AUDIT_LOG_EVENT, VECTOR_QUERY_LOG
- Identify pgvector requirement (DOCUMENT_CHUNK.embedding column)
- Extract foreign key relationships
- Extract unique constraints (UK markers)

### 3. Extract Database Requirements from design.md

**Extract these requirements:**
- DR-001: FHIR-aligned patient-centric schema
- DR-002: Referential integrity enforcement
- DR-003: Immutable audit log storage
- DR-011: Database indexing strategy (7 indexes)
- DR-012: Connection string encryption
- NFR-015: Connection pooling (min 10, max 100)
- TR-014: Static admin account initialization

**Extract indexing requirements from DR-011:**
- user.email
- document.patient_id
- document.upload_date
- processing_job.status
- extracted_entity.patient_id
- audit_log.timestamp
- pgvector HNSW index on embeddings

### 4. Generate EP-DB-001 Epic

**Epic Structure:**

```markdown
### EP-DB-001: Database Infrastructure & Schema Initialization
**Priority**: Critical (Blocking)  
**Business Value**: Establishes the foundational data layer required for all application features; enables secure, performant, and compliant data storage for PHI.  
**Description**: Set up PostgreSQL database with pgvector extension, initialize Entity Framework Core Migrations, create baseline schema for 16 tables from ERD, implement indexing strategy, configure connection pooling, seed static admin account, and establish backup/restore procedures.

**Schema Evolution Note**: The ERD in models.md serves as the baseline schema for Phase 1. Developers may add/modify entities and fields during implementation as needed via EF Core Migrations. All schema changes must be documented in migration comments and models.md must be updated post-implementation to reflect the actual schema.

**Key Deliverables**:
- PostgreSQL 15+ installation with pgvector extension
- Entity Framework Core Migrations framework initialization
- Baseline migration creating 16 tables: USER, SESSION, PASSWORD_RESET_TOKEN, PATIENT, DOCUMENT_BATCH, DOCUMENT, PROCESSING_JOB, DOCUMENT_CHUNK, EXTRACTED_ENTITY, ENTITY_CITATION, CONFLICT, CONFLICT_RESOLUTION, BILLING_CODE_CATALOG_ITEM, CODE_SUGGESTION, AUDIT_LOG_EVENT, VECTOR_QUERY_LOG
- Foreign key constraints and referential integrity enforcement
- Database indexing strategy (DR-011): indexes on user.email, document.patient_id, processing_job.status, extracted_entity.patient_id, audit_log.timestamp, pgvector HNSW index
- Connection pooling configuration (min 10, max 100 connections)
- Static admin account seed data from environment variables
- Database backup/restore procedures with 30-day retention
- Connection string encryption in configuration files
- Row-level security setup for document_chunks table
- Migration strategy documentation for schema evolution
```

**Mapped Requirements:**
- DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014

### 5. Insert into epics.md

**Step 1: Update Epic Summary Table**
- Locate "Epic Summary Table" section
- Find row for EP-TECH
- Insert new row immediately after EP-TECH:
```markdown
| EP-DB-001 | Database Infrastructure & Schema Initialization | DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014 |
```

**Step 2: Add Epic Description**
- Locate "Epic Description" section
- Find "### EP-TECH:" description
- Insert complete EP-DB-001 description immediately after EP-TECH

**DO NOT renumber existing epics** (EP-001 through EP-035 keep their IDs)

### 6. Validate Epic Coverage

**Validation Checklist:**
- [ ] All 16 tables from models.md listed in Key Deliverables
- [ ] pgvector extension mentioned
- [ ] All 7 indexes from DR-011 listed
- [ ] Static admin seed data included (TR-014)
- [ ] Connection pooling config included (NFR-015)
- [ ] Schema flexibility note present
- [ ] All DR requirements mapped (DR-001, DR-002, DR-003, DR-011, DR-012)
- [ ] No requirement duplication with other epics

### 7. Summary Presentation

Print to console:
```
EP-DB-001 Generation Summary
============================

Epic ID: EP-DB-001
Title: Database Infrastructure & Schema Initialization
Position: After EP-TECH, before EP-001
Priority: Critical (Blocking)

Tables Covered: 16/16
- USER, SESSION, PASSWORD_RESET_TOKEN
- PATIENT, DOCUMENT_BATCH, DOCUMENT
- PROCESSING_JOB, DOCUMENT_CHUNK
- EXTRACTED_ENTITY, ENTITY_CITATION
- CONFLICT, CONFLICT_RESOLUTION
- BILLING_CODE_CATALOG_ITEM, CODE_SUGGESTION
- AUDIT_LOG_EVENT, VECTOR_QUERY_LOG

Requirements Mapped: 7
- DR-001: FHIR-aligned schema
- DR-002: Referential integrity
- DR-003: Immutable audit logs
- DR-011: Indexing strategy (7 indexes)
- DR-012: Connection string encryption
- NFR-015: Connection pooling (10-100)
- TR-014: Static admin seed

Indexes Covered: 7/7
- user.email
- document.patient_id
- document.upload_date
- processing_job.status
- extracted_entity.patient_id
- audit_log.timestamp
- pgvector HNSW on embeddings

Schema Flexibility: ✓ Included
Validation Status: PASS

Epic inserted into: .propel/context/docs/epics.md
Position: After EP-TECH
```

## Guardrails
- `rules/ai-assistant-usage-policy.md`: Explicit commands; minimal output
- `rules/dry-principle-guidelines.md`: Single source of truth
- `rules/markdown-styleguide.md`: Front matter, heading hierarchy

---

*This workflow generates EP-DB-001 database setup epic from models.md ERD and inserts it into epics.md after EP-TECH.*
