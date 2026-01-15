# E2E Test Workflow: [JOURNEY_NAME]

## Metadata
| Field | Value |
|-------|-------|
| Journey | [JOURNEY_NAME] |
| Source | [PATH_TO_SPEC] |
| UC Chain | [UC-001 → UC-002 → UC-003] |
| Base URL | [APPLICATION_URL] |

## Journey Overview

### TC-E2E-[JOURNEY_ID]-001: [JOURNEY_NAME]
**Type:** e2e | **Priority:** P0

**Journey Flow:**
| Step | Use Case | Action | Expected State |
|------|----------|--------|----------------|
| 1 | UC-001 | [ACTION] | [STATE] |
| 2 | UC-002 | [ACTION] | [STATE] |
| 3 | UC-003 | [ACTION] | [STATE] |

**Session Requirements:**
- Authentication: [REQUIRED/NOT_REQUIRED]
- State Persistence: [COOKIES/LOCAL_STORAGE/SESSION]
- Cleanup: [STRATEGY]

**Steps:**
```yaml
e2e_steps:
  - phase: "UC-001: [NAME]"
    steps:
      - step_id: "E2E-001"
        action: navigate
        target: "[URL]"
        expect: "page loads successfully"

      - step_id: "E2E-002"
        action: fill
        target: "getByLabel('[FIELD_LABEL]')"
        value: "[TEST_VALUE]"
        expect: "field accepts input"

      - step_id: "E2E-003"
        action: click
        target: "getByRole('button', {name: '[BUTTON_NAME]'})"
        expect: "[EXPECTED_RESULT]"
        checkpoint: true

  - phase: "UC-002: [NAME]"
    steps:
      - step_id: "E2E-004"
        action: verify
        target: "getByText('[SUCCESS_MESSAGE]')"
        expect: "visible"

      - step_id: "E2E-005"
        action: click
        target: "getByRole('link', {name: '[LINK_NAME]'})"
        expect: "navigation to [NEXT_PAGE]"
        checkpoint: true

  - phase: "UC-003: [NAME]"
    steps:
      - step_id: "E2E-006"
        action: [ACTION_TYPE]
        target: "[LOCATOR]"
        expect: "[EXPECTED_RESULT]"
        checkpoint: true
```

**Journey Test Data:**
```yaml
journey_data:
  user:
    email: "[EMAIL]"
    password: "[PASSWORD]"
  [entity_name]:
    [field_1]: "[VALUE_1]"
    [field_2]: "[VALUE_2]"
```

---

## Page Objects
```yaml
pages:
  - name: "[PageName]Page"
    file: "pages/[page-name].page.ts"
    elements:
      - [elementName]: "getByRole('[role]', {name: '[name]'})"
      - [elementName]: "getByLabel('[LABEL]')"
      - [elementName]: "getByTestId('[TEST_ID]')"
    actions:
      - [methodName](params): "[PURPOSE]"
```

## Success Criteria
- [ ] All journey phases complete without errors
- [ ] Session state maintained across phases
- [ ] Checkpoints validate intermediate states
- [ ] Journey runs independently
- [ ] All assertions use web-first patterns

## Locator Reference
| Priority | Method | Example |
|----------|--------|---------|
| 1st | getByRole | `getByRole('button', {name: 'Submit'})` |
| 2nd | getByTestId | `getByTestId('user-menu')` |
| 3rd | getByLabel | `getByLabel('Email')` |
| AVOID | CSS | `.btn-primary`, `#dynamic-123` |

---
*Template: automated-e2e-template.md | Output: .propel/context/test/e2e_[journey].md*
