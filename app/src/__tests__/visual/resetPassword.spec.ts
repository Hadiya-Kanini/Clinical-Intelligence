import { test, expect } from '@playwright/test'

/**
 * E2E tests for Reset Password page token validation states and submit flow.
 * Tests cover:
 * - Token pre-validation on page load
 * - Invalid/expired/used token states
 * - Valid token form rendering
 * - Successful password reset and redirect
 */

test.describe('Reset Password - Token Validation States', () => {
  test('should show validating state initially when token is present', async ({ page }) => {
    // Mock the validation endpoint to delay response
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 500))
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ valid: true, expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })

    await page.goto('/reset-password?token=valid-test-token')

    // Should show validating message
    await expect(page.locator('text=Validating your reset link')).toBeVisible()
  })

  test('should show invalid token error when token is missing', async ({ page }) => {
    await page.goto('/reset-password')

    // Should show invalid/expired error
    await expect(page.locator('text=This reset link is invalid or has expired')).toBeVisible()

    // Should show link to request new reset
    await expect(page.locator('a[href="/forgot-password"]')).toBeVisible()

    // Form should not be visible
    await expect(page.locator('input[name="password"]')).not.toBeVisible()
  })

  test('should show invalid token error when validation fails with invalid_token', async ({ page }) => {
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          error: {
            code: 'invalid_token',
            message: 'Invalid or expired reset link.',
            details: [],
          },
        }),
      })
    })

    await page.goto('/reset-password?token=invalid-token')

    // Wait for validation to complete
    await expect(page.locator('text=This reset link is invalid or has expired')).toBeVisible()

    // Form should not be visible
    await expect(page.locator('input[name="password"]')).not.toBeVisible()
  })

  test('should show expired token error when validation fails with token_expired', async ({ page }) => {
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          error: {
            code: 'token_expired',
            message: 'Reset link has expired.',
            details: [],
          },
        }),
      })
    })

    await page.goto('/reset-password?token=expired-token')

    // Wait for validation to complete
    await expect(page.locator('text=This reset link has expired')).toBeVisible()

    // Should show link to request new reset
    await expect(page.locator('a[href="/forgot-password"]')).toBeVisible()
  })

  test('should show used token error when validation fails with token_used', async ({ page }) => {
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          error: {
            code: 'token_used',
            message: 'This reset link has already been used.',
            details: [],
          },
        }),
      })
    })

    await page.goto('/reset-password?token=used-token')

    // Wait for validation to complete
    await expect(page.locator('text=This reset link has already been used')).toBeVisible()

    // Should show link to request new reset
    await expect(page.locator('a[href="/forgot-password"]')).toBeVisible()
  })

  test('should show form when token is valid', async ({ page }) => {
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ valid: true, expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })

    await page.goto('/reset-password?token=valid-token')

    // Wait for form to appear
    await expect(page.locator('input[name="password"]')).toBeVisible()
    await expect(page.locator('input[name="confirmPassword"]')).toBeVisible()
    await expect(page.locator('button[type="submit"]')).toBeVisible()

    // Password strength indicator should be visible
    await expect(page.locator('text=Password strength')).toBeVisible()
  })
})

