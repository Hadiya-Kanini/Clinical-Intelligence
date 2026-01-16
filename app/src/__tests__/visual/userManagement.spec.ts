import { test, expect } from '@playwright/test'

/**
 * E2E tests for User Management page (SCR-014).
 * Tests cover:
 * - Basic rendering of user list from backend
 * - Search functionality
 * - Sorting functionality
 * - Pagination functionality
 * - Access control (admin-only)
 */

const mockUsersPage1 = {
  items: [
    { id: '1', name: 'Alice Admin', email: 'alice@example.com', role: 'admin', status: 'active' },
    { id: '2', name: 'Bob Standard', email: 'bob@example.com', role: 'standard', status: 'active' },
    { id: '3', name: 'Charlie User', email: 'charlie@example.com', role: 'standard', status: 'inactive' },
  ],
  page: 1,
  pageSize: 20,
  total: 25,
}

const mockUsersPage2 = {
  items: [
    { id: '4', name: 'David Developer', email: 'david@example.com', role: 'standard', status: 'active' },
    { id: '5', name: 'Eve Engineer', email: 'eve@example.com', role: 'admin', status: 'active' },
  ],
  page: 2,
  pageSize: 20,
  total: 25,
}

const mockSearchResults = {
  items: [
    { id: '1', name: 'Alice Admin', email: 'alice@example.com', role: 'admin', status: 'active' },
  ],
  page: 1,
  pageSize: 20,
  total: 1,
}

const mockSortedByNameDesc = {
  items: [
    { id: '3', name: 'Charlie User', email: 'charlie@example.com', role: 'standard', status: 'inactive' },
    { id: '2', name: 'Bob Standard', email: 'bob@example.com', role: 'standard', status: 'active' },
    { id: '1', name: 'Alice Admin', email: 'alice@example.com', role: 'admin', status: 'active' },
  ],
  page: 1,
  pageSize: 20,
  total: 3,
}

test.describe('User Management - Basic Rendering', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: '1', email: 'admin@example.com', role: 'admin' }),
      })
    })

    await page.route('**/api/v1/auth/csrf', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'mock-csrf-token', expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })
  })

  test('should render user list from backend', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=User management')).toBeVisible()

    await expect(page.locator('text=Alice Admin')).toBeVisible()
    await expect(page.locator('text=alice@example.com')).toBeVisible()
    await expect(page.locator('text=Bob Standard')).toBeVisible()
    await expect(page.locator('text=Charlie User')).toBeVisible()
  })

  test('should show loading state initially', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 500))
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=Loading users')).toBeVisible()
  })

  test('should show empty state when no users', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: [], page: 1, pageSize: 20, total: 0 }),
      })
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=No users found')).toBeVisible()
  })

  test('should show error state on API failure', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({
          error: { code: 'internal_error', message: 'Server error', details: [] },
        }),
      })
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=Server error')).toBeVisible()
  })

  test('should show access denied for non-admin', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 403,
        contentType: 'application/json',
        body: JSON.stringify({
          error: { code: 'forbidden', message: 'Admin access required.', details: [] },
        }),
      })
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=Access denied')).toBeVisible()
  })
})

test.describe('User Management - Search', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: '1', email: 'admin@example.com', role: 'admin' }),
      })
    })

    await page.route('**/api/v1/auth/csrf', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'mock-csrf-token', expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })
  })

  test('should filter results when searching', async ({ page }) => {
    let requestCount = 0
    await page.route('**/api/v1/admin/users*', async (route) => {
      const url = route.request().url()
      requestCount++

      if (url.includes('q=alice')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockSearchResults),
        })
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      }
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=Alice Admin')).toBeVisible()
    await expect(page.locator('text=Bob Standard')).toBeVisible()

    const searchInput = page.locator('input[aria-label="Search users"]')
    await searchInput.fill('alice')

    await page.waitForTimeout(400)

    await expect(page.locator('text=Alice Admin')).toBeVisible()
    await expect(page.locator('text=Bob Standard')).not.toBeVisible()
  })

  test('should have accessible search input', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    const searchInput = page.locator('input[aria-label="Search users"]')
    await expect(searchInput).toBeVisible()
    await expect(searchInput).toHaveAttribute('placeholder', 'Search by name or email')
  })
})

