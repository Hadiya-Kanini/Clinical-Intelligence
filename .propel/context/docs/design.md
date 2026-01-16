# Architecture Design

## Project Overview
The Trust-First Clinical Intelligence Platform is a standalone, integration-ready system that reduces healthcare staff "data-detective" work (20+ minutes) to a "review and verify" workflow (~2 minutes). Users upload up to 10 clinical documents (PDF/DOCX) per batch; the system asynchronously processes documents via a RAG pipeline to extract and aggregate grounded clinical entities into a unified 360-degree patient view, detect conflicts, suggest ICD-10/CPT codes with human-in-the-loop approval, and provide a RAG-powered clinical assistant for natural-language querying.

Primary users:
- Standard Users: clinicians, medical coders, and staff who upload documents, review extracted data, resolve conflicts, and finalize exports.
- Admin Users: user management, security/audit review, and platform health oversight.

## Architecture Goals
- Architecture Goal 1 : Trust-first extraction with 100% source grounding (no uncited entities)
- Architecture Goal 2 : Safe clinical review workflow (conflict detection, Accept/Reject for codes, auditable user actions)
- Architecture Goal 3 : Low-latency user experience (fast upload acknowledgement, <3s patient dashboard load)
- Architecture Goal 4 : Asynchronous, resilient processing (queue-based worker scale-out, retries + DLQ)
- Architecture Goal 5 : Integration-ready domain model (FHIR-aligned entities and exportable structured outputs)
- Architecture Goal 6 : HIPAA-oriented security controls (least privilege, encryption, immutable audit trail)

## Technology Stack
| Layer | Primary Choice | Version/Notes |
|---|---|---|
| Frontend | React + TypeScript + Vite + TailwindCSS | React 19 + TypeScript + Vite + TailwindCSS |
| Backend API | ASP.NET Core Web API | .NET 8 (BRD suggests ASP.NET Core 8) |
| AI Worker | Python + FastAPI | Python 3.11+; FastAPI for health endpoints |
| Queue | RabbitMQ | For async processing, retries, DLQ |
| Relational DB | PostgreSQL | With `pgvector` extension |
| Vector Store | pgvector (in PostgreSQL) | 768-d embeddings; HNSW index recommended for scale |
| RAG Orchestration | LangChain | Loaders, splitting, embeddings integration |
| Document Loaders | LangChain `PyPDFLoader`, `Docx2txtLoader` | Extract text + positional metadata |
| Chunking | `RecursiveCharacterTextSplitter` | 500–1000 tokens; 100-token overlap |
| Validation | Pydantic | Schema validation for extracted entities |
| LLM / Embeddings | Google Gemini API | `gemini-2.5-flash` (LLM), `text-embedding-004` (embeddings); Free tier: 15 RPM, 1M tokens/day; Upgrade path available |
| Auth | JWT + HttpOnly cookies | 15-minute access token; session management + revocation |
| Email | SMTP service | Password reset workflows; TLS |
| Observability | Structured logging + health endpoints | `/health` on API; `/health`, `/health/ready`, `/health/live` on worker |
| Database Migrations | Entity Framework Core Migrations | For .NET Backend schema versioning |
| API Documentation | Swagger/OpenAPI | Auto-generated API docs for Backend API |
| Frontend State Management | Redux Toolkit or Zustand | Global state management for React |
| HTTP Client | Axios | Frontend API communication |
| Frontend Testing | Jest + React Testing Library | Unit and integration tests |
| Backend Testing | xUnit | .NET API testing framework |
| AI Worker Testing | pytest | Python worker testing framework |

**Note:** The repository currently contains requirements/spec artifacts but no application source directories; concrete version pinning and package selection should be finalized during implementation.

### Alternative Technology Options
- Alternative 1: Python-only services (FastAPI for Backend API + Python worker) with PostgreSQL + pgvector and RabbitMQ.
- Alternative 2: Node.js API (NestJS) + Python worker, PostgreSQL + pgvector, RabbitMQ.
- Alternative 3: Managed vector DB (Pinecone / Weaviate) instead of pgvector for very large-scale vector workloads.
- Alternative 4: Managed queue (Azure Service Bus / AWS SQS) instead of RabbitMQ for cloud-first operations.
- Alternative 5: Local LLM (Ollama with Llama 3 or Mistral) instead of Google Gemini for zero-cost, fully offline operation.

