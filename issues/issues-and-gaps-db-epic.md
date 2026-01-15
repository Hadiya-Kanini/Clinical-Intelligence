# Issues and Gaps Analysis: Database Setup Epic

**Document Created:** 2025-01-14  
**Status:** Phase 1 Complete - Ready for Review  
**Purpose:** Document missing database setup epic and required prompts for workflow enhancement

---

## Executive Summary

**Critical Gap Identified:** The current epic generation workflow (`create-epics.md`) successfully generates 35 epics from spec.md and design.md, but **completely omits a dedicated epic for database setup and initialization**. This creates a significant implementation blocker as database infrastructure is a foundational prerequisite for all subsequent development work.

**Impact:** Without a DB setup epic, developers lack clear guidance on:
- PostgreSQL installation and configuration
- pgvector extension setup
- Database migration framework initialization  
- Initial schema creation from ERD (16 tables defined in models.md)
- Static admin account seed data
- Database security hardening

**Recommendation:** Create **EP-DB-001: Database Infrastructure & Schema Initialization** as a new epic that should be executed immediately after EP-TECH.

---

## Problem-Solution Mapping (Excel Format)

**CSV File:** `.propel/context/docs/issues-and-gaps-db-epic.csv`

A detailed Excel-compatible spreadsheet documenting:
- **10 Gaps Identified** (GAP-001 through GAP-010)
- **What We Were Facing** - Original problem description
- **Impact** - Business/technical impact of the gap
- **Root Cause** - Why the gap exists
- **What We Are Doing to Fix** - Specific solution
- **Solution Approach** - Implementation method
- **Expected Outcome** - Success criteria
- **Status** - Current state (all Documented)

### Gap Summary

| Gap ID | Problem | Solution |
|--------|---------|----------|
| GAP-001 | No PostgreSQL installation guidance | EP-DB-001 with installation steps |
| GAP-002 | No EF Core Migrations setup | Include migrations in EP-DB-001 |
| GAP-003 | 16 tables not covered | Baseline migration for all tables |
| GAP-004 | DR-011 indexes not mapped | Include 7 indexes in EP-DB-001 |
| GAP-005 | No static admin seed | Add seed data to EP-DB-001 |
| GAP-006 | NFR-015 pooling not mapped | Configure pooling in EP-DB-001 |
| GAP-007 | Database security scattered | Consolidate in EP-DB-001 |
| GAP-008 | No backup procedures | Add backup strategy to EP-DB-001 |
| GAP-009 | ERD too rigid | Add schema flexibility note |
| GAP-010 | Database not prioritized | Position EP-DB-001 after EP-TECH |

**Open CSV in Excel:** Double-click the file to view full details in spreadsheet format

---

## Current State Analysis

### Existing Workflows
✅ **design-model.md** - Generates ERD with 16 database tables in models.md  
✅ **create-epics.md** - Generates 35 epics (EP-TECH through EP-035) from requirements  
✅ **models.md** - Contains complete ERD with all tables, relationships, and constraints

### Existing Epic Coverage
- **EP-TECH**: Project scaffolding, API versioning, error formats (TR-001, TR-015, TR-016, NFR-010, DR-012)
- **EP-035**: Core Domain Data Model & Referential Integrity (DR-001, DR-002)
- **EP-004**: Input Validation & Credential Hardening (includes password hashing)
- **EP-030**: Immutable Audit Framework (DR-003, NFR-007)

### What's Missing
❌ No epic for PostgreSQL installation and configuration  
❌ No epic for pgvector extension setup  
❌ No epic for Entity Framework Core Migrations initialization  
❌ No epic for initial schema creation from ERD  
❌ No epic for database indexing strategy implementation (DR-011)  
❌ No epic for connection pooling configuration (NFR-015)  
❌ No epic for static admin account seed data (TR-014)  
❌ No epic for database backup/restore procedures  
❌ No epic for connection string encryption (DR-012)

---

## Detailed Gap Analysis

### Gap 1: Database Installation & Configuration
**Requirements Not Mapped:**
- PostgreSQL 15+ installation
- pgvector extension installation
- Database user/role creation
- Connection pooling (NFR-015: min 10, max 100 connections)

**Current Epic Coverage:** None

**Impact:** Developers don't know how to set up the database environment

