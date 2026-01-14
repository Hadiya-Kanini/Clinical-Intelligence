# Design Modelling

## UML Models Overview
These UML diagrams provide a shared, implementation-ready view of the Trust-First Clinical Intelligence Platform. The architectural views summarize key containers, integrations, and data paths derived from `.propel/context/docs/design.md`, while the sequence diagrams map each `UC-XXX` from `.propel/context/docs/spec.md` into time-ordered interactions. Together, they clarify system boundaries, responsibilities, and the "trust-first" invariants (grounding, auditability, human-in-the-loop decisions).

## Architectural Views

### Conceptual Architecture Diagram
```mermaid
flowchart TB
    %% Styling
    classDef actor fill:#add8e6
    classDef core fill:#90ee90
    classDef data fill:#ffffe0
    classDef external fill:#d3d3d3

    subgraph External["External Systems"]
        Gemini["Google Gemini API\n(LLM + Embeddings)"]:::external
        SMTP["SMTP Service\n(Email)"]:::external
    end

    subgraph Users["Actors"]
        Std["Standard User"]:::actor
        Adm["Admin User"]:::actor
    end

    subgraph System["Clinical Intelligence Platform"]
        Web["Web App (React)\nReview & Verify UX"]:::core
        API["Backend API (.NET)\nAuth, RBAC, Orchestration"]:::core
        Worker["AI Worker (Python)\nRAG Processing"]:::core

        MQ["RabbitMQ\nJobs + DLQ"]:::data
        DB[("PostgreSQL + pgvector\nTransactional + Vector")]:::data
        FS[("Local File Storage\nOriginal Documents")]:::data
    end

    Std -->|"HTTPS - Use UI"| Web
    Adm -->|"HTTPS - Admin UI"| Web

    Web -->|"HTTPS / REST"| API

    API -->|"SQL"| DB
    API -->|"File I/O"| FS
    API -.->|"AMQP - Enqueue job"| MQ
    API -.->|"TLS - Send email"| SMTP

    Worker -.->|"AMQP - Consume job"| MQ
    Worker -->|"File I/O - Read doc"| FS
    Worker -->|"SQL - Store/read"| DB
    Worker -.->|"HTTPS - Embed/Extract"| Gemini
```

### Component Architecture Diagram
```mermaid
flowchart LR
    %% Styling
    classDef actor fill:#add8e6
    classDef core fill:#90ee90
    classDef data fill:#ffffe0
    classDef external fill:#d3d3d3

    subgraph FE["Presentation Layer"]
        Web["Web App (React)\nLogin, Upload, 360, Assistant, Admin"]:::core
    end

    subgraph BE["Backend API (.NET)"]
        API["Backend API\nAuth/RBAC, Orchestration, Audit, Export"]:::core
    end

    subgraph WK["AI Worker (Python)"]
        Worker["RAG Worker\nValidate, Chunk, Embed, Extract, Map Codes"]:::core
    end

    subgraph Data["Data Layer"]
        MQ["RabbitMQ\nQueue + DLQ"]:::data
        DB[("PostgreSQL + pgvector")]:::data
        FS[("Local File Storage")]:::data
    end

    subgraph Ext["External"]
        Gemini["Google Gemini API"]:::external
        SMTP["SMTP Service"]:::external
    end

    %% FE -> BE
    Web -->|"HTTPS / REST"| API

    %% BE -> Data/Ext
    API -->|"SQL"| DB
    API -->|"File I/O"| FS
    API -.->|"AMQP - Enqueue job"| MQ
    API -.->|"TLS - Email"| SMTP

    %% Worker -> Data/Ext
    Worker -.->|"AMQP - Consume job"| MQ
    Worker -->|"SQL"| DB
    Worker -->|"File I/O"| FS
    Worker -.->|"HTTPS - LLM/Embeddings"| Gemini
```

