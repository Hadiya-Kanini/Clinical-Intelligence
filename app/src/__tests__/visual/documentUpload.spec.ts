import { test, expect } from '@playwright/test'

/**
 * E2E tests for Document Upload page (SCR-005).
 * Tests cover:
 * - Drag-over highlight behavior (UXR-019)
 * - Drop queueing behavior
 * - Multi-select file picker behavior (UXR-022)
 * - File count and names display
 */

test.describe('Document Upload - SCR-005', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: '1', email: 'user@example.com', role: 'standard' }),
      })
    })

    await page.route('**/api/v1/auth/csrf', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ token: 'mock-csrf-token', expiresAt: new Date(Date.now() + 3600000).toISOString() }),
      })
    })

    await page.goto('/documents/upload')
    await page.waitForSelector('[data-testid="drop-zone"]')
  })

  test.describe('Drop Zone Rendering', () => {
    test('should render drop zone with correct initial state', async ({ page }) => {
      const dropZone = page.getByTestId('drop-zone')
      await expect(dropZone).toBeVisible()

      const dropZoneText = page.getByTestId('drop-zone-text')
      await expect(dropZoneText).toHaveText('Drag & drop files here')
    })

    test('should display file picker button', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')
      await expect(fileInput).toBeAttached()
      await expect(fileInput).toHaveAttribute('multiple', '')
      await expect(fileInput).toHaveAttribute('accept', '.pdf,.docx')
    })

    test('should show 0 selected initially', async ({ page }) => {
      await expect(page.getByText('0 selected')).toBeVisible()
    })
  })

  test.describe('Drag-Over Highlight Behavior (UXR-019)', () => {
    test('should highlight drop zone on drag enter', async ({ page }) => {
      const dropZone = page.getByTestId('drop-zone')

      const dataTransfer = await page.evaluateHandle(() => new DataTransfer())

      await dropZone.dispatchEvent('dragenter', { dataTransfer })

      const dropZoneText = page.getByTestId('drop-zone-text')
      await expect(dropZoneText).toHaveText('Drop files here')
    })

    test('should remove highlight on drag leave', async ({ page }) => {
      const dropZone = page.getByTestId('drop-zone')

      const dataTransfer = await page.evaluateHandle(() => new DataTransfer())

      await dropZone.dispatchEvent('dragenter', { dataTransfer })
      await dropZone.dispatchEvent('dragleave', { dataTransfer })

      const dropZoneText = page.getByTestId('drop-zone-text')
      await expect(dropZoneText).toHaveText('Drag & drop files here')
    })
  })

  test.describe('File Picker Multi-Select (UXR-022)', () => {
    test('should queue multiple files via file picker', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')

      await fileInput.setInputFiles([
        { name: 'document1.pdf', mimeType: 'application/pdf', buffer: Buffer.from('PDF content 1') },
        { name: 'document2.docx', mimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', buffer: Buffer.from('DOCX content 2') },
      ])

      await expect(page.getByText('2 selected')).toBeVisible()
      await expect(page.getByText('document1.pdf')).toBeVisible()
      await expect(page.getByText('document2.docx')).toBeVisible()
    })

    test('should append files to existing queue', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')

      await fileInput.setInputFiles([
        { name: 'first.pdf', mimeType: 'application/pdf', buffer: Buffer.from('PDF 1') },
      ])

      await expect(page.getByText('1 selected')).toBeVisible()

      await fileInput.setInputFiles([
        { name: 'second.pdf', mimeType: 'application/pdf', buffer: Buffer.from('PDF 2') },
      ])

      await expect(page.getByText('2 selected')).toBeVisible()
      await expect(page.getByText('first.pdf')).toBeVisible()
      await expect(page.getByText('second.pdf')).toBeVisible()
    })
  })

  test.describe('Drop Queueing Behavior', () => {
    test('should queue files on drop and display count + names', async ({ page }) => {
      const dropZone = page.getByTestId('drop-zone')

      const dataTransfer = await page.evaluateHandle(() => {
        const dt = new DataTransfer()
        dt.items.add(new File(['content1'], 'dropped1.pdf', { type: 'application/pdf' }))
        dt.items.add(new File(['content2'], 'dropped2.docx', { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' }))
        return dt
      })

      await dropZone.dispatchEvent('drop', { dataTransfer })

      await expect(page.getByText('2 selected')).toBeVisible()
      await expect(page.getByText('dropped1.pdf')).toBeVisible()
      await expect(page.getByText('dropped2.docx')).toBeVisible()
    })

    test('should reset highlight after drop', async ({ page }) => {
      const dropZone = page.getByTestId('drop-zone')

      const dataTransfer = await page.evaluateHandle(() => {
        const dt = new DataTransfer()
        dt.items.add(new File(['content'], 'test.pdf', { type: 'application/pdf' }))
        return dt
      })

      await dropZone.dispatchEvent('dragenter', { dataTransfer })
      await dropZone.dispatchEvent('drop', { dataTransfer })

      const dropZoneText = page.getByTestId('drop-zone-text')
      await expect(dropZoneText).toHaveText('Drag & drop files here')
    })
  })

  test.describe('File Queue Display', () => {
    test('should display file names with truncation for long names', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')

      const longFileName = 'this_is_a_very_long_filename_that_should_be_truncated_in_the_ui_display.pdf'

      await fileInput.setInputFiles([
        { name: longFileName, mimeType: 'application/pdf', buffer: Buffer.from('PDF content') },
      ])

      const fileNameElement = page.getByTestId('file-name')
      await expect(fileNameElement).toHaveAttribute('title', longFileName)
    })

    test('should allow removing files from queue', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')

      await fileInput.setInputFiles([
        { name: 'removable.pdf', mimeType: 'application/pdf', buffer: Buffer.from('PDF') },
      ])

      await expect(page.getByText('1 selected')).toBeVisible()

      const removeButton = page.getByTestId('remove-file-btn')
      await removeButton.click()

      await expect(page.getByText('0 selected')).toBeVisible()
      await expect(page.getByText('removable.pdf')).not.toBeVisible()
    })

    test('should clear all files when Clear button is clicked', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')

      await fileInput.setInputFiles([
        { name: 'file1.pdf', mimeType: 'application/pdf', buffer: Buffer.from('PDF 1') },
        { name: 'file2.pdf', mimeType: 'application/pdf', buffer: Buffer.from('PDF 2') },
      ])

      await expect(page.getByText('2 selected')).toBeVisible()

      await page.getByRole('button', { name: 'Clear' }).click()

      await expect(page.getByText('0 selected')).toBeVisible()
    })
  })

  test.describe('File Validation', () => {
    test('should reject unsupported file types', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')

      await fileInput.setInputFiles([
        { name: 'image.png', mimeType: 'image/png', buffer: Buffer.from('PNG content') },
      ])

      await expect(page.getByText('unsupported file type')).toBeVisible()
      await expect(page.getByText('0 selected')).toBeVisible()
    })

    test('should accept valid PDF and DOCX files', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')

      await fileInput.setInputFiles([
        { name: 'valid.pdf', mimeType: 'application/pdf', buffer: Buffer.from('PDF') },
        { name: 'valid.docx', mimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', buffer: Buffer.from('DOCX') },
      ])

      await expect(page.getByText('2 selected')).toBeVisible()
    })
  })

  test.describe('Upload Button State', () => {
    test('should disable Upload button when no files are queued', async ({ page }) => {
      const uploadButton = page.getByRole('button', { name: 'Upload' })
      await expect(uploadButton).toBeDisabled()
    })

    test('should enable Upload button when files are queued', async ({ page }) => {
      const fileInput = page.getByTestId('file-input')

      await fileInput.setInputFiles([
        { name: 'test.pdf', mimeType: 'application/pdf', buffer: Buffer.from('PDF') },
      ])

      const uploadButton = page.getByRole('button', { name: 'Upload' })
      await expect(uploadButton).toBeEnabled()
    })
  })
})
