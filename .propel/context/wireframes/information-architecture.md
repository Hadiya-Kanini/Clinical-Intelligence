# Information Architecture - Trust-First Clinical Intelligence Platform

## 1. Wireframe Specification

**Fidelity Level**: High
**Screen Type**: Web (Desktop)
**Viewport**: 1440px x 900px

## 2. System Overview

The Trust-First Clinical Intelligence Platform is a healthcare application that transforms manual clinical document review into an AI-powered, explainable data extraction and aggregation system. The platform enables healthcare staff to upload multiple clinical documents (PDF/DOCX), automatically extract patient data using RAG-powered AI, resolve data conflicts, review ICD-10/CPT billing codes, and export finalized patient records. The system emphasizes 100% explainability with source citations, conflict detection, and human-in-the-loop verification workflows.

**Target Users:**
- **Standard Users**: Clinicians, medical coders, and healthcare administrators who process patient documents
- **Admin Users**: System administrators managing user accounts and security oversight

**Core Value Proposition**: Reduce 20+ minute manual document review to 2-minute AI-assisted verification with full transparency and source traceability.

## 3. Wireframe References

### Generated Wireframes
**HTML Wireframes**:
| Screen/Feature | File Path | Description | Fidelity | Date Created |
|---------------|-----------|-------------|----------|--------------|
| Login | ./wireframe-login.html | Authentication page with email/password | High | 2026-01-13 |
| Forgot Password | ./wireframe-forgot-password.html | Password reset request form | High | 2026-01-13 |
| Reset Password | ./wireframe-reset-password.html | New password entry form | High | 2026-01-13 |
| Standard User Dashboard | ./wireframe-user-dashboard.html | Main dashboard for standard users | High | 2026-01-13 |
| Document Upload | ./wireframe-document-upload.html | Multi-file drag-and-drop upload interface | High | 2026-01-13 |
| Document List | ./wireframe-document-list.html | Processing status and document management | High | 2026-01-13 |
| Patient 360 View | ./wireframe-patient-360.html | Comprehensive patient data aggregation | High | 2026-01-13 |
| Conflict Resolution | ./wireframe-conflict-resolution.html | Side-by-side conflict comparison interface | High | 2026-01-13 |
| AI Clinical Assistant | ./wireframe-ai-assistant.html | Chat interface for document queries | High | 2026-01-13 |
| User Profile | ./wireframe-user-profile.html | Profile settings and preferences | High | 2026-01-13 |
| Admin Dashboard | ./wireframe-admin-dashboard.html | Admin overview with system metrics | High | 2026-01-13 |
| User Management | ./wireframe-user-management.html | Create/update/deactivate user accounts | High | 2026-01-13 |
| Security Audit Logs | ./wireframe-audit-logs.html | Searchable audit trail viewer | High | 2026-01-13 |

### Component Inventory
**Reference**: See [Component Inventory](./component-inventory.md) for detailed component documentation including:
- Complete component specifications
- Component states and variants
- Responsive behavior details
- Reusability analysis
- Implementation priorities

## 4. User Personas & Flows

### Persona 1: Medical Coder (Sarah)
- **Role**: Medical Coding Specialist
- **Goals**: Accurately extract and verify ICD-10/CPT codes from clinical documents to minimize claim denials
- **Key Screens**: Login, Document Upload, Document List, Patient 360 View, Conflict Resolution
- **Primary Flow**: Login -> Upload Documents -> Monitor Processing -> Review Patient 360 -> Resolve Conflicts -> Accept/Reject Billing Codes -> Export Data
- **Wireframe References**: 
  - wireframe-login.html
  - wireframe-document-upload.html
  - wireframe-patient-360.html
  - wireframe-conflict-resolution.html
- **Decision Points**: 
  - Accept or reject suggested ICD-10/CPT codes
  - Select correct value when conflicts detected
  - Choose export format (CSV/JSON/Clipboard)