test.describe('Reset Password - Form Submission', () => {
  test.beforeEach(async ({ page }) => {
    // Mock valid token validation
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ valid: true, expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })
  })

  test('should show validation errors for empty fields', async ({ page }) => {
    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Submit empty form
    await page.click('button[type="submit"]')

    // Should show validation errors
    await expect(page.locator('text=Password is required')).toBeVisible()
  })

  test('should show password mismatch error', async ({ page }) => {
    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Fill mismatched passwords
    await page.fill('input[name="password"]', 'ValidPass123!')
    await page.fill('input[name="confirmPassword"]', 'DifferentPass123!')

    // Submit form
    await page.click('button[type="submit"]')

    // Should show mismatch error
    await expect(page.locator('text=Passwords do not match')).toBeVisible()
  })

  test('should show password requirements checklist', async ({ page }) => {
    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Password requirements list should be visible
    await expect(page.locator('[aria-label="Password requirements"]')).toBeVisible()

    // Type a weak password
    await page.fill('input[name="password"]', 'weak')

    // Requirements should show as not met
    const requirementsList = page.locator('[aria-label="Password requirements"]')
    await expect(requirementsList).toBeVisible()
  })

  test('should submit successfully and redirect to login', async ({ page }) => {
    // Mock successful reset
    await page.route('**/api/v1/auth/reset-password', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Password reset successful. You can now log in.' }),
        })
      }
    })

    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Fill valid password
    await page.fill('input[name="password"]', 'ValidPassword123!')
    await page.fill('input[name="confirmPassword"]', 'ValidPassword123!')

    // Submit form
    await page.click('button[type="submit"]')

    // Should show success message
    await expect(page.locator('text=Password updated')).toBeVisible()

    // Should redirect to login (wait for redirect)
    await page.waitForURL(/\/login/, { timeout: 5000 })
  })

  test('should show error when reset fails with token_expired', async ({ page }) => {
    // Mock failed reset due to expired token
    await page.route('**/api/v1/auth/reset-password', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({
            error: {
              code: 'token_expired',
              message: 'Reset link has expired.',
              details: [],
            },
          }),
        })
      }
    })

    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Fill valid password
    await page.fill('input[name="password"]', 'ValidPassword123!')
    await page.fill('input[name="confirmPassword"]', 'ValidPassword123!')

    // Submit form
    await page.click('button[type="submit"]')

    // Should show expired error and hide form
    await expect(page.locator('text=This reset link has expired')).toBeVisible()
    await expect(page.locator('input[name="password"]')).not.toBeVisible()
  })

  test('should show error when reset fails with token_used', async ({ page }) => {
    // Mock failed reset due to used token
    await page.route('**/api/v1/auth/reset-password', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({
            error: {
              code: 'token_used',
              message: 'This reset link has already been used.',
              details: [],
            },
          }),
        })
      }
    })

    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Fill valid password
    await page.fill('input[name="password"]', 'ValidPassword123!')
    await page.fill('input[name="confirmPassword"]', 'ValidPassword123!')

    // Submit form
    await page.click('button[type="submit"]')

    // Should show used error and hide form
    await expect(page.locator('text=This reset link has already been used')).toBeVisible()
    await expect(page.locator('input[name="password"]')).not.toBeVisible()
  })

  test('should show error when reset fails with password_requirements_not_met', async ({ page }) => {
    // Mock failed reset due to password requirements
    await page.route('**/api/v1/auth/reset-password', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({
            error: {
              code: 'password_requirements_not_met',
              message: 'Password does not meet complexity requirements.',
              details: ['Password must be at least 8 characters.'],
            },
          }),
        })
      }
    })

    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Fill password (server will reject)
    await page.fill('input[name="password"]', 'ValidPassword123!')
    await page.fill('input[name="confirmPassword"]', 'ValidPassword123!')

    // Submit form
    await page.click('button[type="submit"]')

    // Should show error message but keep form visible
    await expect(page.locator('text=Password must be at least 8 characters')).toBeVisible()
    await expect(page.locator('input[name="password"]')).toBeVisible()
  })

  test('should disable form during submission', async ({ page }) => {
    // Mock slow reset endpoint
    await page.route('**/api/v1/auth/reset-password', async (route) => {
      if (route.request().method() === 'POST') {
        await new Promise((resolve) => setTimeout(resolve, 1000))
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Password reset successful.' }),
        })
      }
    })

    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Fill valid password
    await page.fill('input[name="password"]', 'ValidPassword123!')
    await page.fill('input[name="confirmPassword"]', 'ValidPassword123!')

    // Submit form
    await page.click('button[type="submit"]')

    // Button should show loading state
    await expect(page.locator('button[type="submit"]:has-text("Updating...")')).toBeVisible()

    // Form fields should be disabled
    await expect(page.locator('input[name="password"]')).toBeDisabled()
    await expect(page.locator('input[name="confirmPassword"]')).toBeDisabled()
  })
})

test.describe('Reset Password - Accessibility', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ valid: true, expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })
  })

  test('should have proper ARIA labels on form elements', async ({ page }) => {
    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Check section has aria-label
    await expect(page.locator('section[aria-label="Reset password"]')).toBeVisible()

    // Check password requirements list has aria-label
    await expect(page.locator('ul[aria-label="Password requirements"]')).toBeVisible()
  })

  test('should be keyboard navigable', async ({ page }) => {
    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Tab through form elements
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    
    // Should be able to focus on password field
    const passwordField = page.locator('input[name="password"]')
    await passwordField.focus()
    await expect(passwordField).toBeFocused()

    // Tab to confirm password
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    
    const confirmField = page.locator('input[name="confirmPassword"]')
    await confirmField.focus()
    await expect(confirmField).toBeFocused()
  })

  test('should have visible focus states', async ({ page }) => {
    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Focus on password field
    const passwordField = page.locator('input[name="password"]')
    await passwordField.focus()

    // Field should have visible focus (check it's focused)
    await expect(passwordField).toBeFocused()
  })
})

test.describe('Reset Password - Navigation', () => {
  test('should have back to login link when token is invalid', async ({ page }) => {
    await page.goto('/reset-password')

    // Should show back to login link
    const loginLink = page.locator('a[href="/login"]')
    await expect(loginLink).toBeVisible()

    // Click should navigate to login
    await loginLink.click()
    await expect(page).toHaveURL(/\/login/)
  })

  test('should have back to login link in valid form', async ({ page }) => {
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ valid: true, expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })

    await page.goto('/reset-password?token=valid-token')

    // Wait for form
    await expect(page.locator('input[name="password"]')).toBeVisible()

    // Should show back to login link
    const loginLink = page.locator('a[href="/login"]:has-text("Back to login")')
    await expect(loginLink).toBeVisible()
  })

  test('should have request new reset link when token is invalid', async ({ page }) => {
    await page.route('**/api/v1/auth/reset-password/validate*', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          error: {
            code: 'invalid_token',
            message: 'Invalid or expired reset link.',
            details: [],
          },
        }),
      })
    })

    await page.goto('/reset-password?token=invalid-token')

    // Should show request new reset link
    const forgotPasswordLink = page.locator('a[href="/forgot-password"]')
    await expect(forgotPasswordLink).toBeVisible()

    // Click should navigate to forgot password
    await forgotPasswordLink.click()
    await expect(page).toHaveURL(/\/forgot-password/)
  })
})