### Deployment Architecture Diagram
```plantuml
@startuml Deployment - Hub and Spoke
left to right direction
skinparam linetype ortho
skinparam shadowing false
skinparam packageStyle rectangle

cloud "Shared Services (Hub)" as hub {
  node "Security" as sec #LightGray
  node "Operations" as ops #LightGray
}

package "Workloads (Spokes)" as spokes {
  package "Dev/Test" as dev {
    node "Web App\n(React)" as dev_web #LightGreen
    node "Backend API\n(.NET)" as dev_api #LightGreen
    node "AI Worker\n(Python)" as dev_wkr #LightGreen
    queue "RabbitMQ" as dev_mq #LightYellow
    database "PostgreSQL + pgvector" as dev_db #LightYellow
    node "File Storage" as dev_fs #LightYellow
  }

  package "Prod" as prod {
    node "Web App\n(React)" as prd_web #LightGreen
    node "Backend API\n(.NET)" as prd_api #LightGreen
    node "AI Worker\n(Python)" as prd_wkr #LightGreen
    queue "RabbitMQ" as prd_mq #LightYellow
    database "PostgreSQL + pgvector" as prd_db #LightYellow
    node "File Storage" as prd_fs #LightYellow
  }
}

cloud "External" as ext {
  node "Google Gemini API" as gemini #LightGray
  node "SMTP Service" as smtp #LightGray
}

sec --> dev_api : Policy / Identity
ops --> dev_api : Logs / Metrics

sec --> prd_api : Policy / Identity
ops --> prd_api : Logs / Metrics

dev_web --> dev_api : HTTPS / REST
prd_web --> prd_api : HTTPS / REST

dev_api --> dev_db : SQL
dev_api --> dev_fs : File I/O
dev_api ..> dev_mq : AMQP - Enqueue

dev_wkr ..> dev_mq : AMQP - Consume
dev_wkr --> dev_db : SQL
dev_wkr --> dev_fs : File I/O
dev_wkr ..> gemini : HTTPS - LLM/Embeddings

dev_api ..> smtp : TLS - Email

prd_api --> prd_db : SQL
prd_api --> prd_fs : File I/O
prd_api ..> prd_mq : AMQP - Enqueue

prd_wkr ..> prd_mq : AMQP - Consume
prd_wkr --> prd_db : SQL
prd_wkr --> prd_fs : File I/O
prd_wkr ..> gemini : HTTPS - LLM/Embeddings

prd_api ..> smtp : TLS - Email
@enduml
```

### Data Flow Diagram
```plantuml
@startuml Data Flow - Trust-First Pipeline
left to right direction
skinparam linetype ortho

!define PROCESS rectangle
!define DATASTORE database
!define EXTERNAL component

EXTERNAL "Standard User" as user #LightBlue
EXTERNAL "Admin User" as admin #LightBlue

PROCESS "Web App\n(React UI)" as web #LightGreen
PROCESS "Backend API\n(.NET)" as api #LightGreen
PROCESS "AI Worker\n(RAG Processing)" as worker #LightGreen

DATASTORE "File Storage\nOriginal docs" as fs #LightYellow
DATASTORE "PostgreSQL + pgvector\nPatients, Entities, Vectors" as db #LightYellow
DATASTORE "RabbitMQ\nJobs + DLQ" as mq #LightYellow

EXTERNAL "Google Gemini API" as gemini #LightGray
EXTERNAL "SMTP Service" as smtp #LightGray

user -> web : Login / Upload / Review
admin -> web : Admin actions
web -> api : HTTPS / REST

api -> db : Auth, patients, audits
api -> fs : Store original docs
api -> mq : Enqueue processing job
api -> smtp : Send reset/credential email

mq -> worker : Deliver job
worker -> fs : Read document
worker -> gemini : Embeddings + Extraction
worker -> db : Store chunks, entities, conflicts, codes

api -> db : Load 360 view + conflicts + codes
api -> db : Export finalized record
@enduml
```