---

### Gap 2: Migration Framework Setup
**Requirements Not Mapped:**
- Entity Framework Core Migrations initialization
- Migration versioning strategy
- Rollback procedures

**Current Epic Coverage:** Partially in EP-TECH (mentions migrations) but no dedicated epic

**Impact:** No clear process for schema evolution

---

### Gap 3: Initial Schema Creation
**Requirements Not Mapped:**
- Create 16 tables from ERD in models.md:
  - USER, SESSION, PASSWORD_RESET_TOKEN
  - PATIENT, DOCUMENT_BATCH, DOCUMENT
  - PROCESSING_JOB, DOCUMENT_CHUNK
  - EXTRACTED_ENTITY, ENTITY_CITATION
  - CONFLICT, CONFLICT_RESOLUTION
  - BILLING_CODE_CATALOG_ITEM, CODE_SUGGESTION
  - AUDIT_LOG_EVENT, VECTOR_QUERY_LOG
- Foreign key constraints
- Unique constraints
- Check constraints

**Current Epic Coverage:** EP-035 mentions "FHIR-aligned schema" but doesn't detail table creation

**Impact:** No actionable guidance for implementing the ERD

---

### Gap 4: Database Indexing Strategy
**Requirements Not Mapped:**
- DR-011: Create indexes on:
  - user.email
  - document.patient_id, document.upload_date
  - processing_job.status
  - extracted_entity.patient_id
  - audit_log.timestamp
  - pgvector HNSW index on embeddings

**Current Epic Coverage:** None

**Impact:** Performance degradation without proper indexes

---

### Gap 5: Static Admin Account Seed Data
**Requirements Not Mapped:**
- TR-014: Static admin account initialization
- FR-010a: Pre-configured admin account during DB setup
- FR-010b: Admin credentials from environment variables
- FR-010c: Prevent deletion of static admin

**Current Epic Coverage:** EP-009 mentions "static admin account initialization" but focuses on user provisioning workflow, not DB seed

**Impact:** No admin user to bootstrap the system

---

### Gap 6: Database Security Hardening
**Requirements Not Mapped:**
- DR-012: Connection string encryption
- DR-004: Embedding encryption at rest
- DR-005: Row-level security for document chunks
- Database SSL/TLS configuration

**Current Epic Coverage:** Scattered across multiple epics but no dedicated DB security epic

**Impact:** Security vulnerabilities in database layer

---

## Required Prompts for New Workflow

### Prompt 1: Detect Database Requirements from models.md
```
Analyze models.md ERD and extract:
1. All table definitions with columns and data types
2. All foreign key relationships
3. All unique constraints
4. All indexes mentioned in design.md (DR-011)
5. pgvector extension requirements
```

### Prompt 2: Map Database Requirements to Epic
```
From design.md, identify all DR-XXX, TR-XXX, and NFR-XXX requirements related to:
- Database installation and configuration
- Schema creation and migrations
- Database security (encryption, row-level security)
- Connection pooling and performance
- Backup and restore procedures
- Seed data requirements
```

### Prompt 3: Generate Database Setup Epic
```
Create EP-DB-001 epic with:
- Title: "Database Infrastructure & Schema Initialization"
- Priority: Critical (Blocking) - must execute after EP-TECH
- Mapped Requirements: DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014
- Key Deliverables:
  - PostgreSQL installation and pgvector extension setup
  - Entity Framework Core Migrations initialization
  - Initial schema creation for 16 tables from ERD
  - Database indexing strategy implementation
  - Connection pooling configuration
  - Static admin account seed data
  - Database backup/restore procedures
  - Connection string encryption
```

### Prompt 4: Insert Epic into Correct Position
```
Insert EP-DB-001 into epics.md:
- Position: After EP-TECH, before EP-001
- Renumber subsequent epics if necessary (or use EP-000 numbering)
- Update Epic Summary Table
- Ensure no requirement duplication with other epics
```

### Prompt 5: Validate Epic Coverage
```
Verify that EP-DB-001 covers:
1. All 16 tables from models.md ERD
2. All DR-XXX requirements related to database
3. Static admin account initialization (TR-014)
4. Connection pooling (NFR-015)
5. No overlap with existing epics
```

---

