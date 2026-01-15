# Task - [TASK_002]

## Requirement Reference
- User Story: [us_018] (extracted from input)
- Story Location: [.propel/context/tasks/us_018/us_018.md]
- Acceptance Criteria: 
    - [Given a user enters an email address, When validation runs, Then RFC 5322 compliant regex patterns are used.]
    - [Given an invalid email format, When validation fails, Then a clear error message indicates the specific issue.]
    - [Given email validation, When implemented, Then it validates on both frontend (immediate feedback) and backend (security).]
    - [Given edge-case valid emails (e.g., user+tag@domain.com), When entered, Then they are accepted as valid.]

## Task Overview
Standardize frontend email validation by replacing the current simplistic email regex with a shared RFC 5322-compliant validator utility and applying it consistently across user-facing email inputs (login + forgot password, and any other pages that accept email).

## Dependent Tasks
- [N/A]

## Impacted Components
- [CREATE | app/src/lib/validation/email.ts | Shared RFC 5322 email regex + helper `isValidEmail(...)`]
- [MODIFY | app/src/pages/LoginPage.tsx | Replace simplistic regex with shared validator; provide clearer error text]
- [MODIFY | app/src/pages/ForgotPasswordPage.tsx | Replace simplistic regex with shared validator; provide clearer error text]
- [MODIFY | app/src/pages/UserManagementPage.tsx | Add email format validation before create/edit to prevent invalid email inputs]

## Implementation Plan
- Create a shared email validation utility:
  - Export a single RFC 5322-compliant regex and a helper function (e.g., `isValidEmailRfc5322(email: string): boolean`).
  - Normalize input consistently (trim; avoid lowercasing for local-part display but validate normalized string).
- Update login form validation:
  - Replace `/^[^\s@]+@[^\s@]+\.[^\s@]+$/` with shared validator.
  - Ensure error message remains user-friendly and specific (format error vs required).
- Update forgot password validation:
  - Replace simplistic regex with shared validator.
  - Keep existing non-enumeration UX intact (success message can remain generic).
- Update user management create/edit modal:
  - Add email format validation (currently only checks required fields).
  - Provide clear toast error on invalid email.
- Ensure consistency:
  - The same regex/validator is used everywhere in the frontend to prevent drift.

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/lib/validation/email.ts | RFC 5322 email validation utility used by all email inputs |
| MODIFY | app/src/pages/LoginPage.tsx | Use shared RFC validator and improve format error messaging |
| MODIFY | app/src/pages/ForgotPasswordPage.tsx | Use shared RFC validator and improve format error messaging |
| MODIFY | app/src/pages/UserManagementPage.tsx | Prevent create/edit with invalid email format and show clear error |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.rfc-editor.org/rfc/rfc5322

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual] Confirm `user+tag@domain.com` is accepted on login + forgot password + user management forms.
- [Manual] Confirm invalid format produces a specific error message distinct from "required".
- [Automated] Add/adjust frontend tests (see TASK_003) to lock the validator behavior.

## Implementation Checklist
- [x] Create shared `email.ts` validator utility with RFC 5322 regex
- [x] Replace email regex usage in `LoginPage.tsx`
- [x] Replace email regex usage in `ForgotPasswordPage.tsx`
- [x] Add email format validation to `UserManagementPage.tsx` create/edit flow
- [x] Ensure error messages are specific and accessible
- [x] Verify edge-case valid emails (`+tag`) are accepted

## Design Reference

## UI Impact Assessment
**Has UI Changes**: [ ] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[018]
**Story Title**: Implement RFC-compliant email validation
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: .propel/context/docs/designsystem.md
- **Screen Spec**: .propel/context/docs/figma_spec.md

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Login + forgot password email validation | N/A | N/A | Clear validation messaging for invalid email formats (FR-009b, FR-009f) | High |

### Task Design Mapping
```yaml
TASK_002:
  title: "Frontend - RFC 5322 email validation"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-001 Login", "SCR-003 Forgot Password", "UXR-002"]
  components_affected:
    - LoginPage
    - ForgotPasswordPage
    - UserManagementPage
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Screen Reader**: Validation errors announced via existing field error patterns
