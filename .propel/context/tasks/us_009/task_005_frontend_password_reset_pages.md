# Task - TASK_005

## Requirement Reference
- User Story: us_009
- Story Location: .propel/context/tasks/us_009/us_009.md
- Acceptance Criteria: 
    - AC-1: Given user is on /forgot-password page, When valid email is submitted, Then success message is displayed and email is sent
    - AC-2: Given user receives reset email, When reset link is clicked, Then /reset-password page loads with token from URL
    - AC-3: Given user is on /reset-password page, When new password is entered, Then real-time validation shows which requirements are met
    - AC-4: Given user submits valid new password, When reset is successful, Then success message is shown and user is redirected to login after 3 seconds
    - AC-5: Given reset token is expired or invalid, When page loads, Then clear error message is displayed with option to request new reset

## Task Overview
Implement frontend pages for forgot password and reset password flows with email input validation, password strength indicator, real-time requirement checking, accessible design, and proper error handling for expired/invalid tokens.
Estimated Effort: 5 hours

## Dependent Tasks
- task_003_password_reset_email_flow (backend forgot password endpoint exists)
- task_004_password_reset_completion_endpoint (backend reset password endpoint exists)
- US_006 - Login Page UI (design tokens and styles exist)

## Impacted Components
- app/src/pages/ForgotPasswordPage.tsx
- app/src/pages/ResetPasswordPage.tsx
- app/src/App.tsx
- app/src/components/PasswordStrengthIndicator.tsx (optional)

## Implementation Plan
- Create ForgotPasswordPage component (/forgot-password):
  - Email input field with validation (RFC 5322)
  - Submit button with loading state
  - "Back to Login" link
  - Success message after submission (always same message)
  - Error handling for network issues
  - Rate limit error display (429 response)
  - Accessible form with proper labels and ARIA attributes
  - Responsive design matching login page
  - Prevent duplicate submissions
- Create ResetPasswordPage component (/reset-password):
  - Extract token from URL query parameter (?token=xxx)
  - New password input field
  - Confirm password input field
  - Password visibility toggle
  - Real-time password strength indicator showing:
    - ✓/✗ At least 8 characters
    - ✓/✗ One uppercase letter
    - ✓/✗ One lowercase letter
    - ✓/✗ One number
    - ✓/✗ One special character
  - Password mismatch error
  - Submit button (disabled until all requirements met)
  - Success message with countdown redirect to login
  - Error messages for:
    - Invalid token format
    - Expired token (with link to request new reset)
    - Already used token
    - Network errors
  - Accessible and responsive design
- Update App.tsx routing:
  - Add route: `<Route path="/forgot-password" element={<ForgotPasswordPage />} />`
  - Add route: `<Route path="/reset-password" element={<ResetPasswordPage />} />`
- Optional: Create PasswordStrengthIndicator component:
  - Reusable component for password validation
  - Visual indicators (checkmarks/crosses)
  - Color coding (red/yellow/green)
  - Can be used in other password forms
- Add form validation utilities:
  - Email validation regex (RFC 5322 compliant)
  - Password complexity validation function
  - Reusable across components
- Style components:
  - Match existing login page design
  - Use design tokens from US_006
  - Ensure mobile responsiveness
  - Add loading animations
  - Success/error message styling
**Focus on how to implement**

## Current Project State
```
app/src/
├── pages/
│   └── LoginPage.tsx (exists with design tokens)
├── App.tsx (exists with login route)
└── index.css (exists with design tokens)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/pages/ForgotPasswordPage.tsx | Forgot password page with email input and submission |
| CREATE | app/src/pages/ResetPasswordPage.tsx | Reset password page with token validation and password inputs |
| MODIFY | app/src/App.tsx | Add routes for /forgot-password and /reset-password |
| CREATE | app/src/components/PasswordStrengthIndicator.tsx | Optional reusable password validation component |
| CREATE | app/src/utils/validation.ts | Email and password validation utilities |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/learn/managing-state
- https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA
- https://www.w3.org/WAI/WCAG21/Understanding/error-identification.html

## Build Commands
- cd app
- npm install
- npm run dev
- npm run build

## Implementation Validation Strategy
- Navigate to /forgot-password, submit valid email; validate success message and API call (AC-1).
- Click reset link from email; validate /reset-password page loads with token (AC-2).
- Type password on reset page; validate real-time requirement indicators update (AC-3).
- Submit valid password; validate success message and redirect to login after 3 seconds (AC-4).
- Load /reset-password with expired token; validate error message with link to request new reset (AC-5).

## Implementation Checklist
- [x] Create ForgotPasswordPage with email form
- [x] Add email validation
- [x] Handle success/error states
- [x] Create ResetPasswordPage with password form
- [x] Extract token from URL
- [x] Implement password strength indicator
- [x] Add password confirmation matching
- [x] Handle expired/invalid token errors
- [x] Add countdown redirect after success
- [x] Update App.tsx with routes
- [x] Style components to match login page
- [x] Test keyboard navigation
- [x] Test on mobile devices
- [x] Test with screen reader
