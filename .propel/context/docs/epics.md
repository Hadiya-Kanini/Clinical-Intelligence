# Epic - EP-000

## Epic Summary Table

| Epic ID | Epic Title | Mapped Requirement IDs |
|---------|------------|------------------------|
| EP-TECH | Project Scaffolding & Baseline Architecture | TR-001, TR-015, TR-016, NFR-010, DR-012 |
| EP-DB-001 | Database Infrastructure & Schema Initialization | DR-001, DR-002, DR-003, DR-011, DR-012, NFR-015, TR-014 |
| EP-001 | Login Experience & Error Handling | FR-001a, FR-001c, FR-001d, FR-114, FR-114a, FR-114b, FR-114c, FR-114d, UXR-001, UXR-002, UXR-004, UXR-008 |
| EP-002 | Core Authentication, Sessions & Logout | FR-001, FR-001b, FR-002, FR-003, FR-003a, FR-003b, FR-008, FR-009, FR-009a, UC-001, TR-002 |
| EP-003 | Rate Limiting & Account Lockout Controls | FR-004, FR-006, FR-007, UXR-009, UXR-010, TR-003 |
| EP-004 | Input Validation & Credential Hardening | FR-009b, FR-009c, FR-009d, FR-009f, FR-009g, FR-009h, UXR-012 |
| EP-005 | CSRF Protection & CORS Policy | FR-009e, TR-013, TR-019 |
| EP-006 | Forgot Password Request Flow (Email + UX) | FR-005, FR-009n, FR-009o, FR-009q, FR-115, FR-115a, FR-115b, FR-115c, UXR-003, UXR-007, UC-002 |
| EP-007 | Password Reset Confirmation & Recovery UX | FR-009p, FR-009r, FR-009s, FR-009t, FR-116, FR-116a, FR-116b, FR-116c, FR-116d, UXR-006 |
| EP-008 | Role-Based Access Control (Admin vs Standard) | FR-010, FR-011, FR-012, FR-013, TR-014 |
| EP-009 | Admin-Only User Provisioning (No Public Registration) | FR-009i, FR-009j, FR-009k, FR-009l, FR-009m, FR-087, UXR-005, UC-001a |
| EP-010 | Admin User Management UI & Operations | FR-088, FR-089, FR-122, UXR-011, UXR-044, UC-010 |
| EP-011 | Document Upload UX (Progress + Feedback) | FR-017, FR-018, FR-019, UXR-019, UXR-020, UXR-021, UXR-022, NFR-016, NFR-001 |
| EP-012 | File Upload Validation, Limits & Malware Scanning | FR-014, FR-015, FR-015a, FR-015b, FR-015c, FR-015d, FR-015e, FR-015f, FR-015g, FR-016, TR-018 |
| EP-013 | Document Storage, Listing & Status Tracking | FR-020, FR-021, FR-022, TR-017, DR-010, DR-011, UC-003 |
| EP-014 | Asynchronous Processing Orchestration (RabbitMQ + Retries) | FR-023, FR-024, FR-025, FR-026, FR-027, FR-028, FR-029, UC-004, TR-004, NFR-008, NFR-009, DR-008 |
| EP-015 | RAG Text Extraction & Chunking Pipeline | FR-030, FR-031, FR-032, FR-032a, TR-005, NFR-003 |
| EP-016 | Vector Store Ingestion & Retrieval (pgvector) | FR-033, FR-034, FR-035, FR-036, DR-005 |
| EP-017 | LLM Extraction & Schema Validation | FR-037, FR-038, TR-006, TR-007 |
| EP-018 | Clinical Entity Coverage (Core Categories) | FR-039, FR-040, FR-041, FR-042, FR-043, FR-044, FR-045, FR-046, FR-047, FR-048, FR-049, FR-050 |
| EP-019 | Explainability & Grounding Enforcement | FR-051, FR-052, FR-053, FR-054, FR-055, FR-056, TR-008, NFR-006, UXR-024 |
| EP-020 | Verification Screen UX (Side-by-Side + Editing) | UXR-023, UXR-025, UXR-026, UXR-027, FR-076, UXR-017 |
| EP-021 | Conflict Detection Output & Persistence | FR-057, FR-058, FR-059, DR-009 |
| EP-022 | Conflict Resolution Workflow (UI + Audit) | FR-060, FR-061, FR-062, FR-077, UXR-028, UXR-029, UXR-030, UXR-031, UC-006 |
| EP-023 | ICD-10/CPT Catalog Storage & Refresh | FR-063, FR-064, FR-071, DR-007, TR-009 |
| EP-024 | Code Mapping, Suggestions & Review UX | FR-065, FR-066, FR-067, FR-068, FR-069, FR-070, UXR-032, UXR-033, UXR-034, UXR-035, UC-007, TR-010 |
| EP-025 | Patient 360 View (Performance + Content) | FR-072, FR-073, FR-074, FR-075, FR-120, UC-005, NFR-002, NFR-013, NFR-015 |
| EP-026 | Core App Navigation, Profile & Accessibility | FR-117, FR-118, FR-119, FR-121, UXR-013, UXR-014, UXR-015, UXR-016, UXR-018 |
| EP-027 | AI Clinical Assistant (RAG Chat) | FR-078, FR-079, FR-080, FR-081, UXR-036, UXR-037, UXR-038, UXR-039, UC-008 |
| EP-028 | Finalize & Export (CSV/JSON/Clipboard) | FR-082, FR-083, FR-084, FR-085, FR-086, TR-011, UC-009 |
| EP-029 | Analytics & Productivity Dashboard | FR-091, FR-092, FR-093, FR-094, FR-095, FR-096, UXR-040, UXR-041, UXR-042, UXR-043, NFR-012 |
| EP-030 | Immutable Audit Framework (Integrity + Retention) | FR-101, FR-103, FR-103a, FR-103b, DR-003, NFR-007 |
| EP-031 | Audit Event Coverage (Auth, Docs, Patient Actions) | FR-097, FR-098, FR-099, FR-100, FR-102 |
| EP-032 | Security Audit Log Viewer & Export | FR-090, UXR-045, UXR-046, UC-011 |
| EP-033 | Health Checks, Monitoring & Operational Readiness | FR-104, FR-105, FR-106, FR-107, FR-108, TR-012, NFR-011, NFR-004, NFR-014 |
| EP-034 | Vector Data Security & PHI Controls | FR-109, FR-110, FR-111, FR-112, FR-113, DR-004, DR-006, NFR-005 |
| EP-035 | Core Domain Data Model & Referential Integrity | DR-001, DR-002 |

