# Figma Design Specification - Trust-First Clinical Intelligence Platform

## 1. Figma Specification
**Platform**: Web (Responsive Desktop - 1280px+)

---

## 2. Source References

### Primary Source
| Document | Path | Purpose |
|----------|------|---------|
| Requirements | `.propel/context/docs/spec.md` | Personas, use cases (UC-001 to UC-011), functional requirements (FR-XXX), UX requirements (UXR-XXX) |
| Epics | `.propel/context/docs/epics.md` | UI-impacting epics (EP-001, EP-006, EP-007, EP-009, EP-010, EP-011, EP-013, EP-020, EP-022, EP-024, EP-025, EP-026, EP-027, EP-029, EP-032) |

### Optional Sources
| Document | Path | Purpose |
|----------|------|---------|
| Design | `.propel/context/docs/design.md` | Technical architecture, NFR, TR, DR requirements |

### Related Documents
| Document | Path | Purpose |
|----------|------|---------|
| Design System | `.propel/context/docs/designsystem.md` | Tokens, branding, component specifications |

---

## 3. UX Requirements

*Generated based on use cases with UI impact. These requirements apply to screen implementations.*

### UXR Requirements Table
| UXR-ID | Category | Requirement | Acceptance Criteria | Screens Affected |
|--------|----------|-------------|---------------------|------------------|
| UXR-001 | Visual Design | System MUST provide a clean, professional login page with healthcare-appropriate design and branding | Login page uses medical color palette, professional typography, clear hierarchy | SCR-001 |
| UXR-002 | Error Handling | System MUST display clear, actionable validation errors for invalid login attempts with specific guidance | Error messages specify exact issue (invalid email format, incorrect password, account locked) with recovery steps | SCR-001 |
| UXR-003 | Usability | System MUST provide "Forgot Password" link prominently displayed on login page | Link visible without scrolling, labeled clearly, accessible via keyboard | SCR-001 |
| UXR-004 | Interaction | System MUST show loading indicators during authentication process with progress feedback | Spinner or progress bar appears within 100ms of submit, blocks duplicate submissions | SCR-001 |
| UXR-005 | Usability | System MUST NOT provide public user registration form - only Admin can create users | No "Sign Up" or "Register" links on public pages; user creation only in Admin UI | SCR-001, SCR-014 |
| UXR-006 | Interaction | System MUST display password strength indicators during registration and password reset | Real-time strength meter (weak/medium/strong) with specific improvement suggestions | SCR-003, SCR-014 |
| UXR-007 | Usability | System MUST provide password reset form accessible via email token with clear instructions | Form validates token before display, shows step-by-step instructions, clear error for expired tokens | SCR-003 |
| UXR-008 | Interaction | System MUST show logout confirmation and redirect to login page with success message | Confirmation toast appears, session cleared, redirect within 2 seconds | SCR-001 |
| UXR-009 | Error Handling | System MUST display account lockout messages with clear unlock timeframes | Message shows remaining lockout time, reason for lockout, contact support option | SCR-001 |
| UXR-010 | Error Handling | System MUST provide rate limiting messages with retry timeframes when HTTP 429 is returned | Message shows retry-after time, explains rate limit, suggests waiting period | SCR-001, SCR-002, SCR-003 |
| UXR-011 | Interaction | System MUST display user creation confirmation after Admin creates new user | Success toast with created username, email sent confirmation, option to create another | SCR-014 |
| UXR-012 | Usability | System MUST show password complexity requirements prominently during input | Requirements list visible next to password field, items check off as met | SCR-001, SCR-003, SCR-014 |
| UXR-013 | Usability | System MUST provide intuitive navigation with maximum 3 clicks to any feature | All features accessible within 3 clicks from dashboard, navigation breadcrumbs show path | All screens |
| UXR-014 | Accessibility | System MUST comply with WCAG 2.1 AA accessibility standards | Color contrast ≥4.5:1 for text, ≥3:1 for UI components, keyboard navigation, screen reader support | All screens |
| UXR-015 | Responsiveness | System MUST adapt seamlessly to desktop viewports (1280px+) | Layout optimized for 1280px, 1440px, 1920px breakpoints without horizontal scroll | All screens |
| UXR-016 | Visual Design | System MUST follow consistent design system with defined color palette | All screens use tokens from designsystem.md, no hard-coded colors | All screens |
| UXR-017 | Interaction | System MUST provide real-time feedback for all user actions within 200ms | Button states change, loading indicators appear, success/error feedback within 200ms | All screens |
| UXR-018 | Error Handling | System MUST display clear, actionable error messages with recovery options | Error messages explain what happened, why, and what to do next | All screens |
| UXR-019 | Usability | System MUST provide drag-and-drop file upload functionality | Drag-drop zone highlights on hover, accepts multiple files, shows file count | SCR-005 |
| UXR-020 | Interaction | System MUST display upload progress with percentage indicator | Per-file progress bar with percentage, estimated time remaining, cancel option | SCR-005 |
| UXR-021 | Error Handling | System MUST show clear file validation errors (size, type) | Inline error per file with specific issue (too large, wrong format, corrupted), retry option | SCR-005 |
| UXR-022 | Usability | System MUST allow batch selection of multiple files | File picker supports multi-select, shows selected count before upload | SCR-005 |
| UXR-023 | Usability | System MUST display side-by-side view: original PDF and extracted data | Split-screen layout with PDF viewer on left, extracted data on right, synchronized scrolling | SCR-008 |
| UXR-024 | Interaction | System MUST highlight source text when user clicks extracted data (XAI) | Clicking entity highlights source in PDF, shows page/section metadata, citation tooltip | SCR-008 |
| UXR-025 | Visual Design | System MUST visually distinguish verified, unverified, and modified data | Color-coded badges (green=verified, yellow=unverified, blue=modified), icon indicators | SCR-008 |
| UXR-026 | Interaction | System MUST provide inline editing for all extracted fields | Click-to-edit with validation, save/cancel buttons, preserves source metadata | SCR-008 |
| UXR-027 | Visual Design | System MUST provide clear visual hierarchy for data categories | Grouped sections with headers, collapsible panels, consistent spacing | SCR-008 |
| UXR-028 | Usability | System MUST prominently display conflicts requiring resolution | Conflict banner at top, count badge, red highlight on conflicting fields | SCR-008 |
| UXR-029 | Usability | System MUST show conflicting values side-by-side with source documents | Modal or panel with 2-column comparison, source citations for each value, selection radio buttons | SCR-008 |
| UXR-030 | Interaction | System MUST require explicit user selection to resolve conflicts | Radio buttons or dropdown for selection, "Resolve" button disabled until selection made | SCR-008 |
| UXR-031 | Error Handling | System MUST prevent finalization until all conflicts are resolved | Export button disabled with tooltip explaining pending conflicts, conflict count badge | SCR-008 |
| UXR-032 | Usability | System MUST display suggested codes with descriptions and source references | Code table with code, description, source entity, confidence score, Accept/Reject buttons | SCR-008 |
| UXR-033 | Usability | System MUST provide search/filter for adding new codes | Search input with autocomplete, filter by code type (ICD-10/CPT), recent codes list | SCR-008 |
| UXR-034 | Interaction | System MUST allow one-click removal of suggested codes | Reject button with confirmation for bulk actions, undo option | SCR-008 |
| UXR-035 | Visual Design | System MUST show code validation status (valid/invalid/deprecated) | Status badges with color coding, tooltip with validation details | SCR-008 |
| UXR-036 | Usability | System MUST provide chat-style interface for natural language queries | Chat panel with message history, input field, send button, suggested queries | SCR-009 |
| UXR-037 | Interaction | System MUST display typing indicator during AI response generation | Animated dots or spinner in chat, "AI is thinking..." message | SCR-009 |
| UXR-038 | Usability | System MUST show source citations as clickable links | Citations as hyperlinks in chat messages, clicking opens source document at location | SCR-009 |
| UXR-039 | Usability | System MUST provide suggested queries for common use cases | Quick-action buttons for common queries ("Summarize medications", "Show lab results") | SCR-009 |
| UXR-040 | Visual Design | System MUST provide visual charts and graphs for metrics | Bar charts, line graphs, pie charts for metrics, interactive tooltips | SCR-012, SCR-016 |
| UXR-041 | Usability | System MUST support date range filtering for all metrics | Date range picker with presets (Last 7 days, Last 30 days, Custom), apply/reset buttons | SCR-012, SCR-016 |
| UXR-042 | Interaction | System MUST provide drill-down capabilities from summary to detail views | Clickable chart elements, breadcrumb navigation, back button | SCR-012, SCR-016 |
| UXR-043 | Interaction | System MUST display real-time updates for active processing | Live status updates, auto-refresh every 5 seconds, manual refresh button | SCR-006 |
| UXR-044 | Usability | System MUST provide searchable, sortable user management table | Search input, column sorting, filter by role/status, pagination | SCR-014 |
| UXR-045 | Usability | System MUST provide filterable audit log viewer | Multi-select filters (user, action type, date range, IP), clear filters button | SCR-015 |
| UXR-046 | Interaction | System MUST provide export functionality for reports and logs | Export button with format options (CSV, JSON), download progress indicator | SCR-008, SCR-015 |