### Persona 2: Clinician (Dr. Martinez)
- **Role**: Attending Physician
- **Goals**: Quickly review comprehensive patient history before appointments
- **Key Screens**: Login, Patient 360 View, AI Clinical Assistant
- **Primary Flow**: Login -> View Patient 360 -> Query AI Assistant -> Review Source Documents
- **Wireframe References**:
  - wireframe-login.html
  - wireframe-patient-360.html
  - wireframe-ai-assistant.html
- **Decision Points**:
  - Verify extracted clinical data accuracy
  - Ask follow-up questions via AI assistant
  - Navigate to source documents for verification

### Persona 3: System Administrator (James)
- **Role**: IT Administrator
- **Goals**: Manage user accounts, monitor system security, track platform usage
- **Key Screens**: Login, Admin Dashboard, User Management, Security Audit Logs
- **Primary Flow**: Login -> Admin Dashboard -> Create/Manage Users -> Review Audit Logs -> Monitor System Health
- **Wireframe References**:
  - wireframe-login.html
  - wireframe-admin-dashboard.html
  - wireframe-user-management.html
  - wireframe-audit-logs.html
- **Decision Points**:
  - Create new user accounts with appropriate roles
  - Deactivate compromised or inactive accounts
  - Filter and export audit logs for compliance

### User Flow Diagrams
- **Primary Flow (Standard User)**: Document Processing Workflow
  - Entry: Login (wireframe-login.html)
  - Step 1: Upload Documents (wireframe-document-upload.html)
  - Step 2: Monitor Processing Status (wireframe-document-list.html)
  - Step 3: Review Patient 360 (wireframe-patient-360.html)
  - Step 4: Resolve Conflicts (wireframe-conflict-resolution.html)
  - Step 5: Verify Billing Codes (wireframe-patient-360.html)
  - Exit: Export Finalized Data (wireframe-patient-360.html)

- **Secondary Flow (Admin User)**: User Management Workflow
  - Entry: Login (wireframe-login.html)
  - Step 1: Access Admin Dashboard (wireframe-admin-dashboard.html)
  - Step 2: Navigate to User Management (wireframe-user-management.html)
  - Step 3: Create/Update User Account (wireframe-user-management.html)
  - Exit: Confirm User Creation (wireframe-user-management.html)

- **Tertiary Flow**: Password Recovery
  - Entry: Forgot Password Link (wireframe-login.html)
  - Step 1: Enter Email (wireframe-forgot-password.html)
  - Step 2: Receive Email with Reset Link
  - Step 3: Enter New Password (wireframe-reset-password.html)
  - Exit: Redirect to Login (wireframe-login.html)

## 5. Screen Hierarchy

### Level 1: Public Screens (Unauthenticated)
- **Login** (P0 - Critical) - [Wireframe: wireframe-login.html]
  - Description: Secure authentication with email/password, forgot password link, loading states
  - User Entry Point: Yes
  - Key Components: Login Form, Email Input, Password Input, Submit Button, Forgot Password Link, Error Messages, Loading Spinner
  - Related Use Cases: UC-001 (User Authentication)

- **Forgot Password** (P0 - Critical) - [Wireframe: wireframe-forgot-password.html]
  - Description: Email input for password reset request with rate limiting feedback
  - Parent Screen: Login
  - Key Components: Email Input, Submit Button, Back to Login Link, Success Message, Rate Limit Warning
  - Related Use Cases: UC-002 (Password Reset)

- **Reset Password** (P0 - Critical) - [Wireframe: wireframe-reset-password.html]
  - Description: New password entry with strength indicator and confirmation field
  - Parent Screen: Forgot Password (via email link)
  - Key Components: New Password Input, Confirm Password Input, Password Strength Indicator, Submit Button, Success Message
  - Related Use Cases: UC-002 (Password Reset)

### Level 2: Standard User Screens (Authenticated - Standard Role)
- **Standard User Dashboard** (P0 - Critical) - [Wireframe: wireframe-user-dashboard.html]
  - Description: Main landing page with quick actions, recent documents, productivity metrics
  - User Entry Point: Yes (post-login for Standard Users)
  - Key Components: Header with Navigation, Quick Action Cards, Recent Documents List, Productivity Metrics, Sidebar Navigation
  - Related Use Cases: Multiple