## Proposed Solution: EP-DB-001

### Epic Definition

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
- DR-001: FHIR-aligned patient-centric schema (baseline, extensible)
- DR-002: Referential integrity enforcement
- DR-003: Immutable audit log storage
- DR-011: Database indexing strategy
- DR-012: Connection string encryption
- NFR-015: Connection pooling (min 10, max 100)
- TR-014: Static admin account initialization

---

## Schema Flexibility & Implementation Notes

### Important: ERD as Baseline, Not Constraint

The ERD in models.md defines the **baseline schema** for Phase 1. However, during implementation:

**✅ ALLOWED: Schema Evolution During Implementation**
- If additional entities or fields are discovered during development, developers MAY:
  - Add new tables not in the ERD
  - Add new columns to existing tables
  - Modify data types if requirements change
  - Add additional indexes for performance
  - Create junction tables for many-to-many relationships

**📋 REQUIRED: Documentation & Migration**
- All schema changes MUST be:
  - Implemented via Entity Framework Core Migrations
  - Documented with migration comments explaining the reason
  - Updated in models.md ERD after implementation
  - Reviewed for impact on existing epics

**🔄 CRUD Operations Flexibility**
- Developers can perform database CRUD operations as needed during implementation
- No need to wait for epic updates if schema adjustments are required
- Focus on delivering working features, then update documentation

**Example Scenarios:**
```
Scenario 1: Missing Field Discovered
- Current ERD: USER table has 'name' field
- Implementation finds: Need 'first_name' and 'last_name' separately
- Action: Create migration, add fields, update models.md, continue

Scenario 2: New Entity Required
- Current ERD: No NOTIFICATION table
- Implementation finds: Need to track user notifications
- Action: Create NOTIFICATION table via migration, update models.md, continue

Scenario 3: Performance Optimization
- Current ERD: No index on DOCUMENT.status
- Implementation finds: Slow queries on status filtering
- Action: Add index via migration, update DR-011 in design.md, continue
```

**⚠️ Constraints That MUST Be Maintained:**
- Referential integrity (foreign keys)
- Soft delete pattern (is_deleted, deleted_at)
- Audit trail immutability
- PHI encryption requirements
- FHIR alignment principles (DR-001)

**Documentation Update Process:**
1. Implement schema change via EF Core Migration
2. Test the change
3. Update models.md ERD with new schema
4. Update design.md if DR-XXX requirements affected
5. Note changes in migration commit message

---

## Impact Assessment

### Without DB Setup Epic
❌ Developers blocked on implementation  
❌ No clear guidance on database setup  
❌ Risk of inconsistent database configurations across environments  
❌ Static admin account creation unclear  
❌ Performance issues without proper indexes  
❌ Security vulnerabilities without encryption

### With DB Setup Epic
✅ Clear, actionable database setup guidance  
✅ Consistent database configuration across dev/test/prod  
✅ Static admin account properly seeded  
✅ Performance optimized with proper indexes  
✅ Security hardened with encryption and row-level security  
✅ Backup/restore procedures documented  
✅ Schema evolution flexibility documented

---

## Recommendations

### Phase 1: Analysis & Documentation ✅ COMPLETE
1. ✅ Create this Issues and Gaps document
2. ✅ Document all required prompts
3. ✅ Document schema flexibility guidance
4. ⏳ Review and approve this analysis

### Phase 2: Implementation (AFTER APPROVAL)
1. **Modify create-epics.md workflow:**
   - Add database requirement detection logic from models.md
   - Add EP-DB-001 generation after EP-TECH detection
   - Add validation for database-related DR-XXX requirements
   - Add schema flexibility note to epic description

2. **Regenerate epics.md:**
   - Run modified create-epics.md workflow
   - Verify EP-DB-001 is inserted after EP-TECH
   - Verify schema flexibility note is included
   - Validate no requirement duplication

3. **Test and validate:**
   - Verify all 16 tables from ERD are covered
   - Verify all DR-XXX database requirements are mapped
   - Verify static admin account seed is included
   - Verify schema flexibility guidance is clear

---

## Conclusion