### UXR Categories
- **Usability**: Navigation, discoverability, efficiency (max 3 clicks, clear hierarchy)
- **Accessibility**: WCAG 2.1 AA compliance, assistive technology support
- **Responsiveness**: Breakpoint behavior, viewport adaptation
- **Visual Design**: Design system adherence, consistency
- **Interaction**: Feedback, loading states, animations (response within 200ms)
- **Error Handling**: Error messages, recovery paths

### UXR Derivation Logic
- **Usability UXR**: Derived from UC-XXX success paths (navigation depth, discoverability)
- **Accessibility UXR**: Derived from WCAG 2.1 AA standards + designsystem.md constraints
- **Responsiveness UXR**: Derived from platform targets (desktop 1280px+)
- **Visual Design UXR**: Derived from designsystem.md token requirements
- **Interaction UXR**: Derived from flow complexity + state transitions
- **Error Handling UXR**: Derived from UC-XXX alternative/exception paths

### UXR Numbering Convention
- UXR-001 to UXR-046: Project-wide requirements (existing from spec.md)

---

## 4. Personas Summary

*Derived from spec.md - Reference only*

| Persona | Role | Primary Goals | Key Screens |
|---------|------|---------------|-------------|
| Standard User | Clinician, Medical Coder, Healthcare Staff | Upload documents, review extracted data, resolve conflicts, finalize patient records | SCR-004, SCR-005, SCR-006, SCR-007, SCR-008, SCR-009, SCR-010, SCR-011 |
| Admin User | System Administrator (Single Static Account) | Manage user accounts, monitor security, oversee system health | SCR-012, SCR-013, SCR-014, SCR-015, SCR-016 |

---

## 5. Information Architecture

### Site Map
```
Clinical Intelligence Platform
+-- Public (Unauthenticated)
|   +-- Login (SCR-001)
|   +-- Forgot Password (SCR-002)
|   +-- Reset Password (SCR-003)
+-- Standard User (Authenticated)
|   +-- Dashboard (SCR-004)
|   +-- Document Upload (SCR-005)
|   +-- Document List (SCR-006)
|   +-- Profile (SCR-007)
|   +-- Patient 360 View (SCR-008)
|   +-- AI Clinical Assistant (SCR-009)
|   +-- Export (SCR-010)
|   +-- Productivity Dashboard (SCR-011)
+-- Admin User (Authenticated)
|   +-- Admin Dashboard (SCR-012)
|   +-- System Health (SCR-013)
|   +-- User Management (SCR-014)
|   +-- Security Audit Logs (SCR-015)
|   +-- Analytics Dashboard (SCR-016)
```

