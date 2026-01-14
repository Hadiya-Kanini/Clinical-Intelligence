# Component Inventory - Trust-First Clinical Intelligence Platform

## Component Specification

**Fidelity Level**: High
**Screen Type**: Web (Desktop)
**Viewport**: 1440px x 900px

## Component Summary

| Component Name | Type | Screens Used | Priority | Implementation Status |
|---------------|------|-------------|----------|---------------------|
| Header with Navigation | Layout | All authenticated screens | High | Pending |
| Sidebar Navigation | Navigation | All authenticated screens | High | Pending |
| Login Form | Interactive | Login | High | Pending |
| Password Reset Form | Interactive | Forgot Password, Reset Password | High | Pending |
| Drag-Drop Upload Zone | Interactive | Document Upload | High | Pending |
| File List with Progress | Content | Document Upload | High | Pending |
| Data Table | Content | Document List, User Management, Audit Logs | High | Pending |
| Patient Profile Card | Content | Patient 360 View | High | Pending |
| Clinical Data Section | Content | Patient 360 View | High | Pending |
| Source Reference Link | Interactive | Patient 360 View, Conflict Resolution | High | Pending |
| Conflict Comparison Card | Content | Conflict Resolution | High | Pending |
| ICD-10/CPT Code Table | Content | Patient 360 View | High | Pending |
| Accept/Reject Button Group | Interactive | Patient 360 View | High | Pending |
| Chat Interface | Interactive | AI Clinical Assistant | High | Pending |
| Modal Dialog | Feedback | User Management, Patient 360 View, Multiple | High | Pending |
| Toast Notification | Feedback | All screens | High | Pending |
| Status Badge | Feedback | Document List, Patient 360 View | High | Pending |
| Metrics Card | Content | Admin Dashboard, User Dashboard | Medium | Pending |
| Chart/Graph Component | Content | Admin Dashboard | Medium | Pending |
| Filter Panel | Interactive | Audit Logs, Document List | Medium | Pending |
| Breadcrumb Navigation | Navigation | Patient 360 View, Conflict Resolution | Medium | Pending |
| Tab Navigation | Navigation | Patient 360 View | Medium | Pending |
| Loading Spinner | Feedback | All screens | High | Pending |
| Error Message | Feedback | All forms | High | Pending |
| Pagination Controls | Navigation | Data Tables | Medium | Pending |

## Detailed Component Specifications

### Layout Components

#### Header with Navigation
- **Type**: Layout
- **Used In Screens**: All authenticated screens (Dashboard, Document Upload, Document List, Patient 360, Admin Dashboard, User Management, Audit Logs, Profile)
- **Wireframe References**: Embedded in all authenticated screen wireframes
- **Description**: Persistent top bar containing platform logo, page title, user profile dropdown, and logout button
- **Variants**: 
  - Standard User Header (with user role badge)
  - Admin User Header (with admin role badge)
- **Interactive States**: 
  - Default: Logo, page title, user avatar
  - Hover: User avatar shows dropdown indicator
  - Active: User dropdown menu expanded (Profile, Logout options)
- **Responsive Behavior**:
  - Desktop (1440px): Full width, logo left, title center-left, user controls right
- **Implementation Notes**: Sticky positioning (top: 0), z-index above content, 64px height, shadow on scroll

#### Sidebar Navigation
- **Type**: Navigation
- **Used In Screens**: All authenticated screens
- **Wireframe References**: Embedded in all authenticated screen wireframes
- **Description**: Persistent left sidebar with icon-based navigation menu, role-specific menu items
- **Variants**:
  - Standard User Sidebar (Dashboard, Documents, Upload, AI Assistant, Profile)
  - Admin User Sidebar (Dashboard, User Management, Audit Logs, System Health)
- **Interactive States**:
  - Default: Icons with labels, inactive state
  - Hover: Background highlight, icon color change
  - Active: Current page highlighted with accent color, left border indicator
  - Focus: Keyboard focus outline
- **Responsive Behavior**:
  - Desktop (1440px): 240px width, expanded with icons and labels
- **Implementation Notes**: Fixed positioning, 100vh height, collapsible consideration for future, ARIA landmark role="navigation"

### Navigation Components