The current epic generation workflow has a **critical gap** in database setup coverage. Creating **EP-DB-001: Database Infrastructure & Schema Initialization** will:
- Unblock development by providing clear database setup guidance
- Ensure consistent database configuration across environments
- Properly map all database-related requirements (DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014)
- Enable static admin account initialization
- Establish performance and security best practices from the start
- Provide flexibility for schema evolution during implementation

**Key Principle:** ERD = Baseline, Not Prison. Developers can evolve schema via migrations as needed.

---

## Appendix A: All Prompts Summary

### For Workflow Modification (Phase 2)

**Prompt Set 1: Detection**
- Detect if models.md exists and contains ERD
- Extract all table definitions from ERD
- Identify database-related requirements (DR-XXX, NFR-015, TR-014)

**Prompt Set 2: Epic Generation**
- Generate EP-DB-001 with all deliverables
- Include schema flexibility note
- Map all database requirements

**Prompt Set 3: Integration**
- Insert EP-DB-001 after EP-TECH in Epic Summary Table
- Update epic descriptions
- Validate no requirement duplication

**Prompt Set 4: Validation**
- Verify 16 tables covered
- Verify all DR-XXX mapped
- Verify static admin seed included
- Verify schema flexibility documented

---

## Appendix B: Detailed Prompts for Workflow Implementation

### Prompt 1: Detect Database Requirements from models.md

**Purpose:** Extract all database schema information from the ERD in models.md

**Prompt:**
```
Analyze the file .propel/context/docs/models.md and extract the following:

1. Count total number of tables in the ERD (Logical Data Model section)
2. For each table, extract:
   - Table name
   - All column names and data types
   - Primary key (PK) columns
   - Foreign key (FK) columns and their references
   - Unique key (UK) constraints
3. List all relationships between tables (one-to-many, many-to-one, one-to-one)
4. Identify if pgvector extension is required (look for 'vector' data type)
5. Extract any index requirements mentioned in the ERD or comments

Output format:
- Total tables: [number]
- Tables list: [comma-separated table names]
- pgvector required: [yes/no]
- Relationships count: [number]
```

**Expected Output:**
```
Total tables: 16
Tables: USER, SESSION, PASSWORD_RESET_TOKEN, PATIENT, DOCUMENT_BATCH, DOCUMENT, PROCESSING_JOB, DOCUMENT_CHUNK, EXTRACTED_ENTITY, ENTITY_CITATION, CONFLICT, CONFLICT_RESOLUTION, BILLING_CODE_CATALOG_ITEM, CODE_SUGGESTION, AUDIT_LOG_EVENT, VECTOR_QUERY_LOG
pgvector required: yes (DOCUMENT_CHUNK.embedding column)
Relationships count: 20+
```

---

### Prompt 2: Map Database Requirements from design.md

**Purpose:** Identify all database-related requirements from design.md

**Prompt:**
```
Analyze the file .propel/context/docs/design.md and extract all requirements related to database:

1. Search for all DR-XXX (Data Requirements) and list:
   - Requirement ID
   - Requirement summary
   - Whether it relates to database setup/schema/security

2. Search for database-related TR-XXX (Technical Requirements):
   - TR-014: Static admin account initialization
   - Any TR related to database migrations
   - Any TR related to database security

3. Search for database-related NFR-XXX (Non-Functional Requirements):
   - NFR-015: Connection pooling
   - Any NFR related to database performance

4. Extract database technology stack information:
   - PostgreSQL version
   - pgvector extension requirement
   - Entity Framework Core Migrations

5. Extract database configuration requirements:
   - Connection pooling settings (min/max connections)
   - Backup strategy
   - Encryption requirements

Output format:
- DR requirements: [list with IDs and summaries]
- TR requirements: [list with IDs and summaries]
- NFR requirements: [list with IDs and summaries]
- Technology: [PostgreSQL version, pgvector, EF Core]
- Configuration: [pooling, backup, encryption details]
```

