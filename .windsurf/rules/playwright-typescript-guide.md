---
trigger: glob
globs: "**/*.ts"
---

# Playwright

## Test Writing Guidelines

### Code Quality Standards
- **Locators:** Prioritize user-facing, role-based locators (`getByRole`, `getByLabel`, `getByText`) for resilience and accessibility
- **test.step():** Group interactions to improve test readability and reporting
- **Assertions:** Use auto-retrying web-first assertions starting with `await` keyword (e.g., `await expect(locator).toHaveText()`)
- **Timeouts:** Rely on built-in auto-waiting; avoid hard-coded waits or increased default timeouts
- **Clarity:** Descriptive test and step titles stating intent; comments only for complex logic

### Test Structure
- **Imports:** Start with `import { test, expect } from '@playwright/test';`
- **Organization:** Group related tests under `test.describe()` block
- **Hooks:** Use `beforeEach` for setup actions common to all tests (e.g., navigating to page)
- **Titles:** Clear naming convention: `Feature - Specific action or scenario`

### File Organization
- **Location:** Store test files in `tests/` directory
- **Naming:** Convention `<feature-or-page>.spec.ts` (e.g., `login.spec.ts`, `search.spec.ts`)
- **Scope:** One test file per major application feature or page

### Assertion Best Practices
- **UI Structure:** `toMatchAriaSnapshot` for accessibility tree structure verification
- **Element Counts:** `toHaveCount` for number of elements
- **Text Content:** `toHaveText` for exact matches; `toContainText` for partial matches
- **Navigation:** `toHaveURL` to verify page URL after action

## Test Execution Strategy
1. **Initial Run:** Execute with `npx playwright test --project=chromium`
2. **Debug Failures:** Analyze failures and identify root causes
3. **Iterate:** Refine locators, assertions, or test logic
4. **Validate:** Ensure tests pass consistently and cover intended functionality
5. **Report:** Provide feedback on test results and issues discovered

## Quality Checklist
Before finalizing tests:
- [ ] All locators accessible and specific; avoid strict mode violations
- [ ] Tests grouped logically with clear structure
- [ ] Assertions meaningful and reflect user expectations
- [ ] Tests follow consistent naming conventions
- [ ] Code properly formatted and commented