#### Breadcrumb Navigation
- **Type**: Navigation
- **Used In Screens**: Patient 360 View, Conflict Resolution
- **Wireframe References**: wireframe-patient-360.html, wireframe-conflict-resolution.html
- **Description**: Hierarchical navigation path showing current location within application
- **Variants**: 
  - 2-level (Dashboard > Patient 360)
  - 3-level (Dashboard > Documents > Patient 360)
- **Interactive States**:
  - Default: Text links separated by chevron icons
  - Hover: Link underline, color change
  - Active: Current page not clickable, different text color
- **Responsive Behavior**:
  - Desktop (1440px): Full breadcrumb path displayed
- **Implementation Notes**: ARIA aria-label="Breadcrumb", current page aria-current="page"

#### Tab Navigation
- **Type**: Navigation
- **Used In Screens**: Patient 360 View
- **Wireframe References**: wireframe-patient-360.html
- **Description**: Horizontal tab bar for switching between patient data categories (Profile, Clinical, Codes)
- **Variants**: 3-tab layout (Profile, Clinical Data, Billing Codes)
- **Interactive States**:
  - Default: Inactive tabs with subtle background
  - Hover: Background color change
  - Active: Selected tab with accent color bottom border, bold text
  - Focus: Keyboard focus outline
- **Responsive Behavior**:
  - Desktop (1440px): Horizontal tabs, equal width distribution
- **Implementation Notes**: ARIA role="tablist", role="tab", aria-selected, keyboard navigation (Arrow keys)

#### Pagination Controls
- **Type**: Navigation
- **Used In Screens**: Document List, User Management, Audit Logs
- **Wireframe References**: wireframe-document-list.html, wireframe-user-management.html, wireframe-audit-logs.html
- **Description**: Page navigation controls for data tables (Previous, page numbers, Next)
- **Variants**: 
  - Standard pagination (< 1 2 3 ... 10 >)
  - Compact pagination (< Page 2 of 10 >)
- **Interactive States**:
  - Default: Page numbers, Previous/Next buttons
  - Hover: Button background highlight
  - Active: Current page highlighted with accent color
  - Disabled: Previous disabled on page 1, Next disabled on last page
- **Responsive Behavior**:
  - Desktop (1440px): Full page numbers displayed
- **Implementation Notes**: ARIA aria-label="Pagination", aria-current="page", keyboard navigation

### Content Components

#### Patient Profile Card
- **Type**: Content
- **Used In Screens**: Patient 360 View
- **Wireframe References**: wireframe-patient-360.html
- **Description**: Summary card displaying patient demographics (Name, DOB, MRN, Contact)
- **Variants**: Single variant with optional conflict badges
- **Interactive States**:
  - Default: Card with patient information
  - Hover: Source reference links underline
  - Active: Editable fields on click
- **Responsive Behavior**:
  - Desktop (1440px): Full-width card at top of Patient 360 View
- **Implementation Notes**: Card component with grid layout, source reference icons, conflict badges overlay

#### Clinical Data Section
- **Type**: Content
- **Used In Screens**: Patient 360 View
- **Wireframe References**: wireframe-patient-360.html
- **Description**: Collapsible sections for clinical entities (Allergies, Medications, Diagnoses, Procedures, Lab Results, Vitals, Social History, Clinical Notes)
- **Variants**: 8 clinical entity categories, each with expand/collapse
- **Interactive States**:
  - Default: Section header with expand/collapse icon
  - Hover: Header background highlight
  - Expanded: Section content visible with data items
  - Collapsed: Section content hidden
- **Responsive Behavior**:
  - Desktop (1440px): Full-width sections, stacked vertically
- **Implementation Notes**: Accordion pattern, ARIA aria-expanded, smooth expand/collapse animation

#### Data Table
- **Type**: Content
- **Used In Screens**: Document List, User Management, Audit Logs
- **Wireframe References**: wireframe-document-list.html, wireframe-user-management.html, wireframe-audit-logs.html
- **Description**: Sortable, filterable table for displaying structured data with actions
- **Variants**:
  - Document List Table (columns: Name, Upload Date, Status, Actions)
  - User Management Table (columns: Name, Email, Role, Status, Actions)
  - Audit Logs Table (columns: Timestamp, User, Action, IP Address, Details)