- **Document Upload** (P0 - Critical) - [Wireframe: wireframe-document-upload.html]
  - Description: Drag-and-drop multi-file upload with validation and progress tracking
  - Parent Screen: Standard User Dashboard
  - Key Components: Drag-Drop Zone, File List, Upload Progress Bars, Validation Error Messages, Submit Button
  - Related Use Cases: UC-003 (Multi-Document Upload)

- **Document List** (P1 - High Priority) - [Wireframe: wireframe-document-list.html]
  - Description: Table view of uploaded documents with processing status and actions
  - Parent Screen: Standard User Dashboard
  - Key Components: Data Table, Status Badges, Filter Controls, Search Bar, Action Buttons, Pagination
  - Related Use Cases: UC-003, UC-004 (Document Processing)

- **Patient 360 View** (P0 - Critical) - [Wireframe: wireframe-patient-360.html]
  - Description: Comprehensive patient data aggregation with source citations, conflict highlights, billing codes
  - Parent Screen: Document List
  - Key Components: Patient Profile Card, Clinical Data Sections, Source Reference Links, Conflict Badges, ICD-10/CPT Code Tables, Accept/Reject Buttons, Export Button
  - Related Use Cases: UC-005 (View Patient 360), UC-007 (Billing Code Review), UC-009 (Export)

- **Conflict Resolution** (P0 - Critical) - [Wireframe: wireframe-conflict-resolution.html]
  - Description: Side-by-side comparison of conflicting values with source document references
  - Parent Screen: Patient 360 View
  - Key Components: Conflict Comparison Cards, Source Document Viewers, Selection Radio Buttons, Manual Edit Option, Resolve Button
  - Related Use Cases: UC-006 (Resolve Data Conflicts)

- **AI Clinical Assistant** (P1 - High Priority) - [Wireframe: wireframe-ai-assistant.html]
  - Description: Chat interface for natural language queries about patient documents
  - Parent Screen: Patient 360 View or Dashboard
  - Key Components: Chat Message List, Query Input, Send Button, Typing Indicator, Source Citation Links, Suggested Queries
  - Related Use Cases: UC-008 (Query AI Clinical Assistant)

- **User Profile** (P2 - Medium Priority) - [Wireframe: wireframe-user-profile.html]
  - Description: User account settings and preferences
  - Parent Screen: Standard User Dashboard
  - Key Components: Profile Form, Email Display, Password Change Section, Preferences Settings, Save Button
  - Related Use Cases: Profile Management

### Level 3: Admin User Screens (Authenticated - Admin Role)
- **Admin Dashboard** (P0 - Critical) - [Wireframe: wireframe-admin-dashboard.html]
  - Description: System overview with user metrics, security alerts, system health indicators
  - User Entry Point: Yes (post-login for Admin Users)
  - Key Components: Header with Navigation, Metrics Cards, Charts/Graphs, System Health Status, Quick Actions, Sidebar Navigation
  - Related Use Cases: UC-010, UC-011

- **User Management** (P0 - Critical) - [Wireframe: wireframe-user-management.html]
  - Description: Create, update, deactivate user accounts with searchable table
  - Parent Screen: Admin Dashboard
  - Key Components: User Table, Create User Button, Create User Modal, Search/Filter Controls, Action Buttons (Edit/Deactivate), Pagination
  - Related Use Cases: UC-001a (Admin User Creation), UC-010 (Admin User Management)

- **Security Audit Logs** (P1 - High Priority) - [Wireframe: wireframe-audit-logs.html]
  - Description: Searchable, filterable audit trail with detailed event metadata
  - Parent Screen: Admin Dashboard
  - Key Components: Audit Log Table, Filter Panel, Date Range Picker, Search Bar, Export Button, Event Detail Modal, Pagination
  - Related Use Cases: UC-011 (View Security Audit Logs)

### Screen Priority Legend
- **P0**: Critical path screens (must-have for MVP)
- **P1**: High-priority screens (core functionality)
- **P2**: Medium-priority screens (important features)
- **P3**: Low-priority screens (nice-to-have)

