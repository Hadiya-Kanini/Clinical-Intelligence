---
trigger: glob
globs: "**/*.spec.ts, **/*.test.ts"
---

# Playwright Standards Quick Reference

Complements `playwright-testing-guide.md` and `playwright-typescript-guide.md` with actionable checklists.

## Locator Priority (Use in Order)

| # | Method | Example | When to Use |
|---|--------|---------|-------------|
| 1 | getByRole | `getByRole('button', {name: 'Submit'})` | Buttons, links, headings, inputs |
| 2 | getByTestId | `getByTestId('user-menu')` | Custom components, complex widgets |
| 3 | getByLabel | `getByLabel('Email address')` | Form inputs with labels |
| 4 | getByPlaceholder | `getByPlaceholder('Enter email')` | Inputs without visible labels |
| 5 | getByText | `getByText('Welcome back')` | Static text content |

## Forbidden Patterns

| Pattern | Why Forbidden | Use Instead |
|---------|---------------|-------------|
| `waitForTimeout(ms)` | Flaky, slow | `expect(locator).toBeVisible()` |
| `.btn-primary` | Styling changes | `getByRole('button')` |
| `#user-123-abc` | Dynamic IDs | `getByTestId('user-profile')` |
| `div:nth-child(3)` | DOM structure changes | Role-based locator |
| Shared test state | Cascading failures | Independent tests |

## Required Patterns Checklist

### Test Structure
- [ ] Each test is independent (passes when run alone)
- [ ] `beforeEach` handles navigation/setup
- [ ] `afterEach` clears localStorage/sessionStorage if mutating
- [ ] No test depends on another test's state

### Assertions
- [ ] Use web-first assertions: `await expect(locator).toBeVisible()`
- [ ] URL checks: `await expect(page).toHaveURL(/pattern/)`
- [ ] Text checks: `await expect(locator).toHaveText('exact')`
- [ ] No raw `.toBeTruthy()` on element existence

### Page Objects
- [ ] Locators as getters only
- [ ] Actions as methods (no assertions)
- [ ] No `waitForTimeout` inside page objects
- [ ] No test logic inside page objects

## Assertion Quick Reference

| Check | Assertion |
|-------|-----------|
| Element visible | `await expect(locator).toBeVisible()` |
| Element hidden | `await expect(locator).toBeHidden()` |
| Exact text | `await expect(locator).toHaveText('exact')` |
| Partial text | `await expect(locator).toContainText('partial')` |
| URL pattern | `await expect(page).toHaveURL(/dashboard/)` |
| Element count | `await expect(locator).toHaveCount(3)` |
| Input value | `await expect(locator).toHaveValue('test@example.com')` |
| Enabled | `await expect(locator).toBeEnabled()` |
| Checked | `await expect(locator).toBeChecked()` |

## Page Object Template

```typescript
export class LoginPage {
  constructor(private page: Page) {}

  // Locators as getters
  get emailInput() { return this.page.getByLabel('Email'); }
  get passwordInput() { return this.page.getByLabel('Password'); }
  get submitButton() { return this.page.getByRole('button', { name: 'Login' }); }
  get errorAlert() { return this.page.getByRole('alert'); }

  // Actions (no assertions)
  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.submitButton.click();
  }
}
```

## Test File Template

```typescript
import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';

test.describe('Feature Name', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('TC-UC001-HP-001: should login successfully', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.login('user@example.com', 'password');
    await expect(page).toHaveURL(/dashboard/);
  });
});
```

## Anti-Pattern Detection

| If You See | Replace With |
|------------|--------------|
| `await page.waitForTimeout(2000)` | `await expect(element).toBeVisible()` |
| `page.locator('.btn')` | `page.getByRole('button', {name})` |
| `expect(await el.isVisible()).toBe(true)` | `await expect(el).toBeVisible()` |
| `test('test1', ...)` | Descriptive name: `test('TC-XXX: should...')` |

---
*Quick reference for Playwright automation. See playwright-testing-guide.md for full details.*