test.describe('User Management - Sorting', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: '1', email: 'admin@example.com', role: 'admin' }),
      })
    })

    await page.route('**/api/v1/auth/csrf', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'mock-csrf-token', expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })
  })

  test('should sort by column when header is clicked', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      const url = route.request().url()

      if (url.includes('sortDir=desc')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockSortedByNameDesc),
        })
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      }
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=Alice Admin')).toBeVisible()

    const userHeader = page.locator('th:has-text("User")')
    await userHeader.click()

    await page.waitForTimeout(100)

    const rows = page.locator('tbody tr')
    const firstRowName = rows.first().locator('td').first()
    await expect(firstRowName).toContainText('Charlie User')
  })

  test('should show sort indicator on sorted column', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    const userHeader = page.locator('th:has-text("User")')
    await expect(userHeader).toContainText('▲')
  })

  test('should toggle sort direction on repeated click', async ({ page }) => {
    let sortDir = 'asc'
    await page.route('**/api/v1/admin/users*', async (route) => {
      const url = route.request().url()
      if (url.includes('sortDir=desc')) {
        sortDir = 'desc'
      } else {
        sortDir = 'asc'
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(sortDir === 'desc' ? mockSortedByNameDesc : mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    const userHeader = page.locator('th:has-text("User")')

    await userHeader.click()
    await page.waitForTimeout(100)

    await expect(userHeader).toContainText('▼')
  })

  test('should support keyboard navigation for sort headers', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    const userHeader = page.locator('th:has-text("User")')
    await userHeader.focus()
    await page.keyboard.press('Enter')

    await page.waitForTimeout(100)

    await expect(userHeader).toContainText('▼')
  })
})

test.describe('User Management - Pagination', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: '1', email: 'admin@example.com', role: 'admin' }),
      })
    })

    await page.route('**/api/v1/auth/csrf', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'mock-csrf-token', expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })
  })

  test('should show pagination controls', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    await expect(page.locator('button:has-text("Previous")')).toBeVisible()
    await expect(page.locator('button:has-text("Next")')).toBeVisible()
    await expect(page.locator('text=Page 1 of')).toBeVisible()
  })

  test('should disable Previous button on first page', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    const prevButton = page.locator('button:has-text("Previous")')
    await expect(prevButton).toBeDisabled()
  })

  test('should navigate to next page', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      const url = route.request().url()

      if (url.includes('page=2')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage2),
        })
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      }
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=Alice Admin')).toBeVisible()

    const nextButton = page.locator('button:has-text("Next")')
    await nextButton.click()

    await expect(page.locator('text=David Developer')).toBeVisible()
    await expect(page.locator('text=Alice Admin')).not.toBeVisible()
  })

  test('should show correct pagination info', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    await expect(page.locator('text=Showing 1–')).toBeVisible()
    await expect(page.locator('text=of 25 users')).toBeVisible()
  })

  test('should have accessible pagination buttons', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    const prevButton = page.locator('button[aria-label="Previous page"]')
    const nextButton = page.locator('button[aria-label="Next page"]')

    await expect(prevButton).toBeVisible()
    await expect(nextButton).toBeVisible()
  })
})

test.describe('User Management - Edit User (US_041)', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: '1', email: 'admin@example.com', role: 'admin' }),
      })
    })

    await page.route('**/api/v1/auth/csrf', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'mock-csrf-token', expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })
  })

  test('should open edit modal with user data', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      }
    })

    await page.goto('/admin/users')

    const editButton = page.locator('tr:has-text("Alice Admin") button:has-text("Edit")')
    await editButton.click()

    await expect(page.locator('text=Edit user')).toBeVisible()
    await expect(page.locator('input[value="Alice Admin"]')).toBeVisible()
  })

  test('should update user and show success message', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      const method = route.request().method()
      
      if (method === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      } else if (method === 'PUT') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: '1',
            name: 'Alice Updated',
            email: 'alice@example.com',
            role: 'admin',
            status: 'active',
          }),
        })
      }
    })

    await page.goto('/admin/users')

    const editButton = page.locator('tr:has-text("Alice Admin") button:has-text("Edit")')
    await editButton.click()

    const nameInput = page.locator('input[value="Alice Admin"]')
    await nameInput.fill('Alice Updated')

    const saveButton = page.locator('button:has-text("Save changes")')
    await saveButton.click()

    await expect(page.locator('text=User updated successfully')).toBeVisible()
  })

  test('should show error message on update failure', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      const method = route.request().method()
      
      if (method === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      } else if (method === 'PUT') {
        await route.fulfill({
          status: 409,
          contentType: 'application/json',
          body: JSON.stringify({
            error: { code: 'duplicate_email', message: 'A user with this email already exists.', details: [] },
          }),
        })
      }
    })

    await page.goto('/admin/users')

    const editButton = page.locator('tr:has-text("Alice Admin") button:has-text("Edit")')
    await editButton.click()

    const saveButton = page.locator('button:has-text("Save changes")')
    await saveButton.click()

    await expect(page.locator('text=A user with this email already exists')).toBeVisible()
  })
})