### Navigation Patterns
| Pattern | Type | Platform Behavior |
|---------|------|-------------------|
| Primary Nav | Sidebar | Desktop: Persistent left sidebar with role-based menu items |
| Secondary Nav | Breadcrumb | Desktop: Top breadcrumb trail showing current location |
| Utility Nav | Header | Desktop: Top-right user menu (Profile, Logout) |

---

## 6. Screen Inventory

*All screens derived from use cases in spec.md*

### Screen List
| Screen ID | Screen Name | Derived From | Personas Covered | Priority | States Required |
|-----------|-------------|--------------|------------------|----------|-----------------|
| SCR-001 | Login | UC-001 | Standard User, Admin User | P0 | Default, Loading, Error, Validation, Lockout |
| SCR-002 | Forgot Password | UC-002 | Standard User, Admin User | P0 | Default, Loading, Success, Error, RateLimited |
| SCR-003 | Reset Password | UC-002 | Standard User, Admin User | P0 | Default, Loading, Success, Error, TokenExpired |
| SCR-004 | Standard User Dashboard | UC-005 (entry), UC-003 (entry) | Standard User | P0 | Default, Loading, Empty, Error |
| SCR-005 | Document Upload | UC-003 | Standard User | P0 | Default, Uploading, Success, Error, Validation |
| SCR-006 | Document List | UC-003 | Standard User | P0 | Default, Loading, Empty, Error, Processing |
| SCR-007 | Profile | - | Standard User, Admin User | P1 | Default, Loading, Editing, Success, Error |
| SCR-008 | Patient 360 View | UC-005, UC-006, UC-007 | Standard User | P0 | Default, Loading, Empty, Error, Conflict, Verification |
| SCR-009 | AI Clinical Assistant | UC-008 | Standard User | P1 | Default, Loading, Empty, Error, Responding |
| SCR-010 | Export | UC-009 | Standard User | P0 | Default, Exporting, Success, Error, Blocked |
| SCR-011 | Productivity Dashboard | - | Standard User | P2 | Default, Loading, Empty, Error |
| SCR-012 | Admin Dashboard | UC-010 (entry), UC-011 (entry) | Admin User | P0 | Default, Loading, Empty, Error |
| SCR-013 | System Health | - | Admin User | P1 | Default, Loading, Error, Degraded |
| SCR-014 | User Management | UC-001a, UC-010 | Admin User | P0 | Default, Loading, Empty, Creating, Editing, Success, Error |
| SCR-015 | Security Audit Logs | UC-011 | Admin User | P1 | Default, Loading, Empty, Error, Filtering |
| SCR-016 | Analytics Dashboard | - | Admin User | P2 | Default, Loading, Empty, Error |

### Priority Legend
- **P0**: Critical path (must-have for MVP)
- **P1**: Core functionality (high priority)
- **P2**: Important features (medium priority)

### Screen-to-Persona Coverage Matrix
| Screen | Standard User | Admin User | Notes |
|--------|---------------|------------|-------|
| SCR-001 | Primary | Primary | Entry point for all users |
| SCR-002 | Primary | Primary | Password recovery for all users |
| SCR-003 | Primary | Primary | Password reset for all users |
| SCR-004 | Primary | - | Standard user home |
| SCR-005 | Primary | - | Document upload workflow |
| SCR-006 | Primary | - | Document status tracking |
| SCR-007 | Primary | Secondary | Profile management |
| SCR-008 | Primary | - | Core patient data review |
| SCR-009 | Primary | - | AI-powered query interface |
| SCR-010 | Primary | - | Data export workflow |
| SCR-011 | Primary | - | Productivity metrics |
| SCR-012 | - | Primary | Admin home |
| SCR-013 | - | Primary | System monitoring |
| SCR-014 | - | Primary | User account management |
| SCR-015 | - | Primary | Security monitoring |
| SCR-016 | - | Primary | Business analytics |

### Modal/Overlay Inventory
| Name | Type | Trigger | Parent Screen(s) | Priority |
|------|------|---------|-----------------|----------|
| Conflict Resolution Modal | Modal | Click "Resolve Conflict" | SCR-008 | P0 |
| Code Search Modal | Modal | Click "Add Code" | SCR-008 | P0 |
| Export Options Modal | Modal | Click "Export" | SCR-008, SCR-010 | P0 |
| User Creation Modal | Modal | Click "Create User" | SCR-014 | P0 |
| User Edit Modal | Modal | Click "Edit User" | SCR-014 | P0 |
| Logout Confirmation Modal | Modal | Click "Logout" | All authenticated screens | P0 |
| Delete Confirmation Dialog | Dialog | Delete actions | SCR-006, SCR-014 | P0 |
| Document Preview Drawer | Drawer | Click document name | SCR-006 | P1 |
| Audit Log Detail Drawer | Drawer | Click log entry | SCR-015 | P1 |

---

## 7. Content & Tone

### Voice & Tone
- **Overall Tone**: Professional, trustworthy, clinical
- **Error Messages**: Helpful, non-blaming, actionable with clear recovery steps
- **Empty States**: Encouraging, guiding, with clear CTA to get started
- **Success Messages**: Brief, confirmatory, next-action oriented

### Content Guidelines
- **Headings**: Sentence case (e.g., "Upload documents")
- **CTAs**: Action-oriented, specific verbs (e.g., "Upload documents", "Resolve conflict", "Accept code")
- **Labels**: Concise, descriptive, medical terminology when appropriate
- **Placeholder Text**: Helpful examples (e.g., "Enter patient MRN or name")

---

## 8. Data & Edge Cases