## Epic Description

### EP-TECH: Project Scaffolding & Baseline Architecture
**Priority**: High  
**Business Value**: Enables safe and consistent delivery of all subsequent platform features by establishing the foundational architecture, API standards, and secure configuration practices.  
**Description**: Establish the initial repository structure, baseline architecture boundaries (Web UI, Backend API, AI Worker), API versioning conventions, and standardized error responses so feature work can proceed with consistent quality and predictable integration points.

**Key Deliverables**:
- Define service boundaries and contracts aligned to TR-001
- Establish API versioning convention aligned to TR-015
- Establish standardized error response format aligned to TR-016
- Define maintainability guardrails aligned to NFR-010
- Ensure secure configuration management aligned to DR-012

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
- Database indexing strategy (DR-011): indexes on user.email, document.patient_id, document.upload_date, processing_job.status, extracted_entity.patient_id, audit_log.timestamp, pgvector HNSW index
- Connection pooling configuration (min 10, max 100 connections)
- Static admin account seed data from environment variables
- Database backup/restore procedures with 30-day retention
- Connection string encryption in configuration files
- Row-level security setup for document_chunks table
- Migration strategy documentation for schema evolution

### EP-001: Login Experience & Error Handling
**Priority**: High  
**Business Value**: Improves user adoption and reduces support burden by providing a trustworthy, professional first impression and actionable authentication feedback.  
**Description**: Deliver the login page experience with healthcare-appropriate styling, validation, clear errors, and loading states.