**Expected Output:**
```
DR requirements:
- DR-001: FHIR-aligned patient-centric schema
- DR-002: Referential integrity enforcement
- DR-003: Immutable audit log storage (7-year retention)
- DR-004: Embedding encryption at rest, no PHI in metadata
- DR-005: Row-level security for document chunks
- DR-006: Delete embeddings when source deleted
- DR-007: ICD-10/CPT catalog storage with refresh
- DR-008: Processing job metadata persistence
- DR-009: Conflict persistence with provenance
- DR-010: Soft delete strategy
- DR-011: Database indexing strategy
- DR-012: Connection string encryption

TR requirements:
- TR-014: Static admin account initialization via DB seed

NFR requirements:
- NFR-015: Connection pooling (min 10, max 100)

Technology:
- PostgreSQL 15+
- pgvector extension
- Entity Framework Core Migrations

Configuration:
- Connection pooling: min 10, max 100 connections
- Backup: Daily automated, 30-day retention, monthly archives
- Encryption: Connection strings, embeddings at rest
```

---

### Prompt 3: Generate EP-DB-001 Epic Content

**Purpose:** Create the complete epic definition for database setup

**Prompt:**
```
Generate a new epic EP-DB-001 with the following structure:

Title: "Database Infrastructure & Schema Initialization"

Priority: Critical (Blocking)

Business Value: Write 2-3 sentences explaining:
- Why database setup is foundational
- How it enables all subsequent features
- Security and compliance benefits

Description: Write 3-4 sentences covering:
- PostgreSQL + pgvector setup
- EF Core Migrations initialization
- Baseline schema creation from ERD
- Security and performance configuration

Schema Evolution Note: Include this exact text:
"The ERD in models.md serves as the baseline schema for Phase 1. Developers may add/modify entities and fields during implementation as needed via EF Core Migrations. All schema changes must be documented in migration comments and models.md must be updated post-implementation to reflect the actual schema."

Key Deliverables: List these items:
1. PostgreSQL 15+ installation with pgvector extension
2. Entity Framework Core Migrations framework initialization
3. Baseline migration creating 16 tables: [list all 16 table names]
4. Foreign key constraints and referential integrity enforcement
5. Database indexing strategy (DR-011): [list all indexes from DR-011]
6. Connection pooling configuration (min 10, max 100 connections)
7. Static admin account seed data from environment variables
8. Database backup/restore procedures with 30-day retention
9. Connection string encryption in configuration files
10. Row-level security setup for document_chunks table
11. Migration strategy documentation for schema evolution

Mapped Requirements: List these:
- DR-001: FHIR-aligned patient-centric schema (baseline, extensible)
- DR-002: Referential integrity enforcement
- DR-003: Immutable audit log storage
- DR-011: Database indexing strategy
- DR-012: Connection string encryption
- NFR-015: Connection pooling (min 10, max 100)
- TR-014: Static admin account initialization
```

**Expected Output:** Complete epic definition matching the format in "Proposed Solution: EP-DB-001" section above

---

### Prompt 4: Insert EP-DB-001 into Epic Summary Table

**Purpose:** Add the new epic to the Epic Summary Table in epics.md

**Prompt:**
```
Modify the file .propel/context/docs/epics.md:

1. Locate the "Epic Summary Table" section
2. Find the row for EP-TECH
3. Insert a new row immediately after EP-TECH with:
   - Epic ID: EP-DB-001
   - Epic Title: Database Infrastructure & Schema Initialization
   - Mapped Requirement IDs: DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014

4. Verify no requirement duplication:
   - Check if any of these requirements appear in other epics
   - If DR-012 appears in EP-TECH, note it as "shared requirement"
   - If DR-001, DR-002 appear in EP-035, verify EP-DB-001 focuses on setup while EP-035 focuses on domain model

5. Update epic ordering:
   - EP-TECH remains first
   - EP-DB-001 is second
   - All other epics follow

Do NOT renumber existing epics (EP-001 through EP-035 keep their numbers)
```

**Expected Output:**
```markdown
| Epic ID | Epic Title | Mapped Requirement IDs |
|---------|------------|------------------------|
| EP-TECH | Project Scaffolding & Baseline Architecture | TR-001, TR-015, TR-016, NFR-010, DR-012 |
| EP-DB-001 | Database Infrastructure & Schema Initialization | DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014 |
| EP-001 | Login Experience & Error Handling | FR-001a, FR-001c, FR-001d, FR-114, ... |
...
```

---

### Prompt 5: Add EP-DB-001 Description to Epic Descriptions Section

**Purpose:** Add the full epic description after EP-TECH in the Epic Description section