- **Interactive States**:
  - Default: Table with headers and data rows
  - Hover: Row background highlight
  - Active: Sorted column header with sort indicator (↑↓)
  - Selected: Row selection checkbox checked
- **Responsive Behavior**:
  - Desktop (1440px): Full table width, all columns visible
- **Implementation Notes**: Sticky header on scroll, sortable columns with ARIA aria-sort, row actions dropdown, zebra striping for readability

#### ICD-10/CPT Code Table
- **Type**: Content
- **Used In Screens**: Patient 360 View
- **Wireframe References**: wireframe-patient-360.html
- **Description**: Table displaying suggested billing codes with descriptions, source references, and Accept/Reject actions
- **Variants**:
  - ICD-10 Code Table (Diagnosis codes)
  - CPT Code Table (Procedure codes)
- **Interactive States**:
  - Default: Code rows with Accept/Reject buttons
  - Accepted: Row highlighted green, Accept button active
  - Rejected: Row highlighted red, Reject button active, row fades out
  - Pending: Neutral state, both buttons available
- **Responsive Behavior**:
  - Desktop (1440px): Full table width, columns: Code, Description, Source, Status, Actions
- **Implementation Notes**: Status badges, source reference links, manual code entry button, ARIA live region for status updates

#### Metrics Card
- **Type**: Content
- **Used In Screens**: Admin Dashboard, User Dashboard
- **Wireframe References**: wireframe-admin-dashboard.html, wireframe-user-dashboard.html
- **Description**: Card displaying key performance indicator with value, label, and trend indicator
- **Variants**:
  - Large metric card (primary KPIs)
  - Small metric card (secondary metrics)
- **Interactive States**:
  - Default: Card with metric value and label
  - Hover: Card elevation increase (shadow)
  - Loading: Skeleton loader for metric value
- **Responsive Behavior**:
  - Desktop (1440px): Grid layout, 3-4 cards per row
- **Implementation Notes**: Icon, large numeric value, descriptive label, trend indicator (↑↓), optional sparkline chart

#### Chart/Graph Component
- **Type**: Content
- **Used In Screens**: Admin Dashboard
- **Wireframe References**: wireframe-admin-dashboard.html
- **Description**: Data visualization components (line charts, bar charts, pie charts) for analytics
- **Variants**:
  - Line Chart (time-series data: processing volumes over time)
  - Bar Chart (categorical data: documents by status)
  - Pie Chart (proportional data: user roles distribution)
- **Interactive States**:
  - Default: Chart with data visualization
  - Hover: Tooltip showing data point details
  - Loading: Skeleton loader for chart area
- **Responsive Behavior**:
  - Desktop (1440px): Full card width, responsive height
- **Implementation Notes**: Chart.js or D3.js integration, accessible data table alternative, ARIA labels for screen readers

#### Conflict Comparison Card
- **Type**: Content
- **Used In Screens**: Conflict Resolution
- **Wireframe References**: wireframe-conflict-resolution.html
- **Description**: Side-by-side comparison card showing conflicting values with source document references
- **Variants**: Single variant with 2-column layout
- **Interactive States**:
  - Default: Two columns with conflicting values
  - Hover: Source reference links underline
  - Selected: Radio button selected, column highlighted
- **Responsive Behavior**:
  - Desktop (1440px): 50/50 split, side-by-side layout
- **Implementation Notes**: Radio button group, source document viewer links, manual edit option, clear visual distinction between options

### Interactive Components

#### Login Form
- **Type**: Interactive
- **Used In Screens**: Login
- **Wireframe References**: wireframe-login.html
- **Description**: Authentication form with email and password inputs, submit button, forgot password link
- **Variants**: Single variant
- **Interactive States**:
  - Default: Empty form fields
  - Focus: Input field border highlight
  - Error: Red border, error message below field
  - Loading: Submit button disabled with spinner
  - Success: Redirect to dashboard
- **Responsive Behavior**:
  - Desktop (1440px): Centered form, 400px max-width
- **Implementation Notes**: Email validation (RFC 5322), password masking, show/hide password toggle, ARIA error announcements, rate limiting feedback

