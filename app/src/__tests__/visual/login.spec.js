import { test, expect } from '@playwright/test'

test.describe('Login Page Visual Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/login', async (route) => {
      const request = route.request()

      if (request.method() !== 'POST') {
        await route.fallback()
        return
      }

      const body = (request.postData() || '').toLowerCase()

      if (body.includes('"password":"locked"')) {
        await route.fulfill({
          status: 403,
          contentType: 'application/json',
          body: JSON.stringify({
            error: { code: 'account_locked', message: 'Account temporarily locked.', details: [] },
          }),
        })
        return
      }

      if (body.includes('"password":"ratelimited"')) {
        await route.fulfill({
          status: 429,
          contentType: 'application/json',
          body: JSON.stringify({
            error: { code: 'rate_limited', message: 'Too many requests.', details: [] },
          }),
        })
        return
      }

      if (body.includes('"password":"slow"')) {
        await new Promise((resolve) => setTimeout(resolve, 800))
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ token: 'test-token', expires_in: 900 }),
        })
        return
      }

      if (!body.includes('"password":"password"')) {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({
            error: { code: 'invalid_credentials', message: 'Invalid email or password.', details: [] },
          }),
        })
        return
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'test-token', expires_in: 900 }),
      })
    })

    await page.route('**/api/v1/auth/logout', async (route) => {
      const request = route.request()

      if (request.method() !== 'POST') {
        await route.fallback()
        return
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ status: 'logged_out' }),
      })
    })

    await page.goto('http://localhost:5173/login')
  })

  test('desktop layout renders correctly', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 })
    
    // Wait for page to load
    await page.waitForSelector('text=Clinical Intelligence')
    
    // Take screenshot for visual comparison
    await expect(page).toHaveScreenshot('login-desktop.png')
  })

  test('tablet layout renders correctly', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 })
    
    // Wait for page to load
    await page.waitForSelector('text=Clinical Intelligence')
    
    // Take screenshot for visual comparison
    await expect(page).toHaveScreenshot('login-tablet.png')
  })

  test('mobile layout renders correctly', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 })
    
    // Wait for page to load
    await page.waitForSelector('text=Clinical Intelligence')
    
    // Take screenshot for visual comparison
    await expect(page).toHaveScreenshot('login-mobile.png')
  })

  test('form elements are properly aligned', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 })
    
    // Check form elements exist
    await expect(page.locator('input[type="email"]')).toBeVisible()
    await expect(page.locator('input[type="password"]')).toBeVisible()
    await expect(page.locator('a:has-text("Forgot Password")')).toBeVisible()
    await expect(page.locator('button:has-text("Log in")')).toBeVisible()
    
    // Check branding elements
    await expect(page.locator('text=Clinical Intelligence')).toBeVisible()
    await expect(page.locator('text=Log in')).toBeVisible()
    await expect(page.locator('text=Sign in with your work email')).toBeVisible()
  })

  test('responsive behavior at different zoom levels', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 })
    
    // Test zoom 150%
    await page.evaluate(() => {
      document.body.style.zoom = '1.5'
    })
    await page.waitForSelector('text=Clinical Intelligence')
    await expect(page).toHaveScreenshot('login-zoom-150.png')
    
    // Reset zoom
    await page.evaluate(() => {
      document.body.style.zoom = '1'
    })
  })

  test('keyboard navigation works', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 })
    
    // Tab through form elements
    await page.keyboard.press('Tab')
    await expect(page.locator('input[type="email"]')).toBeFocused()
    
    await page.keyboard.press('Tab')
    await expect(page.locator('input[type="password"]')).toBeFocused()
    
    await page.keyboard.press('Tab')
    await expect(page.locator('a:has-text("Forgot Password")')).toBeFocused()

    await page.keyboard.press('Tab')
    await expect(page.locator('button:has-text("Log in")')).toBeFocused()
  })

  test('form submission is handled correctly', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 })
    
    // Setup console listener
    const messages = []
    page.on('console', msg => messages.push(msg.text()))

    await page.fill('input[type="email"]', 'name@hospital.org')
    await page.fill('input[type="password"]', 'password')
    
    // Submit form
    await page.click('button:has-text("Log in")')
    
    // Check console message
    await page.waitForTimeout(100)
    expect(messages).toContain('Login successful')
  })

  test('shows loading state and blocks duplicate submits during slow authentication', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 })

    await page.fill('input[type="email"]', 'name@hospital.org')
    await page.fill('input[type="password"]', 'slow')

    const submitButton = page.locator('button:has-text("Log in")')
    await submitButton.click()

    await expect(submitButton).toBeDisabled()
    await expect(submitButton).toHaveAttribute('aria-busy', 'true')

    await submitButton.click({ force: true })
    await expect(submitButton).toBeDisabled()
  })

  test('validation errors appear on submit and do not break layout', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 })

    await page.click('button:has-text("Log in")')

    await expect(page.locator('text=Please correct the highlighted fields and try again.')).toBeVisible()
    await expect(page.locator('text=Email is required.')).toBeVisible()
    await expect(page.locator('text=Password is required.')).toBeVisible()
    await expect(page).toHaveScreenshot('login-validation-mobile.png')
  })

  test('auth error state renders without breaking layout (mobile)', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 })

    await page.fill('input[type="email"]', 'name@hospital.org')
    await page.fill('input[type="password"]', 'wrong')
    await page.click('button:has-text("Log in")')

    await expect(page.locator('text=Incorrect email or password. Please try again.')).toBeVisible()
    await expect(page).toHaveScreenshot('login-auth-error-mobile.png')
  })

  test('logout redirects to login and back does not reveal dashboard', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 })

    await page.fill('input[type="email"]', 'name@hospital.org')
    await page.fill('input[type="password"]', 'password')
    await page.click('button:has-text("Log in")')

    await page.waitForURL('**/dashboard')
    await expect(page.locator('text=Dashboard')).toBeVisible()
    await expect(page.locator('button:has-text("Log out")')).toBeVisible()

    await page.click('button:has-text("Log out")')
    await page.waitForURL('**/login')
    await expect(page.locator('text=You have been logged out.')).toBeVisible()

    await page.goBack()
    await page.waitForURL('**/login')
    await expect(page.locator('text=Dashboard')).not.toBeVisible()
  })

  test('dashboard is protected when unauthenticated', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 800 })

    await page.evaluate(() => {
      try {
        window.localStorage.removeItem('ci_auth')
      } catch {
      }
    })

    await page.goto('http://localhost:5173/dashboard')
    await page.waitForURL('**/login')
    await expect(page.locator('text=Log in')).toBeVisible()
  })
})