test.describe('User Management - Toggle Status (US_041)', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: '1', email: 'admin@example.com', role: 'admin' }),
      })
    })

    await page.route('**/api/v1/auth/csrf', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'mock-csrf-token', expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })
  })

  test('should deactivate user and show success message', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      const method = route.request().method()
      const url = route.request().url()
      
      if (method === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      } else if (method === 'PATCH' && url.includes('toggle-status')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: '2',
            name: 'Bob Standard',
            email: 'bob@example.com',
            role: 'standard',
            status: 'inactive',
          }),
        })
      }
    })

    await page.goto('/admin/users')

    const deactivateButton = page.locator('tr:has-text("Bob Standard") button:has-text("Deactivate")')
    await deactivateButton.click()

    await expect(page.locator('text=User deactivated successfully')).toBeVisible()
  })

  test('should activate user and show success message', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      const method = route.request().method()
      const url = route.request().url()
      
      if (method === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      } else if (method === 'PATCH' && url.includes('toggle-status')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: '3',
            name: 'Charlie User',
            email: 'charlie@example.com',
            role: 'standard',
            status: 'active',
          }),
        })
      }
    })

    await page.goto('/admin/users')

    const activateButton = page.locator('tr:has-text("Charlie User") button:has-text("Activate")')
    await activateButton.click()

    await expect(page.locator('text=User activated successfully')).toBeVisible()
  })

  test('should show error for self-deactivation attempt', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      const method = route.request().method()
      const url = route.request().url()
      
      if (method === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      } else if (method === 'PATCH' && url.includes('toggle-status')) {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({
            error: { code: 'invalid_input', message: 'You cannot change your own account status.', details: ['userId:self_status_change'] },
          }),
        })
      }
    })

    await page.goto('/admin/users')

    const deactivateButton = page.locator('tr:has-text("Alice Admin") button:has-text("Deactivate")')
    await deactivateButton.click()

    await expect(page.locator('text=You cannot change your own account status')).toBeVisible()
  })

  test('should show error for static admin deactivation attempt', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      const method = route.request().method()
      const url = route.request().url()
      
      if (method === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(mockUsersPage1),
        })
      } else if (method === 'PATCH' && url.includes('toggle-status')) {
        await route.fulfill({
          status: 403,
          contentType: 'application/json',
          body: JSON.stringify({
            error: { code: 'static_admin_protected', message: 'The static admin account cannot be deactivated.', details: [] },
          }),
        })
      }
    })

    await page.goto('/admin/users')

    const deactivateButton = page.locator('tr:has-text("Alice Admin") button:has-text("Deactivate")')
    await deactivateButton.click()

    await expect(page.locator('text=static admin account cannot be deactivated')).toBeVisible()
  })

  test('should display deactivated status label correctly', async ({ page }) => {
    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })

    await page.goto('/admin/users')

    // Charlie User has status 'inactive' which should display as 'deactivated'
    const charlieRow = page.locator('tr:has-text("Charlie User")')
    await expect(charlieRow.locator('text=deactivated')).toBeVisible()
  })
})

test.describe('User Management - Accessibility', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: '1', email: 'admin@example.com', role: 'admin' }),
      })
    })

    await page.route('**/api/v1/auth/csrf', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'mock-csrf-token', expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })

    await page.route('**/api/v1/admin/users*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockUsersPage1),
      })
    })
  })

  test('should have proper ARIA attributes on sortable headers', async ({ page }) => {
    await page.goto('/admin/users')

    const userHeader = page.locator('th[role="columnheader"]:has-text("User")')
    await expect(userHeader).toHaveAttribute('aria-sort', 'ascending')
  })

  test('should be keyboard navigable', async ({ page }) => {
    await page.goto('/admin/users')

    const searchInput = page.locator('input[aria-label="Search users"]')
    await searchInput.focus()
    await expect(searchInput).toBeFocused()

    await page.keyboard.press('Tab')

    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
  })
})