#### Password Reset Form
- **Type**: Interactive
- **Used In Screens**: Forgot Password, Reset Password
- **Wireframe References**: wireframe-forgot-password.html, wireframe-reset-password.html
- **Description**: Forms for password reset request and new password entry
- **Variants**:
  - Forgot Password Form (email input only)
  - Reset Password Form (new password + confirm password)
- **Interactive States**:
  - Default: Empty form fields
  - Focus: Input field border highlight
  - Error: Validation error messages
  - Success: Confirmation message
  - Loading: Submit button disabled with spinner
- **Responsive Behavior**:
  - Desktop (1440px): Centered form, 400px max-width
- **Implementation Notes**: Password strength indicator, complexity requirements display, token validation, ARIA announcements

#### Drag-Drop Upload Zone
- **Type**: Interactive
- **Used In Screens**: Document Upload
- **Wireframe References**: wireframe-document-upload.html
- **Description**: Drag-and-drop area for multi-file upload with click-to-select alternative
- **Variants**: Single variant with file type restrictions
- **Interactive States**:
  - Default: Dashed border, upload icon, instructional text
  - Hover: Border color change
  - Drag Over: Border solid, background highlight
  - Active: Files being processed
  - Error: Red border, error message
- **Responsive Behavior**:
  - Desktop (1440px): Full-width zone, 300px height
- **Implementation Notes**: Drag-and-drop events, file input fallback, MIME type validation, file size validation, batch limit (10 files), accessibility with keyboard file selection

#### File List with Progress
- **Type**: Content
- **Used In Screens**: Document Upload
- **Wireframe References**: wireframe-document-upload.html
- **Description**: List of uploaded files with individual progress bars and validation status
- **Variants**: Single variant
- **Interactive States**:
  - Uploading: Progress bar animating (0-100%)
  - Success: Green checkmark icon, 100% progress
  - Error: Red X icon, error message
  - Removable: X button to remove file from list
- **Responsive Behavior**:
  - Desktop (1440px): Full-width list, stacked file items
- **Implementation Notes**: ARIA live region for progress updates, file size display, file type icon, remove button, validation error messages

#### Source Reference Link
- **Type**: Interactive
- **Used In Screens**: Patient 360 View, Conflict Resolution, AI Clinical Assistant
- **Wireframe References**: wireframe-patient-360.html, wireframe-conflict-resolution.html, wireframe-ai-assistant.html
- **Description**: Clickable link that opens source document viewer overlay showing exact document location
- **Variants**: 
  - Inline link (within text)
  - Icon button (source icon)
- **Interactive States**:
  - Default: Link with source icon
  - Hover: Underline, color change
  - Active: Opens source document viewer overlay
  - Focus: Keyboard focus outline
- **Responsive Behavior**:
  - Desktop (1440px): Inline or icon button
- **Implementation Notes**: Tooltip showing document name and page number on hover, opens modal/overlay with PDF viewer, highlights relevant text section

#### Accept/Reject Button Group
- **Type**: Interactive
- **Used In Screens**: Patient 360 View (Billing Codes section)
- **Wireframe References**: wireframe-patient-360.html
- **Description**: Paired buttons for accepting or rejecting AI-suggested billing codes
- **Variants**: Single variant with two buttons
- **Interactive States**:
  - Default: Both buttons enabled, neutral state
  - Accept Hover: Green background preview
  - Reject Hover: Red background preview
  - Accepted: Accept button green, Reject button disabled
  - Rejected: Reject button red, Accept button disabled
  - Loading: Both buttons disabled with spinner
- **Responsive Behavior**:
  - Desktop (1440px): Horizontal button group, equal width
- **Implementation Notes**: Mutually exclusive actions, ARIA live region for status updates, undo option consideration

#### Chat Interface
- **Type**: Interactive
- **Used In Screens**: AI Clinical Assistant
- **Wireframe References**: wireframe-ai-assistant.html
- **Description**: Chat-style interface with message list, query input, send button, suggested queries
- **Variants**: Single variant
- **Interactive States**:
  - Default: Empty chat or conversation history
  - Typing: User typing in input field
  - Sending: Send button disabled, typing indicator shown
  - Receiving: AI response streaming in
  - Error: Error message in chat
- **Responsive Behavior**:
  - Desktop (1440px): Full-height chat area, fixed input at bottom
