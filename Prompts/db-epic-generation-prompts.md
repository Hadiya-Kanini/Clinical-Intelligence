# Database Epic Generation Prompts

**Purpose:** Use these prompts to generate EP-DB-001: Database Infrastructure & Schema Initialization  
**Usage:** Copy each prompt and use it sequentially to generate the database setup epic  
**Target File:** `.propel/context/docs/epics.md`

---

## Prompt 1: Detect Database Requirements from models.md

```
Analyze the file .propel/context/docs/models.md and extract:

1. All table definitions with columns and data types from the ERD (Logical Data Model section)
2. All foreign key relationships between tables
3. All unique constraints (UK markers)
4. All indexes mentioned in design.md (DR-011)
5. pgvector extension requirements (look for 'vector' data type)

List all 16 table names and confirm pgvector is required.
```

---

## Prompt 2: Map Database Requirements to Epic

```
From .propel/context/docs/design.md, identify all DR-XXX, TR-XXX, and NFR-XXX requirements related to:

- Database installation and configuration (PostgreSQL, pgvector)
- Schema creation and migrations (Entity Framework Core)
- Database security (encryption, row-level security)
- Connection pooling and performance
- Backup and restore procedures
- Seed data requirements (static admin account)

List all requirement IDs (DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014) with their descriptions.
```

---

## Prompt 3: Generate Database Setup Epic

```
Create EP-DB-001 epic with the following structure:

**Title:** "Database Infrastructure & Schema Initialization"

**Priority:** Critical (Blocking) - must execute after EP-TECH

**Business Value:** Establishes the foundational data layer required for all application features; enables secure, performant, and compliant data storage for PHI.

**Description:** Set up PostgreSQL database with pgvector extension, initialize Entity Framework Core Migrations, create baseline schema for 16 tables from ERD, implement indexing strategy, configure connection pooling, seed static admin account, and establish backup/restore procedures.

**Schema Evolution Note:** The ERD in models.md serves as the baseline schema for Phase 1. Developers may add/modify entities and fields during implementation as needed via EF Core Migrations. All schema changes must be documented in migration comments and models.md must be updated post-implementation to reflect the actual schema.

**Key Deliverables:**
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

**Mapped Requirements:**
- DR-001: FHIR-aligned patient-centric schema (baseline, extensible)
- DR-002: Referential integrity enforcement
- DR-003: Immutable audit log storage
- DR-011: Database indexing strategy
- DR-012: Connection string encryption
- NFR-015: Connection pooling (min 10, max 100)
- TR-014: Static admin account initialization

Generate the complete epic description in markdown format.
```

---

## Prompt 4: Insert Epic into Correct Position

```
Modify the file .propel/context/docs/epics.md:

1. Locate the "Epic Summary Table" section
2. Find the row for EP-TECH
3. Insert a new row immediately after EP-TECH with:
   - Epic ID: EP-DB-001
   - Epic Title: Database Infrastructure & Schema Initialization
   - Mapped Requirement IDs: DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014

4. Locate the "Epic Description" section
5. Find the description for EP-TECH
6. Insert the complete EP-DB-001 description (from Prompt 3) immediately after EP-TECH

Do NOT renumber existing epics (EP-001 through EP-035 keep their numbers).

Show me the updated Epic Summary Table with EP-DB-001 inserted.
```

---

## Prompt 5: Validate Epic Coverage

```
Verify that the newly created EP-DB-001 in .propel/context/docs/epics.md covers:

1. All 16 tables from models.md ERD:
   - USER, SESSION, PASSWORD_RESET_TOKEN
   - PATIENT, DOCUMENT_BATCH, DOCUMENT
   - PROCESSING_JOB, DOCUMENT_CHUNK
   - EXTRACTED_ENTITY, ENTITY_CITATION
   - CONFLICT, CONFLICT_RESOLUTION
   - BILLING_CODE_CATALOG_ITEM, CODE_SUGGESTION
   - AUDIT_LOG_EVENT, VECTOR_QUERY_LOG

2. All database-related DR requirements:
   - DR-001, DR-002, DR-003, DR-011, DR-012

3. Static admin account initialization (TR-014)

4. Connection pooling (NFR-015)

5. Schema flexibility note is included

6. No requirement duplication with other epics (check EP-TECH and EP-035)

Provide a validation report with PASS/FAIL status.
```

---

## Quick Reference: Complete Epic Text

Use this if you want to directly insert the epic:

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

---

## Epic Summary Table Entry

Insert this row after EP-TECH in the Epic Summary Table:

```markdown
| EP-DB-001 | Database Infrastructure & Schema Initialization | DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014 |
```

---

## Usage Instructions

1. **Run Prompt 1** - Verify models.md has 16 tables and pgvector
2. **Run Prompt 2** - Confirm all database requirements are identified
3. **Run Prompt 3** - Generate the complete epic description
4. **Run Prompt 4** - Insert epic into epics.md (both table and description)
5. **Run Prompt 5** - Validate coverage and check for issues

**Alternative:** Use the "Quick Reference" section to directly copy/paste the epic into epics.md

---

**Document Created:** 2025-01-14  
**Status:** Ready to Use  
**Target:** Generate EP-DB-001 in `.propel/context/docs/epics.md`