### Data Scenarios
| Scenario | Description | Handling |
|----------|-------------|----------|
| No Data | User has no uploaded documents | Empty state with "Upload your first document" CTA |
| First Use | New user, no history | Onboarding tooltip tour highlighting key features |
| Large Data | 100+ documents per patient | Pagination (20 items/page), virtualized scrolling for performance |
| Slow Connection | >3s load time | Skeleton screens, progressive loading, offline indicator |
| Processing Delay | Document processing >60s | Real-time status updates, estimated completion time |

### Edge Cases
| Case | Screen(s) Affected | Solution |
|------|-------------------|----------|
| Long patient names | SCR-008, SCR-006 | Truncation with tooltip on hover |
| Missing document preview | SCR-006, SCR-008 | Fallback icon with document type indicator |
| Form validation | All forms | Inline error messages with specific field highlighting |
| Session timeout | All authenticated | Modal with re-login option, preserves unsaved work |
| Concurrent edits | SCR-008 | Optimistic locking with conflict notification |
| Large file uploads | SCR-005 | Chunked upload with resume capability |

---

## 9. Branding & Visual Direction

*See `designsystem.md` for all design tokens (colors, typography, spacing, shadows, etc.)*

### Branding Assets
- **Logo**: Clinical Intelligence Platform logo (primary + icon-only variants)
- **Icon Style**: Outlined, medical/healthcare themed (Lucide icons)
- **Illustration Style**: Minimal, professional medical illustrations for empty states
- **Photography Style**: Not applicable for Phase 1

---

## 10. Component Specifications

*Component specifications defined in designsystem.md. Requirements per screen listed below.*

### Component Library Reference
**Source**: `.propel/context/docs/designsystem.md` (Component Specifications section)

### Required Components per Screen
| Screen ID | Components Required | Notes |
|-----------|---------------------|-------|
| SCR-001 | TextField (2), Button (1), Link (1), Alert (1) | Email, password inputs, login button, forgot password link, error alert |
| SCR-002 | TextField (1), Button (1), Link (1), Alert (1) | Email input, submit button, back to login link, success/error alert |
| SCR-003 | TextField (2), Button (1), PasswordStrength (1), Alert (1) | New password, confirm password, submit button, strength indicator, alert |
| SCR-004 | Card (4), Header (1), Sidebar (1), Button (2) | Quick stats cards, page header, navigation sidebar, action buttons |
| SCR-005 | FileUpload (1), ProgressBar (N), Button (2), Alert (1) | Drag-drop zone, per-file progress, upload/cancel buttons, validation alerts |
| SCR-006 | Table (1), Badge (N), Button (3), Pagination (1) | Document list table, status badges, action buttons, pagination control |
| SCR-007 | TextField (5), Button (2), Avatar (1) | Profile fields, save/cancel buttons, user avatar |
| SCR-008 | Card (10+), Table (3), Badge (N), Button (10+), Modal (3), Tabs (1), PDFViewer (1) | Complex multi-section layout with patient data, codes, conflicts |
| SCR-009 | ChatMessage (N), TextField (1), Button (1), Badge (N) | Chat interface with message history, input field, send button, suggested queries |
| SCR-010 | RadioGroup (1), Button (2), Alert (1) | Export format selection, export/cancel buttons, status alert |
| SCR-011 | Card (6), Chart (4), DateRangePicker (1) | Metric cards, charts (bar, line, pie), date filter |
| SCR-012 | Card (6), Header (1), Sidebar (1), Button (2) | Admin dashboard cards, navigation |
| SCR-013 | Card (4), Badge (N), Table (1), Chart (2) | Health status cards, service status badges, metrics table, health charts |
| SCR-014 | Table (1), Button (4), Modal (2), TextField (5), Select (1) | User table, CRUD buttons, create/edit modals, form fields |
| SCR-015 | Table (1), TextField (1), Select (3), DateRangePicker (1), Button (2), Pagination (1) | Audit log table, search input, filter dropdowns, date filter, export/clear buttons |
| SCR-016 | Card (8), Chart (6), DateRangePicker (1), Table (2) | Analytics cards, various chart types, date filter, detail tables |

### Component Summary
| Category | Components | Variants |
|----------|------------|----------|
| Actions | Button, IconButton, Link | Primary, Secondary, Tertiary, Ghost x S/M/L x States |
| Inputs | TextField, Select, Checkbox, Radio, Toggle, FileUpload, DateRangePicker | States + Sizes |
| Navigation | Header, Sidebar, Tabs, Breadcrumb, Pagination | Platform variants |
| Content | Card, ListItem, Table, Badge, Avatar, PDFViewer | Content variants |
| Feedback | Modal, Drawer, Toast, Alert, Skeleton, ProgressBar, PasswordStrength | Types + States |
| Data Visualization | Chart (Bar, Line, Pie, Donut), Metric Card | Interactive variants |
| Chat | ChatMessage, ChatInput, SuggestedQuery | Sender variants (user/AI) |

### Component Constraints
- Use only components from designsystem.md
- No custom components without approval
- All components must support all defined states (Default, Hover, Focus, Active, Disabled, Loading)
- Follow naming convention: `C/<Category>/<Name>`

---

## 11. Prototype Flows

*Flows derived from use cases in spec.md. Each flow notes which personas it covers.*

### Flow: FL-001 - User Authentication
**Flow ID**: FL-001
**Derived From**: UC-001
**Personas Covered**: Standard User, Admin User
**Description**: User logs into the platform with email and password

#### Flow Sequence
```
1. Entry: Login (SCR-001) / Default
   - User enters email and password
   |
   v
2. Validation: Login (SCR-001) / Validation
   - System validates input format
   |
   v
3. Authentication: Login (SCR-001) / Loading
   - System authenticates credentials
   |
   v
4. Decision Point:
   +-- Success (Standard User) -> Dashboard (SCR-004) / Default
   +-- Success (Admin User) -> Admin Dashboard (SCR-012) / Default
   +-- Invalid Credentials -> Login (SCR-001) / Error
   +-- Account Locked -> Login (SCR-001) / Lockout
   +-- Rate Limited -> Login (SCR-001) / Error (429 message)
```