- **Implementation Notes**: Auto-scroll to latest message, ARIA live region for new messages, suggested query chips, source citation links in responses, message timestamps

#### Filter Panel
- **Type**: Interactive
- **Used In Screens**: Audit Logs, Document List
- **Wireframe References**: wireframe-audit-logs.html, wireframe-document-list.html
- **Description**: Collapsible panel with filter controls (date range, dropdowns, search)
- **Variants**:
  - Audit Logs Filter (User, Action Type, Date Range, IP Address)
  - Document List Filter (Status, Upload Date, Document Type)
- **Interactive States**:
  - Default: Filter controls with current selections
  - Expanded: All filter options visible
  - Collapsed: Filter summary visible
  - Applied: Active filter badges shown
  - Cleared: Reset to default state
- **Responsive Behavior**:
  - Desktop (1440px): Side panel or top panel, 300px width
- **Implementation Notes**: Date range picker, multi-select dropdowns, search input, apply/clear buttons, active filter count badge

### Feedback Components

#### Modal Dialog
- **Type**: Feedback
- **Used In Screens**: User Management (Create/Edit User), Patient 360 View (Export Options), Audit Logs (Event Details), All screens (Logout Confirmation, Session Timeout)
- **Wireframe References**: Embedded in wireframe-user-management.html, wireframe-patient-360.html, wireframe-audit-logs.html
- **Description**: Overlay dialog for forms, confirmations, and detailed views
- **Variants**:
  - Form Modal (Create User, Edit User)
  - Confirmation Dialog (Logout, Deactivate User)
  - Detail Viewer (Audit Log Details, Source Document)
  - Selection Modal (Export Options)
- **Interactive States**:
  - Default: Modal centered on screen with overlay
  - Focus: Focus trapped within modal
  - Closing: Fade-out animation
- **Responsive Behavior**:
  - Desktop (1440px): Centered modal, max-width 600px, overlay covers viewport
- **Implementation Notes**: Focus trap, ESC key to close, overlay click to close (non-critical modals), ARIA role="dialog", aria-labelledby, aria-describedby, return focus to trigger element on close

#### Toast Notification
- **Type**: Feedback
- **Used In Screens**: All screens
- **Wireframe References**: Global component, shown in context on various wireframes
- **Description**: Temporary notification message for success, error, warning, info feedback
- **Variants**:
  - Success Toast (green, checkmark icon)
  - Error Toast (red, X icon)
  - Warning Toast (yellow, warning icon)
  - Info Toast (blue, info icon)
- **Interactive States**:
  - Appearing: Slide-in animation from top-right
  - Visible: Display for 5 seconds (error: 10 seconds)
  - Dismissing: Fade-out animation
  - Dismissed: Removed from DOM
- **Responsive Behavior**:
  - Desktop (1440px): Fixed position top-right, 400px width, stacked if multiple
- **Implementation Notes**: Auto-dismiss timer, manual dismiss button, ARIA role="alert" for errors, aria-live="polite" for info, icon + message + close button

#### Status Badge
- **Type**: Feedback
- **Used In Screens**: Document List, Patient 360 View, User Management
- **Wireframe References**: wireframe-document-list.html, wireframe-patient-360.html, wireframe-user-management.html
- **Description**: Small colored label indicating status or state
- **Variants**:
  - Document Status (Pending, Processing, Completed, Failed, Validation_Failed)
  - Conflict Status (Unresolved, Resolved)
  - Code Status (Pending, Accepted, Rejected, Manually Added)
  - User Status (Active, Inactive, Locked)
- **Interactive States**:
  - Default: Static badge with color and text
  - Hover: Tooltip with additional details (optional)
- **Responsive Behavior**:
  - Desktop (1440px): Inline badge, compact size
- **Implementation Notes**: Color-coded (green: success, yellow: warning, red: error, blue: info, gray: neutral), icon + text, ARIA label for screen readers

#### Loading Spinner
- **Type**: Feedback
- **Used In Screens**: All screens
- **Wireframe References**: Embedded in various components (buttons, cards, tables)
- **Description**: Animated spinner indicating loading or processing state
- **Variants**:
  - Inline Spinner (small, within buttons)
  - Page Spinner (large, centered on page)
  - Section Spinner (medium, within cards or sections)