### Logical Data Model (ERD)
```mermaid
erDiagram
    USER {
        uuid id PK
        string email
        string role
        string status
        datetime created_at
    }

    SESSION {
        uuid id PK
        uuid user_id FK
        datetime created_at
        datetime expires_at
        datetime last_activity_at
    }

    PASSWORD_RESET_TOKEN {
        uuid id PK
        uuid user_id FK
        string token_hash
        datetime expires_at
        datetime used_at
    }

    PATIENT {
        uuid id PK
        string mrn
        string name
        date dob
        datetime created_at
    }

    DOCUMENT_BATCH {
        uuid id PK
        uuid patient_id FK
        uuid uploaded_by_user_id FK
        datetime uploaded_at
    }

    DOCUMENT {
        uuid id PK
        uuid patient_id FK
        uuid document_batch_id FK
        uuid uploaded_by_user_id FK
        string original_name
        string mime_type
        int size_bytes
        string storage_path
        string status
        datetime uploaded_at
    }

    PROCESSING_JOB {
        uuid id PK
        uuid document_id FK
        string status
        int retry_count
        datetime started_at
        datetime completed_at
    }

    DOCUMENT_CHUNK {
        uuid id PK
        uuid document_id FK
        int page
        string section
        string coordinates
        string chunk_hash
    }

    EXTRACTED_ENTITY {
        uuid id PK
        uuid patient_id FK
        uuid document_id FK
        string category
        string name
        string value
        string units
        datetime effective_at
    }

    ENTITY_CITATION {
        uuid id PK
        uuid extracted_entity_id FK
        uuid document_chunk_id FK
        int page
        string section
        string coordinates
        string cited_text
    }

    CONFLICT {
        uuid id PK
        uuid patient_id FK
        string field
        string severity
        string status
        datetime detected_at
    }

    CONFLICT_RESOLUTION {
        uuid id PK
        uuid conflict_id FK
        uuid resolved_by_user_id FK
        string resolved_value
        datetime resolved_at
    }

    BILLING_CODE_CATALOG_ITEM {
        string code PK
        string code_type
        string description
    }

    CODE_SUGGESTION {
        uuid id PK
        uuid patient_id FK
        uuid extracted_entity_id FK
        string code FK
        string status
        datetime suggested_at
        datetime decided_at
    }

    AUDIT_LOG_EVENT {
        uuid id PK
        uuid user_id FK
        string action_type
        string ip_address
        datetime timestamp
        string integrity_hash
    }

    VECTOR_QUERY_LOG {
        uuid id PK
        uuid user_id FK
        uuid patient_id FK
        datetime timestamp
        string query_hash
    }

    USER ||--o{ SESSION : has
    USER ||--o{ PASSWORD_RESET_TOKEN : requests
    USER ||--o{ DOCUMENT_BATCH : uploads
    USER ||--o{ DOCUMENT : uploads
    USER ||--o{ AUDIT_LOG_EVENT : triggers
    USER ||--o{ VECTOR_QUERY_LOG : runs

    PATIENT ||--o{ DOCUMENT_BATCH : groups
    PATIENT ||--o{ DOCUMENT : contains
    PATIENT ||--o{ EXTRACTED_ENTITY : has
    PATIENT ||--o{ CONFLICT : has
    PATIENT ||--o{ CODE_SUGGESTION : has

    DOCUMENT_BATCH ||--o{ DOCUMENT : includes

    DOCUMENT ||--o{ PROCESSING_JOB : processed_by
    DOCUMENT ||--o{ DOCUMENT_CHUNK : split_into
    DOCUMENT ||--o{ EXTRACTED_ENTITY : yields

    DOCUMENT_CHUNK ||--o{ ENTITY_CITATION : supports
    EXTRACTED_ENTITY ||--o{ ENTITY_CITATION : grounded_by

    CONFLICT ||--o| CONFLICT_RESOLUTION : resolved_by

    EXTRACTED_ENTITY ||--o{ CODE_SUGGESTION : suggests
    BILLING_CODE_CATALOG_ITEM ||--o{ CODE_SUGGESTION : used_by
```

