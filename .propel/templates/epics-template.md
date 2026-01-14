# Epic - [EP_XXX]

## Epic Summary Table
Provide a table of all in-scope epics. Each epic must have a unique ID (format: EP-###), a concise action-oriented title, and a complete, comma-separated list of mapped requirement IDs from all categories (FR-, NFR-, TR-, DR-, UXR-). Exclude unclear ([UNCLEAR]) items until clarified. Split or add epics if any single epic maps to more than ~12 requirements or mixes unrelated outcomes. Order epics by business value, then dependency priority.

| Epic ID | Epic Title | Mapped Requirement IDs |
|---------|------------|------------------------|
| EP-001 | User Account Access & Authentication | FR-001, FR-002, FR-003, NFR-002, TR-004, UXR-001 |
| EP-002 | Performance & Reliability Foundation | NFR-001, NFR-003, NFR-004, TR-003, DR-004 |
| EP-003 | Core Data & Persistence Layer | TR-001, DR-001, DR-002, DR-003, DR-005 |
| EP-004 | API & Integration Enablement | TR-002, TR-004, FR-002 (reset flow dependency), DR-005 |
| EP-005 | User Experience & Accessibility | UXR-001, UXR-002, UXR-003, UXR-004, UXR-005, UXR-006 |
| EP-006 | Reporting & Administrative Operations | FR-004 (pending clarification), UC-04 (reports), UC-05 (admin panel), NFR-002 (security scope overlap) |
| EP-007 | Security & Compliance Controls | NFR-002, NFR-003, DR-003, DR-004 |

Notes:
1. Replace or expand rows as real scope is finalized.
2. Move any ambiguous ([UNCLEAR]) tagged requirement into a separate backlog refinement list before mapping.
3. Add EP-TECH if bootstrapping a new project (tooling, CI/CD, scaffolding) becomes necessary.
4. Keep traceability: every requirement must appear in exactly one epic (no duplicates) unless explicitly shared (e.g., security).
5. Append new epics with next sequential ID (zero-padded).

## Epic Description
### [EPIC ID]: [Epic Title]
**Priority**: [High/Medium/Low]  
**Business Value**: []  
**Description**: []

**Key Deliverables**:
- [List of deliverables]