- **Interactive States**:
  - Animating: Continuous rotation
- **Responsive Behavior**:
  - Desktop (1440px): Size varies by context (16px inline, 48px page)
- **Implementation Notes**: CSS animation, ARIA aria-busy="true", ARIA live region announcement "Loading..."

#### Error Message
- **Type**: Feedback
- **Used In Screens**: All forms (Login, Password Reset, Document Upload, User Management)
- **Wireframe References**: Embedded in form wireframes
- **Description**: Inline error message displayed below form fields or at form top
- **Variants**:
  - Field-level Error (below specific input)
  - Form-level Error (top of form, multiple errors)
- **Interactive States**:
  - Visible: Red text, error icon
  - Dismissed: Removed when field corrected
- **Responsive Behavior**:
  - Desktop (1440px): Below field or top of form
- **Implementation Notes**: ARIA aria-invalid="true" on field, aria-describedby linking to error message, icon + descriptive text, actionable guidance

## Component Relationships

```
Application Shell
+-- Header with Navigation
|   +-- Logo
|   +-- Page Title
|   +-- User Profile Dropdown
|       +-- Profile Link
|       +-- Logout Link
+-- Sidebar Navigation
|   +-- Navigation Menu Items
|       +-- Icon
|       +-- Label
+-- Main Content Area
|   +-- Breadcrumb Navigation (conditional)
|   +-- Page Content
|       +-- [Screen-specific components]
+-- Toast Notification Container (global)
+-- Modal Dialog Container (global)
+-- Loading Spinner (global)

Patient 360 View Hierarchy
+-- Breadcrumb Navigation
+-- Tab Navigation
|   +-- Profile Tab
|   +-- Clinical Data Tab
|   +-- Billing Codes Tab
+-- Patient Profile Card
|   +-- Demographics Section
|   +-- Source Reference Links
|   +-- Conflict Badges
+-- Clinical Data Sections (Accordion)
|   +-- Section Header
|   +-- Section Content
|       +-- Data Items
|       +-- Source Reference Links
+-- ICD-10/CPT Code Tables
|   +-- Code Rows
|       +-- Code + Description
|       +-- Source Reference Link
|       +-- Accept/Reject Button Group
+-- Export Button

Document Upload Hierarchy
+-- Drag-Drop Upload Zone
|   +-- Upload Icon
|   +-- Instructional Text
|   +-- File Input (hidden)
+-- File List with Progress
|   +-- File Items
|       +-- File Icon
|       +-- File Name + Size
|       +-- Progress Bar
|       +-- Status Badge
|       +-- Remove Button
+-- Upload Button

Data Table Hierarchy
+-- Filter Panel (optional)
|   +-- Filter Controls
|   +-- Apply/Clear Buttons
+-- Search Bar
+-- Table
|   +-- Table Header
|       +-- Column Headers (sortable)
|       +-- Sort Indicators
|   +-- Table Body
|       +-- Data Rows
|           +-- Data Cells
|           +-- Action Buttons/Dropdown
+-- Pagination Controls
```

## Component States Matrix

| Component | Default | Hover | Active | Focus | Disabled | Error | Loading | Empty |
|-----------|---------|-------|--------|-------|----------|-------|---------|-------|
| Login Form | x | - | - | x | - | x | x | - |
| Button | x | x | x | x | x | - | x | - |
| Input Field | x | x | - | x | x | x | - | x |
| Drag-Drop Zone | x | x | x | x | - | x | x | - |
| Data Table | x | x | - | x | - | - | x | x |
| Status Badge | x | x | - | - | - | - | - | - |
| Source Reference Link | x | x | x | x | - | - | - | - |
| Accept/Reject Buttons | x | x | x | x | x | - | x | - |
| Modal Dialog | x | - | - | x | - | - | - | - |
| Toast Notification | x | - | - | - | - | - | - | - |
| Chat Interface | x | - | - | x | - | x | x | x |
| Filter Panel | x | - | x | x | - | - | - | - |
| Sidebar Navigation | x | x | x | x | - | - | - | - |
| Tab Navigation | x | x | x | x | - | - | - | - |
| Pagination | x | x | x | x | x | - | - | - |

## Reusability Analysis