### Technology Stack Validation
- The chosen split (Backend API in ASP.NET Core + AI Worker in Python) aligns with:
  - enterprise-grade auth/session and audit controls (Backend API)
  - Python-first ecosystem for LangChain, Pydantic, and document parsing (AI Worker)
- PostgreSQL + pgvector satisfies:
  - transactional needs (users, documents, audits, extracted entities)
  - vector similarity search for RAG and clinical assistant
- RabbitMQ satisfies:
  - asynchronous processing requirement
  - retries with exponential backoff and dead-lettering patterns

## Configuration & Infrastructure

### SMTP Configuration
- **Development**: Local SMTP server or Gmail SMTP (smtp.gmail.com:587)
- **Production**: SendGrid, AWS SES, or enterprise SMTP relay
- **Requirements**: TLS 1.2+, authentication, rate limiting support

### File Storage Structure
- **Base Path**: `./storage/documents/` (configurable via environment)
- **Structure**: `{tenant_id}/{patient_id}/{document_id}/original.{ext}`
- **Temp Upload**: `./storage/temp/` (cleaned after processing)
- **Max Storage**: Configurable per environment

### Database Backup Strategy
- **Frequency**: Daily automated backups (PostgreSQL pg_dump)
- **Retention**: 30 days rolling, 12 monthly archives
- **Recovery**: Point-in-time recovery (PITR) enabled
- **Vector Data**: Backup includes pgvector embeddings
- **Testing**: Monthly restore validation required