#### Required Interactions
- Email/password input with validation
- Submit button with loading state
- Error message display
- Forgot password link navigation

---

### Flow: FL-002 - Password Reset
**Flow ID**: FL-002
**Derived From**: UC-002
**Personas Covered**: Standard User, Admin User
**Description**: User resets forgotten password via email token

#### Flow Sequence
```
1. Entry: Login (SCR-001) / Default
   - User clicks "Forgot Password" link
   |
   v
2. Request: Forgot Password (SCR-002) / Default
   - User enters email address
   |
   v
3. Processing: Forgot Password (SCR-002) / Loading
   - System generates reset token and sends email
   |
   v
4. Decision Point:
   +-- Success -> Forgot Password (SCR-002) / Success
   +-- Rate Limited -> Forgot Password (SCR-002) / RateLimited
   |
   v
5. Email Link: User clicks reset link in email
   |
   v
6. Reset Form: Reset Password (SCR-003) / Default
   - User enters new password with strength indicator
   |
   v
7. Processing: Reset Password (SCR-003) / Loading
   - System validates and updates password
   |
   v
8. Decision Point:
   +-- Success -> Reset Password (SCR-003) / Success -> Login (SCR-001) / Default
   +-- Token Expired -> Reset Password (SCR-003) / TokenExpired
   +-- Validation Error -> Reset Password (SCR-003) / Error
```

#### Required Interactions
- Email input with validation
- Password input with strength indicator
- Token validation
- Success/error messaging

---

### Flow: FL-003 - Document Upload & Processing
**Flow ID**: FL-003
**Derived From**: UC-003, UC-004
**Personas Covered**: Standard User
**Description**: User uploads clinical documents for processing

#### Flow Sequence
```
1. Entry: Dashboard (SCR-004) / Default
   - User clicks "Upload Documents"
   |
   v
2. Upload: Document Upload (SCR-005) / Default
   - User drags/drops or selects files (up to 10)
   |
   v
3. Validation: Document Upload (SCR-005) / Validation
   - System validates file type, size, integrity
   |
   v
4. Decision Point:
   +-- Valid Files -> Document Upload (SCR-005) / Uploading
   +-- Invalid Files -> Document Upload (SCR-005) / Error (per-file errors)
   |
   v
5. Upload Progress: Document Upload (SCR-005) / Uploading
   - Per-file progress bars with percentage
   |
   v
6. Success: Document Upload (SCR-005) / Success
   - Confirmation message, redirect to Document List
   |
   v
7. Status Tracking: Document List (SCR-006) / Processing
   - Real-time status updates (Pending -> Processing -> Completed/Failed)
   |
   v
8. Decision Point:
   +-- Completed -> Document List (SCR-006) / Default (view Patient 360 option)
   +-- Failed -> Document List (SCR-006) / Error (retry option)
```

#### Required Interactions
- Drag-and-drop file upload
- Multi-file selection
- Per-file validation and progress
- Real-time status updates
- Navigation to Patient 360 View

---

### Flow: FL-004 - Patient 360 Review & Verification
**Flow ID**: FL-004
**Derived From**: UC-005, UC-006, UC-007
**Personas Covered**: Standard User
**Description**: User reviews extracted patient data, resolves conflicts, and reviews billing codes

#### Flow Sequence
```
1. Entry: Document List (SCR-006) / Default
   - User clicks "View Patient 360" for completed document
   |
   v
2. Loading: Patient 360 View (SCR-008) / Loading
   - System loads aggregated patient data (<3s per NFR-002)
   |
   v
3. Review: Patient 360 View (SCR-008) / Default
   - User reviews patient profile, clinical content, source references
   |
   v
4. Decision Point:
   +-- Conflicts Detected -> Patient 360 View (SCR-008) / Conflict
   +-- No Conflicts -> Continue to Code Review
   |
   v
5. Conflict Resolution: Patient 360 View (SCR-008) / Conflict
   - User clicks "Resolve Conflict"
   - Modal shows side-by-side comparison with sources
   - User selects correct value
   |
   v
6. Code Review: Patient 360 View (SCR-008) / Verification
   - User reviews suggested ICD-10/CPT codes
   - Clicks source references to verify
   - Accepts/Rejects codes or manually adds
   |
   v
7. Finalization: Patient 360 View (SCR-008) / Default
   - All conflicts resolved, codes reviewed
   - Export button enabled
```

#### Required Interactions
- Side-by-side PDF viewer and data display
- Clickable source references with highlighting
- Conflict resolution modal with selection
- Code Accept/Reject buttons
- Inline editing for extracted fields
- Export button state management

---

### Flow: FL-005 - AI Clinical Assistant Query
**Flow ID**: FL-005
**Derived From**: UC-008
**Personas Covered**: Standard User
**Description**: User queries AI assistant about patient documents

#### Flow Sequence
```
1. Entry: Patient 360 View (SCR-008) / Default
   - User clicks "AI Assistant" tab or button
   |
   v
2. Chat Interface: AI Clinical Assistant (SCR-009) / Default
   - User sees suggested queries or enters custom query
   |
   v
3. Query Submission: AI Clinical Assistant (SCR-009) / Loading
   - User clicks suggested query or types and sends
   |
   v
4. Processing: AI Clinical Assistant (SCR-009) / Responding
   - Typing indicator appears
   - System performs vector search and LLM generation
   |
   v
5. Response: AI Clinical Assistant (SCR-009) / Default
   - AI response appears with source citations
   - Citations are clickable links to document sections
   |
   v
6. Follow-up: User can ask additional questions (loop to step 2)
```

#### Required Interactions
- Chat message display (user/AI)
- Text input with send button
- Suggested query buttons
- Typing indicator
- Clickable citation links
- Message history scrolling

---

### Flow: FL-006 - Data Export
**Flow ID**: FL-006
**Derived From**: UC-009
**Personas Covered**: Standard User
**Description**: User exports finalized patient data

