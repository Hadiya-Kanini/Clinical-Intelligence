---
description: Generates Playwright TypeScript automation scripts from test workflow specifications
auto_execution_mode: 1
---

# Playwright Script Generator

As an expert Test Automation Engineer, generate production-ready Playwright TypeScript scripts from test workflow specifications created by `/create-automation-test`.

## Input Parameters

### $ARGUMENTS (Optional)
**Accepts:** Workflow file path | Empty (auto-scan)
**Default:** Auto-discover `tw_*.md` and `e2e_*.md` files in `.propel/context/test/`

### --type (Optional)
**Accepts:** `feature` | `e2e` | `both`
**Default:** `both`

| Value | Input Files | Output Location |
|-------|-------------|-----------------|
| `feature` | `tw_*.md` | `test-automation/tests/<feature>.spec.ts` |
| `e2e` | `e2e_*.md` | `test-automation/e2e/<journey>.spec.ts` |
| `both` | Both patterns | Both locations (default) |

### Input Processing Instructions

| Input | Action |
|-------|--------|
| Empty | Scan `.propel/context/test/tw_*.md` and `e2e_*.md` |
| File path | Process specified workflow file |
| Feature name | Find matching `tw_<feature>.md` or `e2e_<journey>.md` |

## Output

| --type Value | Artifacts Generated |
|--------------|---------------------|
| `feature` | `test-automation/tests/<feature>.spec.ts` |
| `e2e` | `test-automation/e2e/<journey>.spec.ts` |
| `both` | Both locations |

**Common Artifacts:**
- `test-automation/pages/<page>.page.ts`
- `test-automation/data/<feature>.json`
- `test-automation/playwright.config.ts` (if not exists)

**Print** (console only):
- Rules used (bulleted list)
- Evaluation Scores (table)
- Summary (<100 words)

## Execution Flow

### Deep Research Methodology

**MCP Tools Required:**
- `mcp__sequential-thinking__sequentialthinking` - Systematic code generation
- `mcp__context7__resolve-library-id` - Pin Playwright version
- `mcp__context7__get-library-docs` - Fetch API documentation

**Fallback:** If MCP unavailable, use structured iterative analysis.

### Step 1: Workflow Discovery

| --type Value | Scan Pattern |
|--------------|--------------|
| `feature` | `.propel/context/test/tw_*.md` |
| `e2e` | `.propel/context/test/e2e_*.md` |
| `both` | Both patterns |

1. If $ARGUMENTS empty: scan based on --type
2. Parse YAML blocks from workflow file
3. Extract: test_cases, page_objects, test_data, metadata

### Step 2: Technology Research
**Use:** `mcp__context7__resolve-library-id`, `mcp__context7__get-library-docs`

1. Resolve and pin Playwright version
2. Fetch current assertion API patterns
3. Fetch page object best practices
4. Detect TypeScript version for compatibility

### Step 3: Test File Generation (--type feature or both)
**Use:** `mcp__sequential-thinking__sequentialthinking`
**Condition:** Execute when --type is `feature` or `both`

**Output:** `test-automation/tests/<feature>.spec.ts`

**Template:**
```typescript
import { test, expect } from '@playwright/test';
import { [PageName]Page } from '../pages/[page].page';

test.describe('[FEATURE_NAME]', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('[BASE_URL]');
  });

  test('[TC_ID]: [TEST_NAME]', async ({ page }) => {
    // Step-by-step from workflow YAML
  });
});
```

**Generation Rules:**
- One `test.describe` block per feature
- One `test()` per TC-XXX
- `beforeEach` for navigation
- Assertions use `await expect()`

### Step 3.5: E2E Test File Generation (--type e2e or both)
**Use:** `mcp__sequential-thinking__sequentialthinking`
**Condition:** Execute when --type is `e2e` or `both`

**Output:** `test-automation/e2e/<journey>.spec.ts`

**Template:**
```typescript
import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { DashboardPage } from '../pages/dashboard.page';
import { [PageName]Page } from '../pages/[page].page';

test.describe('E2E: [JOURNEY_NAME]', () => {
  test('[TC_ID]: Complete user journey', async ({ page }) => {
    // Phase 1: [UC-001 NAME]
    const loginPage = new LoginPage(page);
    await page.goto('/login');
    await loginPage.login(testData.user.email, testData.user.password);
    await expect(page).toHaveURL(/dashboard/);

    // Phase 2: [UC-002 NAME]
    const dashboardPage = new DashboardPage(page);
    await dashboardPage.navigateToOrders();
    await expect(page).toHaveURL(/orders/);

    // Phase 3: [UC-003 NAME]
    const orderPage = new OrderPage(page);
    await orderPage.createOrder(testData.order);
    await expect(page.getByText('Order Created')).toBeVisible();
  });
});
```