**Key Deliverables**:
- Professional login UI with validation and accessibility considerations
- Actionable authentication error messaging
- Loading indicators during login
- Visible logout confirmation UX entry points

### EP-002: Core Authentication, Sessions & Logout
**Priority**: High  
**Business Value**: Establishes secure access control for PHI-bearing workflows and enables auditability of user actions.  
**Description**: Implement JWT-based authentication with HttpOnly cookies, session tracking, inactivity timeout, and secure logout semantics.

**Key Deliverables**:
- JWT issuance and secure cookie storage
- Session lifecycle and inactivity timeout
- Logout and token invalidation behavior
- UC-001 flow implementation alignment

### EP-003: Rate Limiting & Account Lockout Controls
**Priority**: High  
**Business Value**: Reduces risk of credential stuffing and protects system stability under attack or misuse.  
**Description**: Implement rate limits for authentication-related actions and expose user-friendly lockout and retry messaging.

**Key Deliverables**:
- Login and password-reset request rate limiting
- Account lockout enforcement and unlock timing behavior
- Consistent HTTP 429 handling and UX messaging

### EP-004: Input Validation & Credential Hardening
**Priority**: High  
**Business Value**: Prevents avoidable security incidents by hardening credential handling and validating all user inputs.  
**Description**: Implement robust email/password validation, input sanitization, and secure password hashing.

**Key Deliverables**:
- RFC-compliant email validation
- Password complexity enforcement and UX guidance
- Input sanitization and suspicious input rejection
- Secure password hashing configuration

### EP-005: CSRF Protection & CORS Policy
**Priority**: High  
**Business Value**: Protects cookie-authenticated workflows from making unintended state changes and prevents unsafe cross-origin access.  
**Description**: Implement CSRF protections for state-changing endpoints and configure strict CORS to allow only approved frontend origins.

**Key Deliverables**:
- CSRF protections for state-changing requests
- Strict CORS configuration supporting credentials

### EP-006: Forgot Password Request Flow (Email + UX)
**Priority**: High  
**Business Value**: Reduces account recovery friction while maintaining security through generic responses and rate controls.  
**Description**: Provide the forgot-password request experience, token generation, and email delivery with secure, non-disclosing UX.

**Key Deliverables**:
- Forgot password page UX and navigation
- Secure reset-token generation and email send
- Generic “check email” responses to avoid account enumeration
- UC-002 request portion alignment
`r`n`r`n**User Stories**:`r`n- US_009: Implement Password Reset Flow with Email Notifications (17 hours)`r`n- US_016: Account lockout email notification (2 hours)`r`n
### EP-007: Password Reset Confirmation & Recovery UX
**Priority**: High  
**Business Value**: Enables secure account recovery with reduced support calls and ensures compromised sessions are revoked after reset.  
**Description**: Implement the reset-password confirmation experience, token validation, password update, and session invalidation.

**Key Deliverables**:
- Reset password page UX with strength indicator
- Token validation, one-time use, and invalidation
- Revoke existing sessions upon successful reset
- Audit of reset attempts and completions

### EP-008: Role-Based Access Control (Admin vs Standard)
**Priority**: High  
**Business Value**: Ensures only authorized staff can perform sensitive administrative actions and restricts PHI features appropriately.  
**Description**: Enforce Admin and Standard roles across the UI and API surface. Phase 1 uses a single static admin account created during system initialization.

**Key Deliverables**:
- Role definitions and authorization checks
- Static admin account initialization via database seed
- Restrict admin-only features to Admin role
- Prevent Standard users from accessing admin functionality

### EP-009: Admin-Only User Provisioning (No Public Registration)
**Priority**: High  
**Business Value**: Supports controlled onboarding for healthcare organizations without exposing public registration risk.  
**Description**: Provide the backend/admin flow for creating Standard User accounts. The static admin account (created during system initialization) is the only account authorized to create new users.