### Modal/Dialog/Overlay Inventory
**Purpose:** Systematically document all overlay elements that require wireframes.

| Modal/Dialog Name | Type | Trigger Context | Parent Screen | Wireframe Reference | Priority |
|------------------|------|----------------|---------------|-------------------|----------|
| Create User Modal | Modal | Click "Create User" button | User Management | Embedded in wireframe-user-management.html | P0 |
| Edit User Modal | Modal | Click "Edit" action on user row | User Management | Embedded in wireframe-user-management.html | P0 |
| Deactivate User Confirmation | Dialog | Click "Deactivate" action | User Management | Embedded in wireframe-user-management.html | P0 |
| Logout Confirmation | Dialog | Click "Logout" in header | All authenticated screens | Embedded in header component | P0 |
| Export Options Modal | Modal | Click "Finalize & Export" button | Patient 360 View | Embedded in wireframe-patient-360.html | P0 |
| Audit Log Detail Modal | Modal | Click audit log entry | Security Audit Logs | Embedded in wireframe-audit-logs.html | P1 |
| Source Document Viewer | Overlay | Click source reference link | Patient 360 View | Embedded in wireframe-patient-360.html | P1 |
| File Validation Error Toast | Toast | File upload validation fails | Document Upload | Embedded in wireframe-document-upload.html | P0 |
| Success Notification Toast | Toast | Successful actions (save, export, etc.) | Multiple screens | Global component | P0 |
| Session Timeout Warning | Modal | 2 minutes before session expiry | All authenticated screens | Global component | P0 |

**Modal Behavior Notes:**
- **Responsive Behavior**: Desktop modals remain centered overlays; mobile consideration deferred to Phase 2
- **Trigger Actions**: Explicit user actions (button clicks, link clicks)
- **Dismissal Actions**: Close button (X), overlay click (for non-critical modals), ESC key, successful action completion
- **Focus Management**: Tab trap within modal, return focus to trigger element on close
- **Accessibility**: ARIA role="dialog", aria-labelledby for title, aria-describedby for description, focus on first interactive element

**Common Modal Categories:**
- **Authentication**: Logout Confirmation, Session Timeout Warning
- **Confirmations**: Deactivate User, Resolve Conflict, Finalize Export
- **Forms**: Create User, Edit User, Manual Code Entry
- **Viewers**: Source Document Viewer, Audit Log Detail
- **Notifications**: Success/Error Toasts, Validation Errors

## 6. Navigation Architecture

```
Public Screens
+-- Login (wireframe-login.html)
    +-- Forgot Password (wireframe-forgot-password.html)
        +-- Reset Password (wireframe-reset-password.html) [via email link]

Standard User Screens (Post-Login)
+-- Standard User Dashboard (wireframe-user-dashboard.html)
    +-- Document Upload (wireframe-document-upload.html)
    +-- Document List (wireframe-document-list.html)
    |   +-- Patient 360 View (wireframe-patient-360.html)
    |       +-- Conflict Resolution (wireframe-conflict-resolution.html)
    |       +-- AI Clinical Assistant (wireframe-ai-assistant.html)
    |       +-- Export Options Modal
    +-- AI Clinical Assistant (wireframe-ai-assistant.html) [standalone access]
    +-- User Profile (wireframe-user-profile.html)

Admin User Screens (Post-Login)
+-- Admin Dashboard (wireframe-admin-dashboard.html)
    +-- User Management (wireframe-user-management.html)
    |   +-- Create User Modal
    |   +-- Edit User Modal
    +-- Security Audit Logs (wireframe-audit-logs.html)
        +-- Audit Log Detail Modal
```

### Navigation Patterns
- **Primary Navigation**: 
  - **Standard Users**: Persistent left sidebar with icons and labels (Dashboard, Documents, Upload, AI Assistant, Profile, Logout)
  - **Admin Users**: Persistent left sidebar with icons and labels (Dashboard, User Management, Audit Logs, System Health, Logout)
  - **Wireframe Reference**: Embedded in all authenticated screen wireframes