#### Flow Sequence
```
1. Entry: Patient 360 View (SCR-008) / Default
   - User clicks "Finalize & Export"
   |
   v
2. Validation: Patient 360 View (SCR-008) / Verification
   - System validates all conflicts resolved
   |
   v
3. Decision Point:
   +-- Conflicts Remain -> Patient 360 View (SCR-008) / Blocked (error message)
   +-- All Resolved -> Export (SCR-010) / Default
   |
   v
4. Format Selection: Export (SCR-010) / Default
   - User selects CSV, JSON, or Clipboard
   |
   v
5. Processing: Export (SCR-010) / Exporting
   - System generates export file
   |
   v
6. Success: Export (SCR-010) / Success
   - Download initiated or clipboard copied
   - Success message displayed
```

#### Required Interactions
- Export button state (enabled/disabled)
- Format selection (radio buttons)
- Export progress indicator
- Download trigger or clipboard copy
- Success confirmation

---

### Flow: FL-007 - Admin User Management
**Flow ID**: FL-007
**Derived From**: UC-001a, UC-010
**Personas Covered**: Admin User
**Description**: Admin creates, updates, or deactivates user accounts

#### Flow Sequence
```
1. Entry: Admin Dashboard (SCR-012) / Default
   - Admin clicks "User Management"
   |
   v
2. User List: User Management (SCR-014) / Default
   - System displays all users with search/filter
   |
   v
3. Action Selection:
   +-- Create User -> User Management (SCR-014) / Creating
   +-- Edit User -> User Management (SCR-014) / Editing
   +-- Deactivate User -> Confirmation Dialog
   |
   v
4. Create User: User Management (SCR-014) / Creating
   - Admin fills form (name, email, role)
   - System validates (duplicate email check)
   |
   v
5. Processing: User Management (SCR-014) / Loading
   - System creates account, sends credentials email
   |
   v
6. Decision Point:
   +-- Success -> User Management (SCR-014) / Success
   +-- Validation Error -> User Management (SCR-014) / Error
```

#### Required Interactions
- User table with search/sort/filter
- Create/Edit modals with form validation
- Deactivate confirmation dialog
- Success/error messaging
- Email sent confirmation

---

### Flow: FL-008 - Security Audit Log Review
**Flow ID**: FL-008
**Derived From**: UC-011
**Personas Covered**: Admin User
**Description**: Admin reviews security audit logs with filtering

#### Flow Sequence
```
1. Entry: Admin Dashboard (SCR-012) / Default
   - Admin clicks "Security Logs"
   |
   v
2. Log Viewer: Security Audit Logs (SCR-015) / Default
   - System displays paginated audit logs
   |
   v
3. Filtering: Security Audit Logs (SCR-015) / Filtering
   - Admin applies filters (user, action, date range, IP)
   - System updates results in real-time
   |
   v
4. Detail View: Security Audit Logs (SCR-015) / Default
   - Admin clicks log entry to view full details
   - Drawer opens with action-specific metadata
   |
   v
5. Export (Optional): Security Audit Logs (SCR-015) / Default
   - Admin clicks "Export" for filtered results
   - CSV download initiated
```

#### Required Interactions
- Paginated table with sorting
- Multi-select filters
- Date range picker
- Log detail drawer
- Export to CSV

---

## 12. Export Requirements

### JPG Export Settings
| Setting | Value |
|---------|-------|
| Format | JPG |
| Quality | High (85%) |
| Scale - Desktop | 2x |
| Color Profile | sRGB |

### Export Naming Convention
`ClinicalIntelligence__Web__<ScreenName>__<State>__v1.jpg`