**Key Deliverables**:
- Enforce “no public registration” policy
- Admin-only user creation workflow with validations
- Static admin account initialization with secure credentials
- Duplicate email prevention
- Credential email dispatch
- UC-001a alignment

### EP-010: Admin User Management UI & Operations
**Priority**: High  
**Business Value**: Enables administrators to manage access for healthcare staff and maintain operational continuity.  
**Description**: Provide the user management UI and operations for updating and deactivating accounts.

**Key Deliverables**:
- User management page for Admin role
- Update and deactivate user operations
- Searchable/sortable user table UX
- Confirmation messaging for admin actions
- UC-010 alignment

### EP-011: Document Upload UX (Progress + Feedback)
**Priority**: High  
**Business Value**: Reduces time-on-task by making upload workflows fast, transparent, and error-resilient.  
**Description**: Provide drag-and-drop upload UX with per-file progress, immediate feedback, and performance targets.

**Key Deliverables**:
- Drag-and-drop upload UI
- Per-file progress tracking with percent
- Clear validation errors and user guidance
- Meet upload UX and acknowledgment NFRs

### EP-012: File Upload Validation, Limits & Malware Scanning
**Priority**: High  
**Business Value**: Prevents unsafe or invalid documents from entering the processing pipeline and reduces operational waste.  
**Description**: Validate batch size, MIME types, extensions, file integrity, and scan for malware before accepting for processing.

**Key Deliverables**:
- Strict format validation for PDF/DOCX
- Reject password-protected, corrupted, empty, or suspicious documents
- Enforce file size and batch upload limits
- Malware scanning/quarantine workflow

### EP-013: Document Storage, Listing & Status Tracking
**Priority**: High  
**Business Value**: Gives users visibility into processing progress and creates the operational backbone for downstream workflows.  
**Description**: Store uploaded documents, track statuses, list documents with pagination, and persist metadata required for audits and operations.

**Key Deliverables**:
- Persist document metadata and storage paths
- Document list UI/endpoint behavior and pagination
- Status tracking state machine for user visibility
- UC-003 alignment

### EP-014: Asynchronous Processing Orchestration (RabbitMQ + Retries)
**Priority**: High  
**Business Value**: Enables scalable, resilient processing without blocking end-user workflows.  
**Description**: Implement background job queuing, retries, DLQ monitoring, and real-time status updates.

**Key Deliverables**:
- Enqueue and consume processing jobs
- Retry strategy with exponential backoff
- DLQ monitoring and operational workflows
- Processing metadata tracking and error surfacing

### EP-015: RAG Text Extraction & Chunking Pipeline
**Priority**: High  
**Business Value**: Produces a reliable retrieval corpus enabling accurate extraction and assistant responses.  
**Description**: Extract text from PDF/DOCX with positional metadata, merge per-patient text, and split into semantic chunks.

**Key Deliverables**:
- PDF/DOCX text extraction with positional metadata
- Patient-level merge of multi-document text prior to chunking
- Semantic chunking configuration and performance alignment

### EP-016: Vector Store Ingestion & Retrieval (pgvector)
**Priority**: High  
**Business Value**: Enables RAG retrieval for extraction and clinical assistant queries at scale.  
**Description**: Generate embeddings, persist chunks and vectors, and implement similarity search with access controls.

**Key Deliverables**:
- Embedding generation and persistence
- Store chunks with required metadata
- Similarity search top-K retrieval behavior
- Enforce access controls for vector/chunk retrieval

### EP-017: LLM Extraction & Schema Validation
**Priority**: High  
**Business Value**: Converts unstructured documents into trustworthy, structured clinical data with validation guardrails.  
**Description**: Extract entities using a single-call approach and validate output using schemas.

**Key Deliverables**:
- Single-call extraction prompt strategy
- Versioned schema validation and failure handling
- Structured outputs ready for downstream UI rendering