- **Secondary Navigation**: 
  - **Breadcrumbs**: Displayed below header for deep navigation paths (e.g., Dashboard > Documents > Patient 360)
  - **Tab Navigation**: Within Patient 360 View for different data categories (Profile, Clinical, Codes)
  - **Wireframe Reference**: Patient 360 View (wireframe-patient-360.html)

- **Mobile Navigation**: 
  - Phase 1 targets desktop only (1280px+); mobile navigation deferred to Phase 2

## 7. Interaction Patterns

### Pattern 1: Document Upload and Processing
- **Trigger**: User drags files to upload zone or clicks "Select Files"
- **Flow**: 
  1. User selects/drags up to 10 PDF/DOCX files
  2. System validates each file (extension, MIME type, size, integrity)
  3. Valid files display with progress bars; invalid files show error messages
  4. User clicks "Upload" button
  5. System stores files and queues for processing
  6. User redirected to Document List with "Pending" status
  7. Status updates to "Processing" then "Completed" or "Failed" asynchronously
- **Screens Involved**: 
  - Document Upload (wireframe-document-upload.html)
  - Document List (wireframe-document-list.html)
- **Feedback**: Progress bars, validation error messages, status badges, success toast
- **Components Used**: Drag-Drop Zone, File List, Progress Bar, Status Badge, Error Message, Toast Notification

### Pattern 2: Conflict Resolution
- **Trigger**: User clicks "Resolve Conflicts" badge on Patient 360 View
- **Flow**:
  1. System displays Conflict Resolution screen with side-by-side comparison
  2. User reviews conflicting values and source document references
  3. User clicks source reference links to view full context
  4. User selects correct value via radio button or enters manual correction
  5. User clicks "Resolve" button for each conflict
  6. System updates patient record and removes conflict badge
  7. User returns to Patient 360 View
- **Screens Involved**:
  - Patient 360 View (wireframe-patient-360.html)
  - Conflict Resolution (wireframe-conflict-resolution.html)
- **Feedback**: Conflict count badge, selection highlight, success toast on resolution
- **Components Used**: Conflict Badge, Comparison Card, Radio Button, Source Reference Link, Resolve Button, Toast Notification

### Pattern 3: ICD-10/CPT Code Review
- **Trigger**: User views "Billing Codes" section on Patient 360 View
- **Flow**:
  1. System displays suggested ICD-10 and CPT codes with descriptions
  2. User clicks source reference link to verify code accuracy
  3. User clicks "Accept" button for valid codes (turns green)
  4. User clicks "Reject" button for invalid codes (turns red, removes from list)
  5. User clicks "Add Code" for missing codes and enters manually
  6. System tracks code status (accepted, rejected, manually_added)
- **Screens Involved**:
  - Patient 360 View (wireframe-patient-360.html)
- **Feedback**: Accept/Reject button state changes, code status badges, manual entry modal
- **Components Used**: Code Table, Accept/Reject Buttons, Source Reference Link, Add Code Button, Manual Entry Modal

### Pattern 4: AI Clinical Assistant Query
- **Trigger**: User types natural language query and clicks "Send" or presses Enter
- **Flow**:
  1. User enters query (e.g., "Summarize medication history")
  2. System displays typing indicator
  3. System performs vector similarity search and generates response
  4. System displays response with clickable source citations
  5. User clicks source citation to view relevant document section
  6. User asks follow-up questions in same chat thread
- **Screens Involved**:
  - AI Clinical Assistant (wireframe-ai-assistant.html)
- **Feedback**: Typing indicator, source citation links, suggested queries
- **Components Used**: Chat Message List, Query Input, Send Button, Typing Indicator, Source Citation Link, Suggested Query Chips

### Pattern 5: Admin User Creation
- **Trigger**: Admin clicks "Create User" button on User Management screen
- **Flow**:
  1. System displays Create User Modal
  2. Admin enters user details (name, email, role)
  3. System validates input format and checks for duplicate email
  4. Admin clicks "Create" button
  5. System generates temporary password and creates account
  6. System sends email with credentials to new user
  7. System displays success toast and closes modal
  8. User table refreshes with new user entry