| Component | Reuse Count | Screens | Recommendation |
|-----------|-------------|---------|----------------|
| Header with Navigation | 10+ screens | All authenticated screens | Create as shared layout component |
| Sidebar Navigation | 10+ screens | All authenticated screens | Create as shared layout component |
| Data Table | 3 screens | Document List, User Management, Audit Logs | Create as reusable component with configurable columns |
| Modal Dialog | 5+ instances | User Management, Patient 360, Audit Logs, Global | Create as reusable component with slot-based content |
| Toast Notification | All screens | Global notification system | Create as global singleton service |
| Status Badge | 4 screens | Document List, Patient 360, User Management | Create as reusable component with variant prop |
| Source Reference Link | 3 screens | Patient 360, Conflict Resolution, AI Assistant | Create as reusable component |
| Button | All screens | Universal | Create as reusable component with variants (primary, secondary, danger, ghost) |
| Input Field | All forms | Login, Password Reset, User Management, Filters | Create as reusable form component with validation |
| Loading Spinner | All screens | Global loading states | Create as reusable component with size variants |
| Error Message | All forms | Universal form validation | Create as reusable component |
| Metrics Card | 2 screens | Admin Dashboard, User Dashboard | Create as reusable component |
| Filter Panel | 2 screens | Audit Logs, Document List | Create as reusable component with configurable filters |
| Pagination Controls | 3 screens | Document List, User Management, Audit Logs | Create as reusable component |
| Breadcrumb Navigation | 2 screens | Patient 360, Conflict Resolution | Create as reusable component |

## Responsive Breakpoints Summary

| Breakpoint | Width | Components Affected | Key Adaptations |
|-----------|-------|-------------------|-----------------|
| Desktop | 1440px | All components | Full desktop layout, expanded sidebar, multi-column grids, full data tables |

**Note**: Phase 1 targets desktop only (1280px minimum). Tablet and mobile responsive adaptations deferred to Phase 2.

## Implementation Priority Matrix

### High Priority (Core Components - P0)
- [x] Header with Navigation - Used in all authenticated screens, critical for navigation
- [x] Sidebar Navigation - Used in all authenticated screens, primary navigation pattern
- [x] Login Form - Entry point to application, authentication critical
- [x] Password Reset Form - Security requirement, user account recovery
- [x] Drag-Drop Upload Zone - Core feature, document ingestion
- [x] File List with Progress - Core feature, upload feedback
- [x] Data Table - Used in 3 major screens, data management critical
- [x] Patient Profile Card - Core feature, patient data display
- [x] Clinical Data Section - Core feature, clinical entity display
- [x] Source Reference Link - Core feature, explainability requirement
- [x] Conflict Comparison Card - Core feature, conflict resolution
- [x] ICD-10/CPT Code Table - Core feature, billing code review
- [x] Accept/Reject Button Group - Core feature, code verification workflow
- [x] Modal Dialog - Used across multiple features, critical interactions
- [x] Toast Notification - Universal feedback mechanism
- [x] Status Badge - Visual status communication across features
- [x] Loading Spinner - User feedback for async operations
- [x] Error Message - Form validation and error handling

### Medium Priority (Feature Components - P1)
- [x] Chat Interface - AI Clinical Assistant feature
- [x] Metrics Card - Dashboard analytics
- [x] Chart/Graph Component - Admin analytics visualization
- [x] Filter Panel - Data filtering for tables
- [x] Breadcrumb Navigation - Deep navigation support
- [x] Tab Navigation - Patient 360 View organization
- [x] Pagination Controls - Table navigation

### Low Priority (Enhancement Components - P2)
- [ ] Advanced Search - Enhanced filtering capabilities
- [ ] Bulk Actions - Multi-select table operations
- [ ] Keyboard Shortcuts - Power user efficiency
- [ ] Dark Mode Toggle - User preference (deferred to Phase 2)

## Framework-Specific Notes
**Detected Framework**: React (inferred from healthcare application requirements and modern web stack)
**Component Library**: Custom components with potential integration of headless UI libraries (Radix UI, Headless UI) for accessibility

