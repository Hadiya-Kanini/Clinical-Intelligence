# Task - [TASK_003]

## Requirement Reference
- User Story: [us_020]
- Story Location: [.propel/context/tasks/us_020/us_020.md]
- Acceptance Criteria: 
    - [Given any user input, When rendered in the UI, Then it is sanitized against XSS attacks.]

## Task Overview
Ensure the frontend renders any user-provided or server-provided text safely (no HTML injection), and establish a clear pattern for handling untrusted strings in UI components and pages.

## Dependent Tasks
- [N/A]

## Impacted Components
- [MODIFY | app/src/pages/Patient360Page.tsx | Validate that any citation/source text is rendered as plain text (no HTML interpretation) and remains safe if later wired to API data]
- [MODIFY | app/src/pages/DocumentListPage.tsx | Ensure document names/search inputs remain treated as plain text and do not introduce HTML rendering]
- [MODIFY | app/src/pages/UserManagementPage.tsx | Ensure user-entered values (name/email) are displayed as text and not used in any HTML injection patterns]
- [MODIFY | app/src/pages/DocumentUploadPage.tsx | Ensure file names and error strings are rendered safely and not injected into HTML]

## Implementation Plan
- Confirm current rendering patterns:
  - React renders string children safely by default (escapes HTML).
  - Ensure no usage of `dangerouslySetInnerHTML`, `.innerHTML`, or dynamic HTML parsing utilities.
- Add a small “safe rendering” guideline enforced by tests:
  - Create a unit test (or lightweight grep-based test) that fails if `dangerouslySetInnerHTML` or `innerHTML` is introduced in `app/src`.
- Validate future-proofing:
  - Pages like `Patient360Page` display `sourceText` and other narrative fields that may later become user-supplied clinical text.
  - Ensure any future need for rich text explicitly uses a vetted sanitizer (decision point) rather than ad-hoc HTML.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/security/xss_guardrail.test.ts | Add a guardrail test to detect introduction of `dangerouslySetInnerHTML` / `innerHTML` patterns in frontend source |
| MODIFY | app/src/pages/Patient360Page.tsx | Confirm displayed narrative values are kept as text-only rendering patterns |
| MODIFY | app/src/pages/DocumentListPage.tsx | Confirm safe rendering for document names and filter string usage |
| MODIFY | app/src/pages/UserManagementPage.tsx | Confirm safe rendering for user-entered values |
| MODIFY | app/src/pages/DocumentUploadPage.tsx | Confirm safe rendering for file names and errors |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react-dom/components/common#dangerously-setting-the-inner-html
- https://owasp.org/www-community/attacks/xss/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run frontend unit tests; ensure guardrail fails when unsafe HTML rendering APIs are introduced.
- [Manual] Enter strings resembling HTML/JS (e.g., `<img src=x onerror=alert(1)>`) into any UI input and confirm it renders as visible text, not executed markup.

## Implementation Checklist
- [ ] Add a frontend guardrail test for `dangerouslySetInnerHTML` / `innerHTML` usage
- [ ] Validate high-risk display areas (`Patient360Page` citations and narrative fields) render as text-only
- [ ] Validate user-entered values (User management, document search) are displayed without HTML injection
- [ ] Document a decision point: if rich text rendering is required later, choose a vetted sanitizer rather than introducing raw HTML
