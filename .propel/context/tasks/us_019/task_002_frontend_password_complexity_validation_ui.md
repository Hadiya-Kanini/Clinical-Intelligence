# Task - [TASK_002]

## Requirement Reference
- User Story: [us_019]
- Story Location: [.propel/context/tasks/us_019/us_019.md]
- Acceptance Criteria: 
    - [Given password complexity requirements, When the user types, Then the UI shows requirements with real-time checkmarks as each is met (UXR-012).]
    - [Given an invalid password, When validation fails, Then specific missing requirements are highlighted.]
    - [Given a user sets or changes a password, When complexity is validated, Then it must have minimum 8 characters.]
    - [Given password complexity validation, When checked, Then it must contain at least one uppercase letter, one lowercase letter, one number, and one special character.]

## Task Overview
Update the reset password UI to enforce password complexity (FR-009c) locally with real-time requirement checkmarks and targeted error messaging so users immediately see what is missing before submission.

## Dependent Tasks
- [N/A]

## Impacted Components
- [MODIFY | app/src/pages/ResetPasswordPage.tsx | Extend validation to enforce uppercase/lowercase/number/special requirements and highlight missing requirements]
- [CREATE | app/src/utils/passwordPolicy.ts | Centralize client-side password requirement checks so UI and validation logic stay consistent]

## Implementation Plan
- Create a small client-side password policy helper:
  - Compute requirement statuses: length, lowercase, uppercase, digit, special character.
  - Return a list of unmet requirements (used for both inline errors and UI highlighting).
- Update `ResetPasswordPage`:
  - Replace current `validate()` (length-only) with policy-based validation.
  - Keep existing real-time requirement list but ensure it is driven by the shared helper and highlights unmet requirements when submitted.
  - Ensure specific missing requirements are communicated accessibly (e.g., error text listing what’s missing).
- Ensure frontend and backend are aligned on:
  - Special character definition (and whether unicode symbols are accepted)
  - Maximum password length (if enforced)

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/utils/passwordPolicy.ts | Implement reusable password requirement checks + helper to list missing requirements |
| MODIFY | app/src/pages/ResetPasswordPage.tsx | Use shared password policy for real-time checkmarks and submission validation with targeted missing-requirement feedback |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/hooks/use-search-params

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] On Reset Password screen, verify each requirement toggles its state as the user types and submission shows targeted guidance when one or more requirements are missing.
- [Manual] Verify confirm password mismatch behavior remains correct.

## Implementation Checklist
- [x] Add `app/src/lib/validation/passwordPolicy.ts` with requirement checks and a "missing requirements" helper
- [x] Update `ResetPasswordPage.validate()` to enforce all required rules (length, lower, upper, number, special)
- [x] Highlight missing requirements on validation failure (not only generic error)
- [x] Ensure real-time requirement list and validation use the same underlying policy helper
- [x] Confirm accessibility: errors are associated to fields and missing requirements are readable
- [x] Align special character logic and max length behavior with backend policy

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-019
**Story Title**: Enforce password complexity requirements
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: [N/A]

### Screen-to-Design Mappings
| Screen/Feature | Description | Implementation Priority |
|---------------|-------------|----------------------|
| Reset Password | Real-time password requirement checkmarks and missing requirement highlighting (UXR-012) | High |

### Design Tokens
```yaml
colors:
  status_success:
    usage: "Met requirement checkmarks / met requirement text"
  status_muted:
    usage: "Unmet requirement text"
spacing:
  form_gap:
    usage: "Consistent spacing between inputs and requirement list"
```

### Accessibility Requirements
- **WCAG Level**: AA (for modified elements)
- **Screen Reader**: Missing requirements must be announced via associated error text
