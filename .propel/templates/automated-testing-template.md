# Test Workflow: [FEATURE_NAME]

## Metadata
| Field | Value |
|-------|-------|
| Feature | [FEATURE_NAME] |
| Source | [PATH_TO_SPEC] |
| Use Case | [UC-XXX] |
| Base URL | [APPLICATION_URL] |

## Test Cases

### TC-[UC_ID]-HP-001: [HAPPY_PATH_NAME]
**Type:** happy_path | **Priority:** P0

**Preconditions:**
- [PRECONDITION_1]
- [PRECONDITION_2]

**Steps:**
```yaml
steps:
  - step_id: "001"
    action: navigate
    target: "[URL]"
    expect: "page loads successfully"

  - step_id: "002"
    action: fill
    target: "getByLabel('[FIELD_LABEL]')"
    value: "[TEST_VALUE]"
    expect: "field accepts input"

  - step_id: "003"
    action: click
    target: "getByRole('button', {name: '[BUTTON_NAME]'})"
    expect: "[EXPECTED_RESULT]"

  - step_id: "004"
    action: verify
    target: "getByText('[SUCCESS_MESSAGE]')"
    expect: "visible"
```

**Test Data:**
```yaml
test_data:
  [FIELD_NAME]: "[VALUE]"
  [FIELD_NAME_2]: "[VALUE_2]"
```

---

### TC-[UC_ID]-EC-001: [EDGE_CASE_NAME]
**Type:** edge_case | **Priority:** P1

**Scenario:** [BOUNDARY_CONDITION_DESCRIPTION]

**Steps:**
```yaml
steps:
  - step_id: "EC001"
    action: [ACTION_TYPE]
    target: "[LOCATOR]"
    value: "[BOUNDARY_VALUE]"
    expect: "[EXPECTED_BEHAVIOR]"
```

---

### TC-[UC_ID]-ER-001: [ERROR_CASE_NAME]
**Type:** error | **Priority:** P1

**Trigger:** [WHAT_CAUSES_ERROR]

**Steps:**
```yaml
steps:
  - step_id: "ER001"
    action: [ACTION_TYPE]
    target: "[LOCATOR]"
    value: "[INVALID_VALUE]"
    expect: "[ERROR_MESSAGE_OR_BEHAVIOR]"
```

---

## Page Objects
```yaml
pages:
  - name: "[PageName]Page"
    file: "pages/[page-name].page.ts"
    elements:
      - submitButton: "getByRole('button', {name: '[NAME]'})"
      - emailInput: "getByLabel('[LABEL]')"
      - errorAlert: "getByRole('alert')"
      - [elementName]: "getByTestId('[TEST_ID]')"
    actions:
      - login(email, password): "Fill credentials and submit"
      - [methodName]: "[PURPOSE]"
```

## Success Criteria
- [ ] All happy path steps execute without errors
- [ ] Edge case validations pass
- [ ] Error scenarios handled correctly
- [ ] Test runs independently (no shared state)
- [ ] All assertions use web-first patterns

## Locator Reference
| Priority | Method | Example |
|----------|--------|---------|
| 1st | getByRole | `getByRole('button', {name: 'Submit'})` |
| 2nd | getByTestId | `getByTestId('user-menu')` |
| 3rd | getByLabel | `getByLabel('Email')` |
| AVOID | CSS | `.btn-primary`, `#dynamic-123` |

---
*Template: automated-testing-template.md | Output: .propel/context/test/tw_[feature]_[YYYYMMDD].md*