### Export Manifest
| Screen | State | Platform | Filename |
|--------|-------|----------|----------|
| Login | Default | Web | ClinicalIntelligence__Web__Login__Default__v1.jpg |
| Login | Loading | Web | ClinicalIntelligence__Web__Login__Loading__v1.jpg |
| Login | Error | Web | ClinicalIntelligence__Web__Login__Error__v1.jpg |
| Login | Validation | Web | ClinicalIntelligence__Web__Login__Validation__v1.jpg |
| Login | Lockout | Web | ClinicalIntelligence__Web__Login__Lockout__v1.jpg |
| Forgot Password | Default | Web | ClinicalIntelligence__Web__ForgotPassword__Default__v1.jpg |
| Forgot Password | Loading | Web | ClinicalIntelligence__Web__ForgotPassword__Loading__v1.jpg |
| Forgot Password | Success | Web | ClinicalIntelligence__Web__ForgotPassword__Success__v1.jpg |
| Forgot Password | Error | Web | ClinicalIntelligence__Web__ForgotPassword__Error__v1.jpg |
| Forgot Password | RateLimited | Web | ClinicalIntelligence__Web__ForgotPassword__RateLimited__v1.jpg |
| Reset Password | Default | Web | ClinicalIntelligence__Web__ResetPassword__Default__v1.jpg |
| Reset Password | Loading | Web | ClinicalIntelligence__Web__ResetPassword__Loading__v1.jpg |
| Reset Password | Success | Web | ClinicalIntelligence__Web__ResetPassword__Success__v1.jpg |
| Reset Password | Error | Web | ClinicalIntelligence__Web__ResetPassword__Error__v1.jpg |
| Reset Password | TokenExpired | Web | ClinicalIntelligence__Web__ResetPassword__TokenExpired__v1.jpg |
| Dashboard | Default | Web | ClinicalIntelligence__Web__Dashboard__Default__v1.jpg |
| Dashboard | Loading | Web | ClinicalIntelligence__Web__Dashboard__Loading__v1.jpg |
| Dashboard | Empty | Web | ClinicalIntelligence__Web__Dashboard__Empty__v1.jpg |
| Dashboard | Error | Web | ClinicalIntelligence__Web__Dashboard__Error__v1.jpg |
| Document Upload | Default | Web | ClinicalIntelligence__Web__DocumentUpload__Default__v1.jpg |
| Document Upload | Uploading | Web | ClinicalIntelligence__Web__DocumentUpload__Uploading__v1.jpg |
| Document Upload | Success | Web | ClinicalIntelligence__Web__DocumentUpload__Success__v1.jpg |
| Document Upload | Error | Web | ClinicalIntelligence__Web__DocumentUpload__Error__v1.jpg |
| Document Upload | Validation | Web | ClinicalIntelligence__Web__DocumentUpload__Validation__v1.jpg |
| Document List | Default | Web | ClinicalIntelligence__Web__DocumentList__Default__v1.jpg |
| Document List | Loading | Web | ClinicalIntelligence__Web__DocumentList__Loading__v1.jpg |
| Document List | Empty | Web | ClinicalIntelligence__Web__DocumentList__Empty__v1.jpg |
| Document List | Error | Web | ClinicalIntelligence__Web__DocumentList__Error__v1.jpg |
| Document List | Processing | Web | ClinicalIntelligence__Web__DocumentList__Processing__v1.jpg |
| Profile | Default | Web | ClinicalIntelligence__Web__Profile__Default__v1.jpg |
| Profile | Loading | Web | ClinicalIntelligence__Web__Profile__Loading__v1.jpg |
| Profile | Editing | Web | ClinicalIntelligence__Web__Profile__Editing__v1.jpg |
| Profile | Success | Web | ClinicalIntelligence__Web__Profile__Success__v1.jpg |
| Profile | Error | Web | ClinicalIntelligence__Web__Profile__Error__v1.jpg |
| Patient 360 View | Default | Web | ClinicalIntelligence__Web__Patient360View__Default__v1.jpg |
| Patient 360 View | Loading | Web | ClinicalIntelligence__Web__Patient360View__Loading__v1.jpg |
| Patient 360 View | Empty | Web | ClinicalIntelligence__Web__Patient360View__Empty__v1.jpg |
| Patient 360 View | Error | Web | ClinicalIntelligence__Web__Patient360View__Error__v1.jpg |
| Patient 360 View | Conflict | Web | ClinicalIntelligence__Web__Patient360View__Conflict__v1.jpg |
| Patient 360 View | Verification | Web | ClinicalIntelligence__Web__Patient360View__Verification__v1.jpg |
| AI Clinical Assistant | Default | Web | ClinicalIntelligence__Web__AIClinicalAssistant__Default__v1.jpg |
| AI Clinical Assistant | Loading | Web | ClinicalIntelligence__Web__AIClinicalAssistant__Loading__v1.jpg |
| AI Clinical Assistant | Empty | Web | ClinicalIntelligence__Web__AIClinicalAssistant__Empty__v1.jpg |
| AI Clinical Assistant | Error | Web | ClinicalIntelligence__Web__AIClinicalAssistant__Error__v1.jpg |
| AI Clinical Assistant | Responding | Web | ClinicalIntelligence__Web__AIClinicalAssistant__Responding__v1.jpg |
| Export | Default | Web | ClinicalIntelligence__Web__Export__Default__v1.jpg |
| Export | Exporting | Web | ClinicalIntelligence__Web__Export__Exporting__v1.jpg |
| Export | Success | Web | ClinicalIntelligence__Web__Export__Success__v1.jpg |
| Export | Error | Web | ClinicalIntelligence__Web__Export__Error__v1.jpg |
| Export | Blocked | Web | ClinicalIntelligence__Web__Export__Blocked__v1.jpg |
| Productivity Dashboard | Default | Web | ClinicalIntelligence__Web__ProductivityDashboard__Default__v1.jpg |
| Productivity Dashboard | Loading | Web | ClinicalIntelligence__Web__ProductivityDashboard__Loading__v1.jpg |
| Productivity Dashboard | Empty | Web | ClinicalIntelligence__Web__ProductivityDashboard__Empty__v1.jpg |
| Productivity Dashboard | Error | Web | ClinicalIntelligence__Web__ProductivityDashboard__Error__v1.jpg |
| Admin Dashboard | Default | Web | ClinicalIntelligence__Web__AdminDashboard__Default__v1.jpg |
| Admin Dashboard | Loading | Web | ClinicalIntelligence__Web__AdminDashboard__Loading__v1.jpg |
| Admin Dashboard | Empty | Web | ClinicalIntelligence__Web__AdminDashboard__Empty__v1.jpg |
| Admin Dashboard | Error | Web | ClinicalIntelligence__Web__AdminDashboard__Error__v1.jpg |
| System Health | Default | Web | ClinicalIntelligence__Web__SystemHealth__Default__v1.jpg |
| System Health | Loading | Web | ClinicalIntelligence__Web__SystemHealth__Loading__v1.jpg |
| System Health | Error | Web | ClinicalIntelligence__Web__SystemHealth__Error__v1.jpg |
| System Health | Degraded | Web | ClinicalIntelligence__Web__SystemHealth__Degraded__v1.jpg |
| User Management | Default | Web | ClinicalIntelligence__Web__UserManagement__Default__v1.jpg |
| User Management | Loading | Web | ClinicalIntelligence__Web__UserManagement__Loading__v1.jpg |
| User Management | Empty | Web | ClinicalIntelligence__Web__UserManagement__Empty__v1.jpg |
| User Management | Creating | Web | ClinicalIntelligence__Web__UserManagement__Creating__v1.jpg |
| User Management | Editing | Web | ClinicalIntelligence__Web__UserManagement__Editing__v1.jpg |
| User Management | Success | Web | ClinicalIntelligence__Web__UserManagement__Success__v1.jpg |
| User Management | Error | Web | ClinicalIntelligence__Web__UserManagement__Error__v1.jpg |
| Security Audit Logs | Default | Web | ClinicalIntelligence__Web__SecurityAuditLogs__Default__v1.jpg |
| Security Audit Logs | Loading | Web | ClinicalIntelligence__Web__SecurityAuditLogs__Loading__v1.jpg |
| Security Audit Logs | Empty | Web | ClinicalIntelligence__Web__SecurityAuditLogs__Empty__v1.jpg |
| Security Audit Logs | Error | Web | ClinicalIntelligence__Web__SecurityAuditLogs__Error__v1.jpg |
| Security Audit Logs | Filtering | Web | ClinicalIntelligence__Web__SecurityAuditLogs__Filtering__v1.jpg |
| Analytics Dashboard | Default | Web | ClinicalIntelligence__Web__AnalyticsDashboard__Default__v1.jpg |
| Analytics Dashboard | Loading | Web | ClinicalIntelligence__Web__AnalyticsDashboard__Loading__v1.jpg |
| Analytics Dashboard | Empty | Web | ClinicalIntelligence__Web__AnalyticsDashboard__Empty__v1.jpg |
| Analytics Dashboard | Error | Web | ClinicalIntelligence__Web__AnalyticsDashboard__Error__v1.jpg |