- **Screens Involved**:
  - User Management (wireframe-user-management.html)
- **Feedback**: Validation error messages, success toast, email sent confirmation
- **Components Used**: Create User Button, Modal, Form Inputs, Validation Messages, Submit Button, Toast Notification

## 8. Error Handling

### Error Scenario 1: File Upload Validation Failure
- **Trigger**: User uploads invalid file (wrong format, too large, corrupted, password-protected)
- **Error Screen/State**: Document Upload (wireframe-document-upload.html) - File list item with error badge and message
- **User Action**: Remove invalid file and upload valid replacement
- **Recovery Flow**: 
  1. System displays specific error message (e.g., "File exceeds 50MB limit")
  2. User clicks "Remove" button on invalid file
  3. User uploads valid file
  4. System validates and proceeds with upload

### Error Scenario 2: Document Processing Failure
- **Trigger**: AI Worker fails to process document after 3 retry attempts
- **Error Screen/State**: Document List (wireframe-document-list.html) - Status badge shows "Failed" with error icon
- **User Action**: View error details, retry processing, or contact support
- **Recovery Flow**:
  1. System displays "Failed" status badge
  2. User clicks status badge to view error details modal
  3. User clicks "Retry Processing" button
  4. System re-queues document for processing

### Error Scenario 3: Session Timeout
- **Trigger**: User inactive for 15 minutes
- **Error Screen/State**: Session Timeout Warning Modal (2 minutes before expiry) on any authenticated screen
- **User Action**: Click "Stay Logged In" to extend session or allow timeout
- **Recovery Flow**:
  1. System displays warning modal at 13-minute mark
  2. User clicks "Stay Logged In" button
  3. System refreshes JWT token and extends session
  4. Modal closes and user continues work
  5. If no action: System logs out user and redirects to Login with "Session expired" message

### Error Scenario 4: Conflict Resolution Incomplete
- **Trigger**: User attempts to export patient data with unresolved conflicts
- **Error Screen/State**: Patient 360 View (wireframe-patient-360.html) - Export button disabled, error message displayed
- **User Action**: Navigate to Conflict Resolution and resolve all conflicts
- **Recovery Flow**:
  1. System displays error toast: "Cannot export: X conflicts require resolution"
  2. User clicks "Resolve Conflicts" badge
  3. User resolves all conflicts
  4. System enables Export button
  5. User proceeds with export

### Error Scenario 5: Network Error During API Call
- **Trigger**: Network connectivity issue or API timeout
- **Error Screen/State**: Global error toast on any screen
- **User Action**: Retry action or refresh page
- **Recovery Flow**:
  1. System displays error toast: "Network error. Please try again."
  2. User clicks "Retry" button on toast
  3. System re-attempts API call
  4. If successful: Toast disappears and action completes
  5. If persistent: User refreshes page or contacts support

### Error Scenario 6: Unauthorized Access Attempt
- **Trigger**: Standard User attempts to access Admin screens via URL manipulation
- **Error Screen/State**: 403 Forbidden error page with message
- **User Action**: Return to authorized dashboard
- **Recovery Flow**:
  1. System detects unauthorized role access
  2. System displays 403 error page: "You do not have permission to access this page"
  3. User clicks "Return to Dashboard" button
  4. System redirects to appropriate role-based dashboard
  5. System logs unauthorized access attempt in audit trail

## 9. Responsive Strategy

| Breakpoint | Width | Layout Changes | Navigation Changes | Component Adaptations |
|-----------|-------|----------------|-------------------|---------------------|
| Desktop | 1440px | Multi-column grid, sidebar + main content | Expanded sidebar with icons and labels | Full data tables, side-by-side conflict views, expanded forms |

**Note**: Phase 1 targets desktop viewports only (1280px minimum). Tablet and mobile responsive design deferred to Phase 2.

### Responsive Wireframe Variants
- **Desktop variants**: All wireframes generated at 1440px x 900px
- **Tablet variants**: Deferred to Phase 2
- **Mobile variants**: Deferred to Phase 2