**Prompt:**
```
Modify the file .propel/context/docs/epics.md:

1. Locate the "Epic Description" section
2. Find the description for EP-TECH
3. Insert the complete EP-DB-001 description immediately after EP-TECH
4. Use the epic content generated in Prompt 3
5. Ensure markdown formatting is consistent with other epic descriptions
6. Include all sections:
   - Priority
   - Business Value
   - Description
   - Schema Evolution Note
   - Key Deliverables
   - Mapped Requirements
```

**Expected Output:** Complete epic description inserted after EP-TECH description

---

### Prompt 6: Validate Epic Coverage

**Purpose:** Ensure EP-DB-001 covers all database requirements without duplication

**Prompt:**
```
Validate the newly created EP-DB-001 in .propel/context/docs/epics.md:

1. Verify all 16 tables from models.md ERD are mentioned in Key Deliverables:
   - USER, SESSION, PASSWORD_RESET_TOKEN
   - PATIENT, DOCUMENT_BATCH, DOCUMENT
   - PROCESSING_JOB, DOCUMENT_CHUNK
   - EXTRACTED_ENTITY, ENTITY_CITATION
   - CONFLICT, CONFLICT_RESOLUTION
   - BILLING_CODE_CATALOG_ITEM, CODE_SUGGESTION
   - AUDIT_LOG_EVENT, VECTOR_QUERY_LOG

2. Verify all database-related DR requirements are mapped:
   - DR-001, DR-002, DR-003, DR-011, DR-012 (minimum)
   - Check if DR-004, DR-005, DR-006 should be included

3. Verify TR-014 (static admin account) is mapped

4. Verify NFR-015 (connection pooling) is mapped

5. Check for requirement duplication:
   - Compare EP-DB-001 requirements with EP-TECH
   - Compare EP-DB-001 requirements with EP-035
   - Note any shared requirements and verify they're intentional

6. Verify schema flexibility note is included

7. Verify all indexes from DR-011 are listed:
   - user.email
   - document.patient_id
   - document.upload_date
   - processing_job.status
   - extracted_entity.patient_id
   - audit_log.timestamp
   - pgvector HNSW index on embeddings

Output validation report:
- Tables covered: [yes/no] [count]
- DR requirements mapped: [list]
- TR requirements mapped: [list]
- NFR requirements mapped: [list]
- Duplications found: [list or "none"]
- Schema flexibility note: [present/missing]
- Indexes covered: [yes/no] [count]
- Validation status: [PASS/FAIL]
```

**Expected Output:**
```
Validation Report:
- Tables covered: YES (16/16)
- DR requirements mapped: DR-001, DR-002, DR-003, DR-011, DR-012
- TR requirements mapped: TR-014
- NFR requirements mapped: NFR-015
- Duplications found: DR-012 shared with EP-TECH (intentional - connection string encryption)
- Schema flexibility note: PRESENT
- Indexes covered: YES (7/7)
- Validation status: PASS
```

---

### Prompt 7: Update Epic Count and Summary

**Purpose:** Update the document header and summary to reflect the new epic

**Prompt:**
```
Update .propel/context/docs/epics.md:

1. If there's an epic count in the document header, update it:
   - Old: "35 epics (EP-TECH through EP-035)"
   - New: "36 epics (EP-TECH, EP-DB-001, EP-001 through EP-035)"

2. If there's an executive summary or overview section, add a note:
   - "EP-DB-001 added to address database infrastructure setup"
   - "EP-DB-001 must be executed after EP-TECH and before EP-001"

3. Update any epic dependency notes:
   - EP-001 through EP-035 now depend on EP-DB-001 completion
   - Database must be initialized before authentication features
```

---

### Prompt 8: Generate Implementation Checklist

**Purpose:** Create a checklist for developers implementing EP-DB-001