### Total Export Count
- **Screens**: 16
- **States per screen**: 4-7 (average 5)
- **Total JPGs**: 81

---

## 13. Figma File Structure

### Page Organization
```
Clinical Intelligence Platform Figma File
+-- 00_Cover
|   +-- Project info, version, stakeholders
|   +-- Design system reference
+-- 01_Foundations
|   +-- Color tokens (Light mode)
|   +-- Typography scale
|   +-- Spacing scale (8px base)
|   +-- Radius tokens
|   +-- Elevation/shadows
|   +-- Grid definitions (12-column, 1280px+)
+-- 02_Components
|   +-- C/Actions/[Button, IconButton, Link]
|   +-- C/Inputs/[TextField, Select, Checkbox, Radio, Toggle, FileUpload, DateRangePicker]
|   +-- C/Navigation/[Header, Sidebar, Tabs, Breadcrumb, Pagination]
|   +-- C/Content/[Card, ListItem, Table, Badge, Avatar, PDFViewer]
|   +-- C/Feedback/[Modal, Drawer, Toast, Alert, Skeleton, ProgressBar, PasswordStrength]
|   +-- C/DataViz/[Chart, MetricCard]
|   +-- C/Chat/[ChatMessage, ChatInput, SuggestedQuery]
+-- 03_Patterns
|   +-- Auth form pattern (login, password reset)
|   +-- Upload pattern (drag-drop, progress)
|   +-- Table pattern (search, filter, sort, pagination)
|   +-- Conflict resolution pattern
|   +-- Code review pattern
|   +-- Error/Empty/Loading patterns
+-- 04_Screens_Public
|   +-- SCR-001_Login/[Default, Loading, Error, Validation, Lockout]
|   +-- SCR-002_ForgotPassword/[Default, Loading, Success, Error, RateLimited]
|   +-- SCR-003_ResetPassword/[Default, Loading, Success, Error, TokenExpired]
+-- 05_Screens_StandardUser
|   +-- SCR-004_Dashboard/[Default, Loading, Empty, Error]
|   +-- SCR-005_DocumentUpload/[Default, Uploading, Success, Error, Validation]
|   +-- SCR-006_DocumentList/[Default, Loading, Empty, Error, Processing]
|   +-- SCR-007_Profile/[Default, Loading, Editing, Success, Error]
|   +-- SCR-008_Patient360View/[Default, Loading, Empty, Error, Conflict, Verification]
|   +-- SCR-009_AIClinicalAssistant/[Default, Loading, Empty, Error, Responding]
|   +-- SCR-010_Export/[Default, Exporting, Success, Error, Blocked]
|   +-- SCR-011_ProductivityDashboard/[Default, Loading, Empty, Error]
+-- 06_Screens_Admin
|   +-- SCR-012_AdminDashboard/[Default, Loading, Empty, Error]
|   +-- SCR-013_SystemHealth/[Default, Loading, Error, Degraded]
|   +-- SCR-014_UserManagement/[Default, Loading, Empty, Creating, Editing, Success, Error]
|   +-- SCR-015_SecurityAuditLogs/[Default, Loading, Empty, Error, Filtering]
|   +-- SCR-016_AnalyticsDashboard/[Default, Loading, Empty, Error]
+-- 07_Prototype
|   +-- FL-001: User Authentication
|   +-- FL-002: Password Reset
|   +-- FL-003: Document Upload & Processing
|   +-- FL-004: Patient 360 Review & Verification
|   +-- FL-005: AI Clinical Assistant Query
|   +-- FL-006: Data Export
|   +-- FL-007: Admin User Management
|   +-- FL-008: Security Audit Log Review
+-- 08_Handoff
    +-- Token usage rules
    +-- Component guidelines
    +-- Responsive specs (1280px, 1440px, 1920px)
    +-- Edge cases documentation
    +-- Accessibility notes (WCAG 2.1 AA)
```

---

## 14. Quality Checklist

### Pre-Export Validation
- [ ] All screens have required states (Default/Loading/Empty/Error + screen-specific states)
- [ ] All components use design tokens from designsystem.md (no hard-coded values)
- [ ] Color contrast meets WCAG 2.1 AA (≥4.5:1 text, ≥3:1 UI)
- [ ] Focus states defined for all interactive elements
- [ ] Touch targets ≥44x44px (future mobile consideration)
- [ ] Prototype flows wired and functional (FL-001 through FL-008)
- [ ] Naming conventions followed (ClinicalIntelligence__Web__<Screen>__<State>__v1.jpg)
- [ ] Export manifest complete (81 JPGs)

### Post-Generation
- [ ] designsystem.md updated with Figma component references
- [ ] Export manifest generated
- [ ] JPG files named correctly per convention
- [ ] Handoff documentation complete
- [ ] All UXR requirements mapped to screens
- [ ] All use cases covered by flows