### Framework Patterns Applied
- **Component Composition**: Atomic design principles - atoms (buttons, inputs), molecules (form groups, card headers), organisms (forms, tables), templates (page layouts)
- **State Management**: React Context for global state (auth, notifications), local state for component-specific interactions
- **Routing**: React Router for navigation between screens with role-based route guards
- **Form Handling**: React Hook Form for form validation and submission
- **Data Fetching**: React Query for async data fetching, caching, and synchronization

### Component Library Mappings
| Wireframe Component | Framework Component | Customization Required |
|-------------------|-------------------|----------------------|
| Button | Custom Button component | Variants (primary, secondary, danger, ghost), sizes, loading state |
| Input Field | Custom Input component | Validation states, error messages, icons, password toggle |
| Modal Dialog | Radix UI Dialog | Custom styling, focus trap, animations |
| Toast Notification | Custom Toast component | Auto-dismiss, stacking, variants |
| Data Table | TanStack Table (React Table) | Custom styling, sorting, filtering, pagination |
| Drag-Drop Zone | react-dropzone | Custom styling, validation, progress tracking |
| Chart/Graph | Chart.js or Recharts | Custom styling, responsive sizing, accessibility |
| Date Picker | react-datepicker | Custom styling, date range selection |
| Dropdown/Select | Radix UI Select | Custom styling, multi-select, search |

## Accessibility Considerations

| Component | ARIA Attributes | Keyboard Navigation | Screen Reader Notes |
|-----------|----------------|-------------------|-------------------|
| Header with Navigation | role="banner", aria-label="Main navigation" | Tab to user dropdown, Enter to open | Announces "Main navigation banner" |
| Sidebar Navigation | role="navigation", aria-label="Primary navigation" | Tab through menu items, Enter to activate | Announces current page with aria-current="page" |
| Login Form | aria-label="Login form", aria-describedby for errors | Tab through fields, Enter to submit | Announces field labels and validation errors |
| Drag-Drop Zone | aria-label="File upload area", aria-describedby | Tab to activate, Space/Enter to open file picker | Announces "File upload area, click or drag files" |
| Data Table | role="table", aria-sort on sortable headers | Tab through cells, Enter to sort | Announces column headers, row count, sort state |
| Modal Dialog | role="dialog", aria-labelledby, aria-describedby | Tab trap within modal, ESC to close | Announces dialog title and description |
| Toast Notification | role="alert" (error), aria-live="polite" (info) | Focus on action button if present | Auto-announces message content |
| Status Badge | aria-label with full status description | Not focusable | Announces status with context (e.g., "Document status: Completed") |
| Source Reference Link | aria-label with document context | Tab to focus, Enter to activate | Announces "View source: [Document Name], page [X]" |
| Accept/Reject Buttons | aria-label with code context | Tab to focus, Enter to activate | Announces "Accept ICD-10 code [code]" |
| Chat Interface | aria-label="Chat messages", aria-live="polite" | Tab to input, Enter to send | Announces new messages as they arrive |
| Filter Panel | role="search" or role="form", aria-label="Filter options" | Tab through controls, Enter to apply | Announces filter labels and current selections |
| Breadcrumb Navigation | aria-label="Breadcrumb", aria-current="page" | Tab through links, Enter to navigate | Announces navigation path |
| Tab Navigation | role="tablist", role="tab", aria-selected | Arrow keys to navigate tabs, Enter to activate | Announces tab name and selected state |
| Pagination | aria-label="Pagination", aria-current="page" | Tab through page numbers, Enter to navigate | Announces "Page [X] of [Y]" |

## Design System Integration

**Design System Reference**: Design tokens defined in design-tokens-applied.md

### Components Matching Design System
- [x] Button - Uses design token colors, spacing, radius, typography
- [x] Input Field - Uses design token colors, spacing, radius, typography
- [x] Card Components - Uses design token spacing, radius, shadows
- [x] Status Badge - Uses design token semantic colors
- [x] Typography - Uses design token font scale and weights
- [x] Spacing - Uses design token spacing scale (8px base unit)

### New Components to Add to Design System
- [x] Source Reference Link - New pattern for explainability, requires design token definition
- [x] Conflict Comparison Card - New pattern for conflict resolution UI
- [x] Accept/Reject Button Group - New pattern for code verification workflow
- [x] Drag-Drop Upload Zone - New pattern for file upload interaction
- [x] Chat Interface - New pattern for AI assistant interaction
