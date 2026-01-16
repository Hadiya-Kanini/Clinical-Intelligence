import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import DocumentUploadPage from '../pages/DocumentUploadPage'

/**
 * Unit/Integration tests for DocumentUploadPage upload progress behavior (US_043)
 * Tests cover:
 * - Progress bar percentage display
 * - Cancel button functionality
 * - State transitions (queued → uploading → success/error/cancelled)
 * - Retry flow after cancel/error
 * - Edge cases (small files, multiple files)
 * - Accessibility (ARIA attributes)
 */

function createTestStore() {
  return configureStore({
    reducer: {
      auth: () => ({
        isAuthenticated: true,
        user: { id: '1', email: 'test@example.com', role: 'standard' },
        isLoading: false,
      }),
      ui: () => ({}),
    },
  })
}

function renderWithProviders(ui: React.ReactElement) {
  const store = createTestStore()
  return render(
    <Provider store={store}>
      <MemoryRouter>
        {ui}
      </MemoryRouter>
    </Provider>
  )
}

function createMockFile(name: string, size: number = 1024, type: string = 'application/pdf'): File {
  const content = new Array(size).fill('a').join('')
  return new File([content], name, { type })
}

describe('DocumentUploadPage - Upload Progress Behavior', () => {
  beforeEach(() => {
    vi.useFakeTimers({ shouldAdvanceTime: true })
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  describe('Progress Display', () => {
    it('should display progress bar with percentage during upload', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(150)
      })

      await waitFor(() => {
        const progressBar = screen.getByTestId('upload-progress-bar')
        expect(progressBar).toBeInTheDocument()
        expect(progressBar).toHaveAttribute('role', 'progressbar')
      })
    })

    it('should show percentage text during uploading status', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(150)
      })

      await waitFor(() => {
        const percentage = screen.queryByTestId('progress-percentage')
        expect(percentage).toBeInTheDocument()
      })
    })

    it('should update progress bar ARIA attributes correctly', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(150)
      })

      await waitFor(() => {
        const progressBar = screen.getByTestId('upload-progress-bar')
        expect(progressBar).toHaveAttribute('aria-valuemin', '0')
        expect(progressBar).toHaveAttribute('aria-valuemax', '100')
        expect(progressBar).toHaveAttribute('aria-valuenow')
      })
    })

    it('should show each file with independent progress tracking', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const files = [
        createMockFile('doc1.pdf', 1024 * 1024),
        createMockFile('doc2.pdf', 1024 * 1024),
      ]

      await act(async () => {
        fireEvent.change(fileInput, { target: { files } })
      })

      await waitFor(() => {
        const progressBars = screen.getAllByTestId('upload-progress-bar')
        expect(progressBars).toHaveLength(2)
      })
    })
  })

  describe('Cancel Functionality', () => {
    it('should show Cancel button only during uploading status', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      expect(screen.queryByTestId('cancel-upload-btn')).not.toBeInTheDocument()

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(50)
      })

      await waitFor(() => {
        expect(screen.getByTestId('cancel-upload-btn')).toBeInTheDocument()
      })
    })

    it('should stop upload when Cancel is clicked', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(150)
      })

      await waitFor(() => {
        expect(screen.getByTestId('cancel-upload-btn')).toBeInTheDocument()
      })

      const cancelButton = screen.getByTestId('cancel-upload-btn')
      await act(async () => {
        await user.click(cancelButton)
      })

      await waitFor(() => {
        expect(screen.queryByTestId('cancel-upload-btn')).not.toBeInTheDocument()
        expect(screen.getByText('Cancelled')).toBeInTheDocument()
      })
    })

    it('should show cancelled status after cancel', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(150)
      })

      const cancelButton = await screen.findByTestId('cancel-upload-btn')
      await act(async () => {
        await user.click(cancelButton)
      })

      await waitFor(() => {
        const progressBar = screen.getByTestId('upload-progress-bar')
        expect(progressBar).toHaveClass('ui-progress--cancelled')
      })
    })

    it('should allow retry after cancel', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(150)
      })

      const cancelButton = await screen.findByTestId('cancel-upload-btn')
      await act(async () => {
        await user.click(cancelButton)
      })

      await waitFor(() => {
        expect(screen.getByTestId('retry-upload-btn')).toBeInTheDocument()
      })

      const retryButton = screen.getByTestId('retry-upload-btn')
      await act(async () => {
        await user.click(retryButton)
      })

      await waitFor(() => {
        expect(screen.getByText('Ready')).toBeInTheDocument()
      })
    })
  })

  describe('State Transitions', () => {
    it('should transition from queued to uploading to success', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 50 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      expect(screen.getByText('Ready')).toBeInTheDocument()

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(50)
      })

      await waitFor(() => {
        expect(screen.getByText('Uploading')).toBeInTheDocument()
      })

      await act(async () => {
        vi.advanceTimersByTime(2000)
      })

      await waitFor(() => {
        expect(screen.getByText('Uploaded')).toBeInTheDocument()
      })
    })

    it('should show success variant styling after upload completes', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 50 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(2000)
      })

      await waitFor(() => {
        const progressBar = screen.getByTestId('upload-progress-bar')
        expect(progressBar).toHaveClass('ui-progress--success')
      })
    })
  })

  describe('Edge Cases', () => {
    it('should handle very small files with quick upload', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const smallFile = createMockFile('tiny.pdf', 50 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [smallFile] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(500)
      })

      await waitFor(() => {
        expect(screen.getByText('Uploaded')).toBeInTheDocument()
      })
    })

    it('should handle multiple files uploading simultaneously in queue', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const files = [
        createMockFile('doc1.pdf', 1024 * 1024),
        createMockFile('doc2.pdf', 1024 * 1024),
        createMockFile('doc3.pdf', 1024 * 1024),
      ]

      await act(async () => {
        fireEvent.change(fileInput, { target: { files } })
      })

      await waitFor(() => {
        expect(screen.getByText('3 selected')).toBeInTheDocument()
        expect(screen.getAllByText('Ready')).toHaveLength(3)
      })
    })
  })

  describe('Accessibility', () => {
    it('should have accessible Cancel button with aria-label', async () => {
      const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      await act(async () => {
        await user.click(uploadButton)
      })

      await act(async () => {
        vi.advanceTimersByTime(150)
      })

      await waitFor(() => {
        const cancelButton = screen.getByTestId('cancel-upload-btn')
        expect(cancelButton).toHaveAttribute('aria-label', 'Cancel upload for test.pdf')
      })
    })

    it('should have aria-live region for progress updates', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      await waitFor(() => {
        const liveRegion = screen.getByTestId('upload-progress-bar').closest('[aria-live]')
        expect(liveRegion).toHaveAttribute('aria-live', 'polite')
      })
    })

    it('should have accessible Remove button with aria-label', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf', 1024 * 1024)

      await act(async () => {
        fireEvent.change(fileInput, { target: { files: [file] } })
      })

      await waitFor(() => {
        const removeButton = screen.getByTestId('remove-file-btn')
        expect(removeButton).toHaveAttribute('aria-label', 'Remove test.pdf')
      })
    })
  })
})