## 10. Accessibility

### WCAG Compliance
- **Target Level**: WCAG 2.1 AA
- **Color Contrast**: 
  - Text contrast minimum 4.5:1 (normal text)
  - Large text contrast minimum 3:0:1 (18pt+ or 14pt+ bold)
  - UI component contrast minimum 3:1
  - Focus indicator contrast minimum 3:1
- **Keyboard Navigation**: All interactive elements accessible via Tab, Enter, Space, Arrow keys
- **Screen Reader Support**: ARIA labels, roles, and live regions for dynamic content

### Accessibility Considerations by Screen
| Screen | Key Accessibility Features | Wireframe Notes |
|--------|---------------------------|----------------|
| Login | Focus order: Email → Password → Submit → Forgot Password; ARIA error announcements | wireframe-login.html |
| Document Upload | Drag-drop alternative (click to select); file list with ARIA live region for status updates | wireframe-document-upload.html |
| Patient 360 View | Landmark regions (navigation, main, complementary); heading hierarchy (H1-H3); source links with descriptive text | wireframe-patient-360.html |
| Conflict Resolution | Radio button groups with fieldset/legend; clear labels for each conflicting value | wireframe-conflict-resolution.html |
| AI Clinical Assistant | Chat messages with ARIA live region; query input with label; source links with descriptive text | wireframe-ai-assistant.html |
| User Management | Data table with sortable headers (ARIA sort); modal focus trap; form validation with ARIA invalid | wireframe-user-management.html |
| Audit Logs | Filterable table with ARIA live region for results; date picker with keyboard navigation | wireframe-audit-logs.html |

### Focus Order
- **Login Flow**: Email Input → Password Input → Submit Button → Forgot Password Link
- **Document Upload**: Upload Zone → File List Items → Remove Buttons → Upload Button
- **Patient 360 View**: Sidebar Navigation → Main Content Tabs → Data Sections → Action Buttons → Export Button
- **Conflict Resolution**: Conflict Card 1 Radio Buttons → Conflict Card 2 Radio Buttons → Resolve Button
- **Modal Dialogs**: Close Button → Form Fields → Action Buttons (Cancel/Submit)

## 11. Content Strategy

### Content Hierarchy
- **H1**: Page title (e.g., "Patient 360 View", "User Management") - one per page
- **H2**: Major section headings (e.g., "Patient Profile", "Clinical Data", "Billing Codes")
- **H3**: Subsection headings (e.g., "Demographics", "Allergies", "Medications")
- **Body Text**: 16px base font size, 1.5 line-height for readability
- **Placeholder Content**: Realistic clinical data examples (e.g., "John Doe", "Hypertension", "Lisinopril 10mg")

### Content Types by Screen
| Screen | Content Types | Wireframe Reference |
|--------|--------------|-------------------|
| Login | Form labels, error messages, link text | wireframe-login.html |
| Document Upload | Instructions, file names, validation errors, progress percentages | wireframe-document-upload.html |
| Document List | Table headers, status labels, document metadata, action labels | wireframe-document-list.html |
| Patient 360 View | Patient demographics, clinical data, source citations, code descriptions, action labels | wireframe-patient-360.html |
| Conflict Resolution | Conflict descriptions, source references, selection labels, instructions | wireframe-conflict-resolution.html |
| AI Clinical Assistant | Chat messages, query prompts, source citations, suggested queries | wireframe-ai-assistant.html |
| Admin Dashboard | Metric labels, chart titles, system status messages | wireframe-admin-dashboard.html |
| User Management | Table headers, form labels, validation messages, confirmation dialogs | wireframe-user-management.html |
| Audit Logs | Table headers, event descriptions, filter labels, metadata | wireframe-audit-logs.html |

### Content Tone
- **Professional and Clinical**: Healthcare-appropriate language, medical terminology where relevant
- **Clear and Actionable**: Explicit instructions, unambiguous labels, helpful error messages
- **Trust-Building**: Emphasize transparency, source citations, human verification
- **Concise**: Minimize cognitive load, use progressive disclosure for complex information