**E2E Generation Rules:**
- One `test.describe` block per journey
- One `test()` per E2E journey (phases are sequential within test)
- NO `beforeEach` for E2E (journey maintains state across phases)
- Checkpoints validate intermediate states
- Session state shared across all phases

### Step 4: Page Object Generation
**Use:** `mcp__context7__get-library-docs` + `mcp__sequential-thinking__sequentialthinking`

**Output:** `test-automation/pages/<page>.page.ts`

**Template:**
```typescript
import { Page, Locator } from '@playwright/test';

export class [PageName]Page {
  constructor(private page: Page) {}

  // Locators as getters
  get [elementName](): Locator {
    return this.page.getByRole('[role]', { name: '[name]' });
  }

  // Actions (no assertions)
  async [actionName]([params]): Promise<void> {
    // Action implementation
  }
}
```

**Rules:**
- Locators as readonly getters
- Actions as async methods
- NO assertions in page objects
- NO waitForTimeout

### Step 5: Test Data Generation

**Output:** `test-automation/data/<feature>.json`

Extract test_data from workflow YAML into JSON fixtures.

### Step 6: Configuration

**Output:** `test-automation/playwright.config.ts` (if not exists)

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  timeout: 30000,
  expect: { timeout: 5000 },
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: 'html',
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
});
```

### Step 7: Validation

1. Run: `npx tsc --noEmit`
2. Fix any TypeScript errors
3. Verify all TC-XXX have corresponding test() blocks

## Guardrails

- `rules/playwright-testing-guide.md`: Test independence, wait strategies
- `rules/playwright-typescript-guide.md`: Code quality, assertions
- `rules/playwright-standards.md`: Locator priority, templates
- `rules/typescript-styleguide.md`: TypeScript conventions

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

### Playwright Script Quality Assessment

| # | Dimension | Score | Gate | Criteria |
|---|-----------|-------|------|----------|
| 1 | TypeScript Validity | [PASS/FAIL] | MUST PASS | `tsc --noEmit` succeeds |
| 2 | Test Coverage | [0-100%] | >=80% | All TC-XXX have test() |
| 3 | Locator Quality | [0-100%] | >=80% | getByRole/getByTestId; no CSS |
| 4 | Wait Strategy | [0-100%] | >=80% | No waitForTimeout |
| 5 | Test Isolation | [PASS/FAIL] | MUST PASS | Each test independent |
| 6 | POM Compliance | [0-100%] | >=80% | No assertions in page objects |
| 7 | E2E Coverage | [0-100%] | >=80% | All E2E journeys have test() (if --type e2e/both) |
| 8 | Journey Continuity | [PASS/FAIL] | MUST PASS | E2E tests maintain session state (if --type e2e/both) |

### Detection Rules

| # | Logic | Penalty |
|---|-------|---------|
| 1 | Run `tsc --noEmit` | Error: BLOCKED |
| 2 | Count TC in workflow vs test() | Missing: -15%/instance |
| 3 | Grep for getByRole vs CSS | CSS: -10%/instance |
| 4 | Grep for waitForTimeout | Found: -20%/instance |
| 5 | Tests use beforeEach; no shared state | Shared: BLOCKED |
| 6 | Page objects: no expect() | Assertion in POM: -15%/instance |
| 7 | E2E journeys have test() blocks | Missing: -15%/instance |
| 8 | E2E tests use shared page context | No session: BLOCKED |

**Overall Score**: [Weighted Average]%

**Evaluation Summary** (Top 3 Weaknesses):
1. **[Lowest]** ([X]%): [Reason]
2. **[2nd Lowest]** ([X]%): [Reason]
3. **[3rd Lowest]** ([X]%): [Reason]

**Critical Failures**: [List MUST gates that failed, or "None"]

### Self-Healing Protocol

**Retry Limits:** 1 patch per dimension | Mode: Edit-in-place

**Patchable:**
| Dimension | Patch Action |
|-----------|--------------|
| Test Coverage | Add missing test() blocks |
| Locator Quality | Replace CSS with role-based |
| Wait Strategy | Replace waitForTimeout with expect |
| POM Compliance | Move assertions to test file |
| E2E Coverage | Add missing E2E test() blocks |
| Journey Continuity | Fix session state handling |

**Non-Patchable:** TypeScript errors, Test Isolation failures, Journey Continuity failures, >=3 failing dimensions

**Retry Flow:**
```
IF tsc fails: REPORT "[MANUAL FIX] TypeScript errors"
READ generated files
FOR each failing dimension:
  IF patchable: EDIT in-place; RE-EVALUATE once
  ELSE: REPORT "[MANUAL FIX] {dimension}"
IF >=3 manual fixes needed: ABORT all retries
```

---

*Generates production-ready Playwright TypeScript scripts from test workflow specifications.*