### EP-018: Clinical Entity Coverage (Core Categories)
**Priority**: High  
**Business Value**: Ensures the platform produces a comprehensive 360-degree view across key clinical dimensions.  
**Description**: Implement extraction coverage for the core entity categories and support extensibility beyond the initial set.

**Key Deliverables**:
- Support extraction for demographics, allergies, meds, diagnoses, procedures
- Support extraction for labs, vitals, social history, clinical notes
- Document metadata extraction and extensible category support
- Multi-document patient linkage rules

### EP-019: Explainability & Grounding Enforcement
**Priority**: High  
**Business Value**: Delivers the “trust-first” differentiator by preventing hallucinations and enabling clinicians to verify source evidence quickly.  
**Description**: Enforce grounding so every entity includes citations and location metadata; provide explainability cues that surface rationale and sources.

**Key Deliverables**:
- Citation enforcement and rejection of uncited entities
- Source metadata and clickable reference support
- Extraction rationale UX behavior
- Source text highlighting trigger behavior

### EP-020: Verification Screen UX (Side-by-Side + Editing)
**Priority**: High  
**Business Value**: Shifts work from manual extraction to fast verification and correction, driving time-on-task improvements.  
**Description**: Provide the verification UX with side-by-side document/data view, clear status indicators, and inline editing.

**Key Deliverables**:
- Side-by-side view of source document and extracted data
- Visual differentiation of verified/unverified/modified data
- Inline editing for extracted fields while preserving provenance
- Real-time feedback for user actions

### EP-021: Conflict Detection Output & Persistence
**Priority**: High  
**Business Value**: Reduces clinical and billing risk by identifying inconsistent information across documents.  
**Description**: Detect critical conflicts and persist them in structured form with provenance.

**Key Deliverables**:
- Conflict detection for critical data fields
- Structured conflict records with source provenance
- Persisted conflicts ready for UI presentation

### EP-022: Conflict Resolution Workflow (UI + Audit)
**Priority**: High  
**Business Value**: Ensures patient records are consistent and export-ready, with auditable resolution decisions.  
**Description**: Provide conflict-resolution UI and record user decisions in audit logs.

**Key Deliverables**:
- Side-by-side conflict comparison UI with sources
- Required explicit user selection to resolve conflicts
- Prevent finalization until conflicts resolved
- UC-006 alignment

### EP-023: ICD-10/CPT Catalog Storage & Refresh
**Priority**: High  
**Business Value**: Enables accurate, searchable coding catalogs and supports ongoing updates to medical code sets.  
**Description**: Store ICD-10 and CPT catalogs with search indexing and support periodic refresh.

**Key Deliverables**:
- ICD-10 and CPT catalog persistence
- Full-text search and refresh mechanism
- Integration hooks for code mapping pipeline

### EP-024: Code Mapping, Suggestions & Review UX
**Priority**: High  
**Business Value**: Reduces missed billing codes while keeping humans in control to prevent coding errors.  
**Description**: Map diagnoses/procedures to codes, present suggestions with sources, and support Accept/Reject and manual add flows.

**Key Deliverables**:
- Fuzzy mapping of diagnoses to ICD-10 and procedures to CPT
- Suggestion display with descriptions and source references
- Accept/Reject workflow and manual entry UX
- UC-007 alignment

### EP-025: Patient 360 View (Performance + Content)
**Priority**: High  
**Business Value**: Delivers the primary user value: a fast, comprehensive 360-degree patient summary with traceability.  
**Description**: Implement the Patient 360 dashboard content areas and meet dashboard performance and API responsiveness targets.

**Key Deliverables**:
- Aggregated patient profile and clinical content rendering
- Coding outputs section integration
- Performance targets for dashboard load and API responsiveness
- UC-005 alignment

### EP-026: Core App Navigation, Profile & Accessibility
**Priority**: Medium  
**Business Value**: Improves usability and adoption by ensuring the app is navigable, consistent, and accessible.  
**Description**: Implement core pages (Dashboard, Upload, Document List, Patient 360, Profile) with consistent navigation and WCAG-aligned accessibility.