### PDF/DOCX Viewer Integration
- **Approach**: PDF.js for PDF rendering, Mammoth.js for DOCX preview
- **Clickable References**: Anchor links to page numbers (#page=N)
- **Highlighting**: Text coordinate-based highlighting when available
- **Fallback**: Page-level navigation if coordinates unavailable
  
References package:
```yaml
- url: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0
  section: ASP.NET Core JWT bearer authentication
  critical: Cookie/JWT integration affects CSRF defenses and token handling

- url: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0
  section: Anti-forgery / CSRF protections
  critical: Required when using cookies for auth on state-changing routes

- url: https://github.com/pgvector/pgvector/blob/master/README.md
  section: Indexing (HNSW/IVFFlat) and query patterns
  critical: ANN indexing changes recall/performance characteristics

- url: https://www.rabbitmq.com/docs/dlx
  section: Dead letter exchanges
  critical: Required for reliable handling of poison messages and max retry enforcement

- url: https://ai.google.dev/gemini-api/docs/embeddings
  section: Embeddings API usage and model capabilities
  critical: Embedding dimensionality and model selection impacts storage and retrieval quality
```

### Technology Decision Matrix
Comparison of primary stack vs a secondary stack (Python-only) across 5 criteria:

| Criterion | Primary: React + .NET API + Python Worker + RabbitMQ + Postgres/pgvector | Secondary: React + Python API + Python Worker + RabbitMQ + Postgres/pgvector |
|---|---:|---:|
| Security & compliance primitives | High | Medium-High |
| RAG/Document processing ecosystem | High | High |
| Performance under enterprise load | High | Medium-High |
| Team availability & hiring pool | [UNCLEAR] | [UNCLEAR] |
| Operational simplicity (one-language) | Medium | High |

## Non-Functional Requirements

### NFR list to generate (overview)
| Req-ID | Type | Summary |
|---|---|---|
| NFR-001 | Performance | Upload acknowledgement within 5 seconds (per file up to 50MB) |
| NFR-002 | Performance | Patient 360 dashboard loads within 3 seconds |
| NFR-003 | Performance | 10-page document processed within 60 seconds (async) |
| NFR-004 | Availability | 99.9% uptime target |
| NFR-005 | Security | Encrypt PHI at rest and in transit |
| NFR-006 | Security | 100% grounding: no uncited entities displayed/stored as final |
| NFR-007 | Security | Immutable audit trail with integrity verification; 7-year retention |
| NFR-008 | Reliability | Retry failed jobs up to 3 attempts with exponential backoff; DLQ |
| NFR-009 | Scalability | Support growth in concurrent users and document volume via worker scale-out |
| NFR-010 | Maintainability | Clear separation of concerns (API vs worker), versioned schemas, migrations |
| NFR-011 | Observability | Health endpoints + structured logs + metrics for queue depth/latency |
| NFR-012 | Usability | >60% reduction in time-on-task via review-and-verify UX |
| NFR-013 | Performance | API response time <200ms for non-processing endpoints |
| NFR-014 | Scalability | Support 50-100 concurrent users without degradation |
| NFR-015 | Performance | Database connection pooling (min 10, max 100 connections) |
| NFR-016 | Usability | Real-time file upload progress tracking with percentage |

### NFR expansion
- NFR-001: System MUST validate and acknowledge each uploaded file (≤50MB) within 5 seconds under normal load.
- NFR-002: System MUST load the Patient 360 Dashboard within 3 seconds for typical patient records.
- NFR-003: System MUST complete processing of an average 10-page PDF (including extraction, chunking, embedding, retrieval, and entity extraction) within 60 seconds asynchronously.
- NFR-004: System MUST target 99.9% uptime excluding planned maintenance windows.
- NFR-005: System MUST encrypt PHI in transit using TLS 1.2+ and encrypt PHI at rest using industry-standard encryption (implementation-specific; e.g., database/storage encryption).
- NFR-006: System MUST enforce "100% grounding" such that every extracted entity presented to users includes source citations and document location metadata; entities lacking citations MUST be rejected or flagged as non-final.
- NFR-007: System MUST maintain an immutable audit trail for all security-sensitive and PHI access events with cryptographic integrity verification and retain authentication audit logs for a minimum of 7 years.
- NFR-008: System MUST provide resilient processing with automatic retries (max 3) and exponential backoff; messages exceeding retries MUST be routed to a dead letter queue for inspection.
- NFR-009: System MUST support horizontal scaling of AI workers to handle increased document processing volume without degrading user-facing responsiveness.
- NFR-010: System MUST be maintainable via modular services (API/worker), versioned API contracts, and schema migration strategy.
- NFR-011: System MUST provide observability via structured logs, health endpoints, and operational metrics (including queue depth, processing latency, error rates).
- NFR-012: System MUST provide an intuitive UI enabling measured >60% reduction in "time-on-task" during UAT.
- NFR-013: System MUST respond to non-processing API requests (e.g., dashboard, user profile) within 200ms under normal load.
- NFR-014: System MUST support 50-100 concurrent users with <10% performance degradation during peak usage.
- NFR-015: System MUST implement database connection pooling with minimum 10 and maximum 100 connections to optimize resource usage.
- NFR-016: System MUST provide real-time upload progress feedback with percentage completion for each file in batch uploads.

## Technical Requirements

### TR list to generate (overview)
| Req-ID | Type | Summary |
|---|---|---|
| TR-001 | Architecture | API-first separation between Web UI, Backend API, and AI Worker |
| TR-002 | AuthN/AuthZ | JWT auth with HttpOnly cookie storage, inactivity timeout, token revocation |
| TR-003 | Security | Rate limiting (login, password reset) with HTTP 429 |
| TR-004 | Processing | RabbitMQ-based async processing with status transitions and DLQ monitoring |
| TR-005 | RAG | Chunking + embeddings + pgvector storage with top-K retrieval |
| TR-006 | LLM | Single-call extraction for all entity categories + conflict identification |
| TR-007 | Validation | Pydantic schema validation for extracted entities |
| TR-008 | Explainability | Persist and surface page/section/coordinates + source text for every entity |
| TR-009 | Codes | ICD-10 and CPT storage with full-text search and fuzzy matching |
| TR-010 | UI | Conflict resolution UI + Accept/Reject workflow for codes |
| TR-011 | Export | Block export until all conflicts resolved; support CSV/JSON/clipboard |
| TR-012 | Observability | Health endpoints on API and worker; structured logging |
| TR-013 | CSRF | CSRF protection for all state-changing operations when cookies used |
| TR-014 | Access control | Enforce Admin vs Standard role separation across endpoints |
| TR-015 | API Design | API versioning strategy with /api/v1/ prefix |
| TR-016 | API Design | Standardized error response format across all endpoints |
| TR-017 | Performance | Pagination for document lists and patient records (page size: 20-50) |
| TR-018 | Security | File virus/malware scanning before processing |
| TR-019 | Security | CORS configuration for Frontend-Backend communication |

### TR expansion
- TR-001: System MUST implement a 3-tier architecture with a Web UI, Backend API, and AI Worker service, with clear boundaries and contracts.
- TR-002: System MUST implement JWT-based authentication with a 15-minute access token and server-side session revocation; JWTs MUST be stored in HttpOnly cookies.
- TR-003: System MUST implement rate limiting for login attempts (5/min/IP) and password reset requests (3/hour/IP) and return HTTP 429 when exceeded.
- TR-004: System MUST enqueue document processing jobs in RabbitMQ and implement status transitions: Pending, Processing, Completed, Failed, Validation_Failed, including retries (max 3) and DLQ handling.
- TR-005: System MUST split merged document text into 500–1000 token chunks with 100-token overlap and store chunk embeddings in PostgreSQL using pgvector.
- TR-006: System MUST extract all core entity categories in a single LLM call and include conflict detection output in a structured format.
- TR-007: System MUST validate extracted entities against versioned Pydantic schemas; invalid payloads MUST be rejected and the job marked Failed with validation errors persisted.
- TR-008: System MUST store and expose source references for each entity including document identifier, page number, section, and coordinates (when available) along with the cited source text.
- TR-009: System MUST store ICD-10 and CPT code catalogs in the database with full-text search and support fuzzy matching from extracted diagnoses/procedures.
- TR-010: System MUST provide a UI for conflict review (side-by-side values + sources) and Accept/Reject controls for billing code suggestions.
- TR-011: System MUST prevent export until all conflicts are resolved and code review decisions are captured; export MUST support CSV and JSON and provide clipboard copy.
- TR-012: System MUST expose health check endpoints on the Backend API (`/health`) and on the AI Worker (`/health`, `/health/ready`, `/health/live`) and implement structured logging suitable for centralized aggregation.
- TR-013: System MUST implement CSRF protections for all state-changing endpoints when cookie-based auth is used.
- TR-014: System MUST enforce RBAC with at least two roles (Admin and Standard) and restrict admin-only features (user management, system health views) from Standard users.
- TR-015: System MUST implement API versioning using URL path prefix (e.g., /api/v1/) to support backward compatibility and future API evolution.
- TR-016: System MUST return standardized error responses with consistent structure: { "error": { "code": "string", "message": "string", "details": [] } }.
- TR-017: System MUST implement pagination for all list endpoints (documents, patients, audit logs) with configurable page size (default 20, max 50) and total count.
- TR-018: System MUST scan uploaded files for viruses and malware before processing; infected files MUST be quarantined and flagged with appropriate user notification.
- TR-019: System MUST configure CORS to allow Frontend origin only, with credentials support for cookie-based auth and appropriate preflight handling.

## Data Requirements

### DR list to generate (overview)
| Req-ID | Type | Summary |
|---|---|---|
| DR-001 | Data model | FHIR-aligned patient-centric schema (patient, documents, extractions) |
| DR-002 | Data integrity | Enforce referential integrity across users, documents, chunks, entities |
| DR-003 | Audit | Persist immutable audit logs with retention >= 7 years |
| DR-004 | Vector security | Prevent PHI leakage in embedding metadata; encrypt embeddings at rest |
| DR-005 | Access control | Row-level security (or equivalent) for document chunks by user permissions |
| DR-006 | Retention | Delete embeddings when source document deleted; support archival policies |
| DR-007 | Codes | Store ICD-10/CPT catalogs with refresh capability |
| DR-008 | Processing | Persist processing jobs with retry counts, timings, and error payloads |
| DR-009 | Conflicts | Persist conflicts and resolution decisions with source provenance |
| DR-010 | Data integrity | Soft delete strategy for documents and patients with audit trail |
| DR-011 | Performance | Database indexing strategy for high-frequency queries |
| DR-012 | Security | Connection string encryption in configuration files |

### DR expansion
- DR-001: System MUST store patient-centric records using a schema that is compatible with future FHIR mapping (e.g., patient demographics, encounters/documents, observations/labs, medications, diagnoses, procedures) while supporting Phase 1 standalone usage.
- DR-002: System MUST enforce referential integrity between users, documents, processing jobs, document chunks, extracted entities, conflicts, and code suggestions.
- DR-003: System MUST store audit logs in an immutable manner (append-only + integrity verification) and retain them for a minimum of 7 years.
- DR-004: System MUST ensure embedding metadata does not store PHI and MUST encrypt embeddings at rest.
- DR-005: System MUST enforce access controls for vector data such that similarity search and chunk retrieval only operate on documents the requesting user is authorized to access.
- DR-006: System MUST delete vector embeddings when the associated source document is deleted and MUST support archival/retention policies for PHI and audits.
- DR-007: System MUST store ICD-10 and CPT code catalogs with a full-text search index on description and support periodic refresh.
- DR-008: System MUST persist processing job metadata (job id, timestamps, processing time, retry count, failure reason, error details) for observability and user-facing status.
- DR-009: System MUST persist detected conflicts as structured records including conflicting values, source citations, and final resolution decisions linked to the resolving user and timestamp.
- DR-010: System MUST implement soft delete for documents and patients using is_deleted flag and deleted_at timestamp; hard delete only after retention period with admin approval.
- DR-011: System MUST create indexes on: user.email, document.patient_id, document.upload_date, processing_job.status, extracted_entity.patient_id, audit_log.timestamp, and pgvector HNSW index on embeddings.
- DR-012: System MUST encrypt database connection strings and API keys in configuration files using environment-specific encryption keys or secure vault services.

## Domain Entities
- User: Authenticated principal (Admin or Standard) with email identity and status (active/locked/deactivated).
- Session: Server-side session state for revocation and inactivity timeout (ties to JWT lifecycle).
- PasswordResetToken: Time-bound token for password reset (1-hour expiry) with audit tracking.
- Document: Uploaded file metadata (original name, MIME type, size, storage path, upload timestamp, owner/tenant context).
- DocumentBatch: A logical upload batch grouping up to 10 documents.
- ProcessingJob: Queue-backed job record for document processing; includes state machine, retries, timestamps, and error details.
- DocumentChunk: Text chunk with embedding vector and non-PHI metadata (document_id, page, section, coordinates, chunk_hash).
- Patient: Aggregated patient identity (MRN and/or name+DOB match) with demographics.
- ExtractedEntity: Normalized entity record (category, name, value, units, effective dates) with citations to one or more chunks.
- EntityCitation: Reference to source document location and cited text for a particular extracted entity.
- Conflict: Detected disagreement for the same field across documents (values + citations + severity).
- ConflictResolution: User decision selecting correct value (or manual override) with audit linkage.
- BillingCodeCatalogItem: ICD-10 or CPT code with description and search index.
- CodeSuggestion: Suggested code linked to extracted diagnoses/procedures with status (pending/accepted/rejected/manually_added).
- AuditLogEvent: Append-only security and PHI access event with integrity verification.
- VectorQueryLog: Audit record of vector searches (who, when, patient/document scope, query metadata).

## Technical Constraints & Assumptions
- Phase 1 storage: uploaded documents stored on local file system; the database stores file paths.
- Upload limits: up to 10 files per batch; max 50MB per file; only PDF and DOCX allowed.
- Document validation: reject password-protected, corrupted/malformed, empty, or suspicious documents.
- RAG parameters: chunk size 500–1000 tokens; 100-token overlap; retrieve top-K (10–15) chunks.
- Grounding constraint: extracted entities without citations are invalid and must not be finalized.
- Deployment model: Phase 1 does not require containerization; RabbitMQ may run as a local/Windows service for development.
- Phase 1 assumes single-organization deployment; multi-tenant isolation deferred to Phase 2.
- Clickable references use PDF.js with page anchors (#page=N); coordinate-based highlighting when metadata available.
- Phase 1 uses 15-minute access tokens without refresh tokens; concurrent sessions allowed but tracked in audit logs.

## Development Workflow
1. Domain entity changes require database migration scripts, referential integrity checks, and unit tests for repositories/services.
2. Backend API endpoints should be specified via OpenAPI and implemented with RBAC, rate limiting, CSRF protection (when cookie-auth), and structured audit logging.
3. AI Worker implementation should be developed against stable job contracts (message schema) and idempotent processing rules to safely support retries.
4. UI implementation should integrate loading/error states for upload and processing status, provide conflict resolution and code review workflows, and enforce export gating when conflicts exist.
5. Every feature that touches PHI must include security validation (authorization, audit logging) and basic performance checks aligned to NFR thresholds.