### Use Case Sequence Diagrams

#### UC-001: User Authentication
**Source**: [spec.md#UC-001](.propel/context/docs/spec.md#uc-001-user-authentication)

```mermaid
sequenceDiagram
    participant User as User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL

    Note over User,DB: UC-001 - User Authentication

    User->>Web: Navigate to Login
    User->>Web: Submit email + password
    Web->>API: POST /auth/login
    API->>DB: Validate credentials + status

    alt Invalid credentials
        DB-->>API: Not found / password mismatch
        API->>DB: Increment failed attempts
        API-->>Web: 401 + error
        Web-->>User: Show actionable error
    else Account locked / rate-limited
        DB-->>API: Locked / exceeded limits
        API-->>Web: 429/423 + message
        Web-->>User: Show lockout/retry time
    else Success
        DB-->>API: User + role
        API->>DB: Create session + log LOGIN_SUCCESS
        API-->>Web: Set HttpOnly JWT cookie
        Web-->>User: Redirect to dashboard
    end
```

#### UC-001a: Admin User Creation
**Source**: [spec.md#UC-001a](.propel/context/docs/spec.md#uc-001a-admin-user-creation)

```mermaid
sequenceDiagram
    participant Admin as Admin User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL
    participant SMTP as SMTP Service

    Note over Admin,SMTP: UC-001a - Admin User Creation

    Admin->>Web: Open User Management
    Admin->>Web: Submit new user details
    Web->>API: POST /admin/users
    API->>DB: Validate + check duplicate email

    alt Duplicate email / invalid input
        DB-->>API: Duplicate/validation error
        API-->>Web: 400 + validation details
        Web-->>Admin: Display errors
    else Success
        DB-->>API: OK
        API->>DB: Create user + log USER_CREATED
        API-.->>SMTP: Send credential email
        SMTP-->>API: Delivery status
        API-->>Web: 201 Created
        Web-->>Admin: Show confirmation
    end
```

#### UC-002: Password Reset
**Source**: [spec.md#UC-002](.propel/context/docs/spec.md#uc-002-password-reset)

```mermaid
sequenceDiagram
    participant User as User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL
    participant SMTP as SMTP Service

    Note over User,SMTP: UC-002 - Password Reset

    User->>Web: Open Forgot Password
    User->>Web: Submit email
    Web->>API: POST /auth/password-reset
    API->>DB: Rate-limit + lookup user

    alt Rate limit exceeded
        DB-->>API: Limit exceeded
        API-->>Web: 429
        Web-->>User: Show retry timeframe
    else Continue (generic response)
        DB-->>API: User exists? (not disclosed)
        API->>DB: Create reset token (1h)
        API-.->>SMTP: Send reset link
        API-->>Web: 200 (generic "check email")
        Web-->>User: Show confirmation
    end

    User->>Web: Open link + enter new password
    Web->>API: POST /auth/password-reset/confirm
    API->>DB: Validate token + update password
    API->>DB: Invalidate token + sessions
    API->>DB: Log PASSWORD_RESET_COMPLETED
    API-->>Web: 200 OK
    Web-->>User: Show success + login link
```

#### UC-003: Multi-Document Upload
**Source**: [spec.md#UC-003](.propel/context/docs/spec.md#uc-003-multi-document-upload)

```mermaid
sequenceDiagram
    participant User as Standard User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant FS as File Storage
    participant DB as PostgreSQL
    participant MQ as RabbitMQ

    Note over User,MQ: UC-003 - Multi-Document Upload

    User->>Web: Drag & drop up to 10 files
    Web->>API: POST /documents/batch (files)

    API->>API: Validate type/MIME/size/structure
    alt Any file invalid
        API-->>Web: 400 + per-file errors
        Web-->>User: Display validation results
    else All valid
        API->>FS: Store originals
        API->>DB: Create batch + documents (Pending)
        API-.->>MQ: Enqueue processing jobs
        API->>DB: Log DOCUMENT_UPLOADED (per doc)
        API-->>Web: 202 Accepted + statuses
        Web-->>User: Show Document List (Pending)
    end
```

#### UC-004: Document Processing (RAG Pipeline)
**Source**: [spec.md#UC-004](.propel/context/docs/spec.md#uc-004-document-processing-rag-pipeline)

```mermaid
sequenceDiagram
    participant MQ as RabbitMQ
    participant W as AI Worker (Python)
    participant FS as File Storage
    participant DB as PostgreSQL + pgvector
    participant Gemini as Google Gemini API

    Note over MQ,Gemini: UC-004 - Document Processing (RAG)

    MQ-.->>W: Deliver document job
    W->>DB: Log DOCUMENT_PROCESSING_STARTED
    W->>FS: Load original document
    W->>W: Extract text + positional metadata
    W->>W: Chunk text (500-1000, overlap)

    W-.->>Gemini: Create embeddings
    Gemini-->>W: Embeddings
    W->>DB: Store chunks + vectors

    W->>DB: Similarity search top-K
    DB-->>W: Relevant chunks
    W-.->>Gemini: Extract entities + conflicts
    Gemini-->>W: Grounded entities

    W->>DB: Validate + store entities/conflicts
    W->>DB: Map ICD/CPT + store suggestions
    W->>DB: Update document status (Completed)
    W->>DB: Log DOCUMENT_PROCESSING_COMPLETED
```

#### UC-005: View Patient 360 Dashboard
**Source**: [spec.md#UC-005](.propel/context/docs/spec.md#uc-005-view-patient-360-dashboard)

```mermaid
sequenceDiagram
    participant User as Standard User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL

    Note over User,DB: UC-005 - View Patient 360 Dashboard

    User->>Web: Open Patient 360
    Web->>API: GET /patients/{id}/360
    API->>DB: Load aggregated entities + citations
    API->>DB: Load conflicts + code suggestions

    alt Slow load
        API-->>Web: 200 OK (delayed)
        Web-->>User: Show loading indicator
    else Normal
        DB-->>API: Patient 360 payload
        API-->>Web: 200 OK
        Web-->>User: Render dashboard + sources
    end
```

#### UC-006: Resolve Data Conflicts
**Source**: [spec.md#UC-006](.propel/context/docs/spec.md#uc-006-resolve-data-conflicts)

```mermaid
sequenceDiagram
    participant User as Standard User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL

    Note over User,DB: UC-006 - Resolve Data Conflicts

    User->>Web: Open conflict details
    Web->>API: GET /patients/{id}/conflicts
    API->>DB: Load conflicts + citations
    DB-->>API: Conflict list
    API-->>Web: 200 OK

    User->>Web: Select resolved value
    Web->>API: POST /conflicts/{conflictId}/resolve

    alt Manual override
        API->>DB: Save resolution (manual value)
    else Choose one source value
        API->>DB: Save selected value
    end

    API->>DB: Mark conflict resolved
    API->>DB: Log CONFLICT_RESOLVED
    API-->>Web: 200 OK
    Web-->>User: Show resolved state
```

#### UC-007: Review and Accept/Reject Billing Codes
**Source**: [spec.md#UC-007](.propel/context/docs/spec.md#uc-007-review-and-acceptreject-billing-codes)

```mermaid
sequenceDiagram
    participant Coder as Medical Coder
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL

    Note over Coder,DB: UC-007 - Billing Code Review

    Coder->>Web: Open coding section
    Web->>API: GET /patients/{id}/codes
    API->>DB: Load code suggestions + sources
    DB-->>API: Suggestions
    API-->>Web: 200 OK

    alt No code found
        Web-->>Coder: Show manual entry option
    else Decide
        Coder->>Web: Accept/Reject suggestion
        Web->>API: POST /codes/{suggestionId}/decision
        API->>DB: Update status
        API->>DB: Log ICD/CPT decision
        API-->>Web: 200 OK
        Web-->>Coder: Update UI state
    end
```

#### UC-008: Query AI Clinical Assistant
**Source**: [spec.md#UC-008](.propel/context/docs/spec.md#uc-008-query-ai-clinical-assistant)

```mermaid
sequenceDiagram
    participant User as Standard User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant W as AI Worker (Python)
    participant DB as PostgreSQL + pgvector
    participant Gemini as Google Gemini API

    Note over User,Gemini: UC-008 - Query AI Clinical Assistant

    User->>Web: Submit natural language query
    Web->>API: POST /assistant/query
    API->>W: HTTP - Run RAG query

    W-.->>Gemini: Create query embedding
    Gemini-->>W: Query vector
    W->>DB: Similarity search top-K
    DB-->>W: Relevant chunks
    W-.->>Gemini: Generate answer w/ context
    Gemini-->>W: Answer + citations

    W->>DB: Log VECTOR_QUERY
    W-->>API: Answer + sources
    API-->>Web: 200 OK
    Web-->>User: Render answer + links

    alt No relevant chunks
        W-->>API: "No relevant information found"
        API-->>Web: 200 OK
        Web-->>User: Show empty-state message
    end
```

#### UC-009: Export Patient Data
**Source**: [spec.md#UC-009](.propel/context/docs/spec.md#uc-009-export-patient-data)

```mermaid
sequenceDiagram
    participant User as Standard User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL

    Note over User,DB: UC-009 - Export Patient Data

    User->>Web: Click Finalize & Export
    Web->>API: POST /patients/{id}/export
    API->>DB: Check conflicts resolved

    alt Unresolved conflicts
        DB-->>API: Conflicts remain
        API-->>Web: 409 + details
        Web-->>User: Highlight pending conflicts
    else Ready
        DB-->>API: OK
        API->>DB: Build export payload
        API->>DB: Log PATIENT_DATA_EXPORTED
        API-->>Web: 200 OK + file/content
        Web-->>User: Download / copy output
    end
```

#### UC-010: Admin User Management
**Source**: [spec.md#UC-010](.propel/context/docs/spec.md#uc-010-admin-user-management)

```mermaid
sequenceDiagram
    participant Admin as Admin User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL

    Note over Admin,DB: UC-010 - Admin User Management

    Admin->>Web: Open User Management
    Web->>API: GET /admin/users
    API->>DB: Query users (paginated)
    DB-->>API: User list
    API-->>Web: 200 OK

    alt Update user
        Admin->>Web: Edit user fields
        Web->>API: PUT /admin/users/{id}
        API->>DB: Update user
        API->>DB: Log USER_UPDATED
        API-->>Web: 200 OK
    else Deactivate user
        Admin->>Web: Deactivate user
        Web->>API: POST /admin/users/{id}/deactivate
        API->>DB: Deactivate user
        API->>DB: Log USER_DEACTIVATED
        API-->>Web: 200 OK
    end
```

#### UC-011: View Security Audit Logs
**Source**: [spec.md#UC-011](.propel/context/docs/spec.md#uc-011-view-security-audit-logs)

```mermaid
sequenceDiagram
    participant Admin as Admin User
    participant Web as Web App (React)
    participant API as Backend API (.NET)
    participant DB as PostgreSQL

    Note over Admin,DB: UC-011 - View Security Audit Logs

    Admin->>Web: Open Security Logs
    Web->>API: GET /admin/audit?filters
    API->>DB: Query audit logs (paginated)
    DB-->>API: Audit entries
    API-->>Web: 200 OK
    Web-->>Admin: Render entries

    alt Export filtered logs
        Admin->>Web: Export CSV
        Web->>API: POST /admin/audit/export
        API->>DB: Query filtered set
        API-->>Web: 200 OK + CSV
        Web-->>Admin: Download CSV
    end
```