**Key Deliverables**:
- Core page scaffolding and navigation model
- WCAG 2.1 AA aligned UX requirements
- Consistent design system application

### EP-027: AI Clinical Assistant (RAG Chat)
**Priority**: Medium  
**Business Value**: Enables rapid question answering across patient documents while preserving trust through citations.  
**Description**: Provide the RAG-powered chat interface and backend query workflow.

**Key Deliverables**:
- Chat UI with response loading feedback
- Retrieval-backed answering with citations
- Suggested query prompts
- UC-008 alignment

### EP-028: Finalize & Export (CSV/JSON/Clipboard)
**Priority**: High  
**Business Value**: Completes the “review and verify” workflow by enabling downstream use of verified data and reducing manual re-entry.  
**Description**: Provide finalization gating and export options for CSV/JSON/clipboard.

**Key Deliverables**:
- Export gating based on conflict resolution completion
- CSV and JSON exports plus clipboard copy
- UC-009 alignment

### EP-029: Analytics & Productivity Dashboard
**Priority**: Medium  
**Business Value**: Quantifies value delivery and drives adoption with visibility into quality and productivity KPIs.  
**Description**: Provide dashboards for conflicts, agreement rate, adoption metrics, and time-on-task tracking.

**Key Deliverables**:
- Metrics dashboards with charts and drill-down
- Date range filtering and real-time update behavior
- Track productivity and quality KPIs

### EP-030: Immutable Audit Framework (Integrity + Retention)
**Priority**: High  
**Business Value**: Supports compliance and governance requirements with tamper-evident, long-retained security and PHI access records.  
**Description**: Implement immutable audit storage with cryptographic integrity and retention controls.

**Key Deliverables**:
- Append-only audit event model with integrity verification
- Retention policies aligned to compliance requirements
- Audit schema and storage strategy

### EP-031: Audit Event Coverage (Auth, Docs, Patient Actions)
**Priority**: High  
**Business Value**: Enables traceability for investigations, compliance, and security monitoring by capturing required event types.  
**Description**: Ensure required audit events are logged for auth, user management, documents, and patient-level activity.

**Key Deliverables**:
- Capture auth events, admin events, document events, and patient action events
- Ensure PHI access views are explicitly logged

### EP-032: Security Audit Log Viewer & Export
**Priority**: Medium  
**Business Value**: Empowers admins to monitor security posture and respond faster to potential incidents.  
**Description**: Provide a paginated, filterable audit log viewer with export capability.

**Key Deliverables**:
- Audit log viewer UX with filters
- Export capability for filtered results
- UC-011 alignment

### EP-033: Health Checks, Monitoring & Operational Readiness
**Priority**: High  
**Business Value**: Reduces downtime and accelerates incident response by enabling visibility into system health and performance.  
**Description**: Implement health endpoints and operational telemetry for the API, worker, and queue.

**Key Deliverables**:
- Health endpoints for API and AI worker
- RabbitMQ monitoring surface and core metrics
- Structured logging suitable for aggregation
- Uptime and concurrency targets alignment

### EP-034: Vector Data Security & PHI Controls
**Priority**: High  
**Business Value**: Prevents PHI leakage through embeddings and ensures vector operations are fully auditable and compliant.  
**Description**: Encrypt embeddings at rest, ensure no PHI in embedding metadata, audit vector queries, and enforce deletion semantics.

**Key Deliverables**:
- Embedding encryption at rest and PHI-safe metadata strategy
- Row-level security enforcement for vector data access
- Vector query auditing and deletion on source document deletion

### EP-035: Core Domain Data Model & Referential Integrity
**Priority**: High  
**Business Value**: Provides a durable, integration-ready data foundation that supports FHIR alignment and consistent downstream behavior.  
**Description**: Define the patient-centric domain model and enforce referential integrity across core entities.

**Key Deliverables**:
- FHIR-aligned patient-centric schema approach
- Referential integrity constraints across domain entities