**Prompt:**
```
Generate an implementation checklist for EP-DB-001 that developers can use:

Format as markdown checklist:

## EP-DB-001 Implementation Checklist

### Phase 1: PostgreSQL Setup
- [ ] Install PostgreSQL 15+
- [ ] Install pgvector extension
- [ ] Create database user/role
- [ ] Configure SSL/TLS
- [ ] Test connection

### Phase 2: EF Core Migrations Setup
- [ ] Install Entity Framework Core packages
- [ ] Initialize migrations project
- [ ] Configure connection string (encrypted)
- [ ] Test migration framework

### Phase 3: Baseline Schema Creation
- [ ] Create migration for 16 tables
- [ ] Add foreign key constraints
- [ ] Add unique constraints
- [ ] Add check constraints
- [ ] Test migration up/down

### Phase 4: Indexing Strategy
- [ ] Create index on user.email
- [ ] Create index on document.patient_id
- [ ] Create index on document.upload_date
- [ ] Create index on processing_job.status
- [ ] Create index on extracted_entity.patient_id
- [ ] Create index on audit_log.timestamp
- [ ] Create pgvector HNSW index on embeddings

### Phase 5: Configuration
- [ ] Configure connection pooling (min 10, max 100)
- [ ] Set up row-level security for document_chunks
- [ ] Configure encryption at rest

### Phase 6: Seed Data
- [ ] Create static admin account seed
- [ ] Load admin credentials from environment variables
- [ ] Test admin account login

### Phase 7: Backup/Restore
- [ ] Configure automated daily backups
- [ ] Set 30-day retention policy
- [ ] Test restore procedure
- [ ] Document backup/restore process

### Phase 8: Validation
- [ ] Verify all 16 tables created
- [ ] Verify all indexes created
- [ ] Verify foreign keys enforced
- [ ] Verify static admin account exists
- [ ] Verify connection pooling works
- [ ] Run integration tests
```

---

## Appendix C: Workflow Modification Pseudocode

### For create-epics.md Workflow

```python
# Pseudocode for adding EP-DB-001 generation to create-epics.md

def generate_epics():
    # Existing code...
    
    # Read source documents
    spec = read_file(".propel/context/docs/spec.md")
    design = read_file(".propel/context/docs/design.md")
    models = read_file(".propel/context/docs/models.md")  # NEW
    
    # Detect project type
    is_greenfield = detect_greenfield()
    
    # Generate EP-TECH if greenfield
    if is_greenfield:
        epics.append(generate_ep_tech())
    
    # NEW: Generate EP-DB-001 if models.md has ERD
    if has_erd(models):
        db_requirements = extract_db_requirements(design, models)
        ep_db_001 = generate_ep_db_001(db_requirements, models)
        epics.append(ep_db_001)
    
    # Generate feature epics (existing code)
    feature_epics = generate_feature_epics(spec, design)
    epics.extend(feature_epics)
    
    # Write output
    write_epics_file(epics)

def has_erd(models_content):
    """Check if models.md contains ERD"""
    return "erDiagram" in models_content or "Logical Data Model" in models_content

def extract_db_requirements(design_content, models_content):
    """Extract database-related requirements"""
    requirements = {
        "dr_requirements": extract_dr_requirements(design_content),
        "tr_requirements": extract_tr_requirements(design_content, ["TR-014"]),
        "nfr_requirements": extract_nfr_requirements(design_content, ["NFR-015"]),
        "tables": extract_tables_from_erd(models_content),
        "indexes": extract_indexes(design_content),
        "pgvector_required": "vector" in models_content
    }
    return requirements

def generate_ep_db_001(db_requirements, models_content):
    """Generate EP-DB-001 epic"""
    epic = {
        "id": "EP-DB-001",
        "title": "Database Infrastructure & Schema Initialization",
        "priority": "Critical (Blocking)",
        "business_value": generate_business_value(),
        "description": generate_description(db_requirements),
        "schema_evolution_note": get_schema_flexibility_note(),
        "key_deliverables": generate_deliverables(db_requirements),
        "mapped_requirements": db_requirements["dr_requirements"] + 
                               db_requirements["tr_requirements"] + 
                               db_requirements["nfr_requirements"]
    }
    return epic

def get_schema_flexibility_note():
    """Return the schema flexibility note"""
    return (
        "The ERD in models.md serves as the baseline schema for Phase 1. "
        "Developers may add/modify entities and fields during implementation "
        "as needed via EF Core Migrations. All schema changes must be documented "
        "in migration comments and models.md must be updated post-implementation "
        "to reflect the actual schema."
    )
```

---

**Document Status:** ✅ Phase 1 Complete  
**Next Action:** Review and approve before Phase 2 implementation  
**Phase 2 Blocked Until:** Manager approval received
