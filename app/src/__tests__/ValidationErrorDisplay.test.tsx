import { describe, it, expect } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import DocumentUploadPage from '../pages/DocumentUploadPage'

function createTestStore() {
  return configureStore({
    reducer: {
      auth: () => ({ isAuthenticated: true, user: { id: '1', email: 'test@example.com', role: 'standard' }, isLoading: false }),
      ui: () => ({}),
    },
  })
}

function renderWithProviders(ui: React.ReactElement) {
  return render(<Provider store={createTestStore()}><MemoryRouter>{ui}</MemoryRouter></Provider>)
}

function createMockFile(name: string, size: number = 1024, type: string = 'application/pdf'): File {
  return new File([new Array(size).fill('a').join('')], name, { type })
}

describe('ValidationErrorDisplay', () => {
  describe('Error Message Specificity', () => {
    it('should show "Unsupported file type" for invalid file type', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('test.txt', 1024, 'text/plain')] } })
      await waitFor(() => {
        expect(screen.getByText('Unsupported file type')).toBeInTheDocument()
      })
    })

    it('should show "File exceeds 50MB limit" for oversized file', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('large.pdf', 51 * 1024 * 1024)] } })
      await waitFor(() => {
        expect(screen.getByText('File exceeds 50MB limit')).toBeInTheDocument()
      })
    })

    it('should show "File is empty" for empty file', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('empty.pdf', 0)] } })
      await waitFor(() => {
        expect(screen.getByText('File is empty')).toBeInTheDocument()
      })
    })
  })

  describe('Guidance Text Display', () => {
    it('should show guidance for invalid file type', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('test.txt', 1024, 'text/plain')] } })
      await waitFor(() => {
        expect(screen.getByText('Please upload PDF or DOCX files only')).toBeInTheDocument()
      })
    })

    it('should show guidance for oversized file', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('large.pdf', 51 * 1024 * 1024)] } })
      await waitFor(() => {
        expect(screen.getByText('Reduce file size or split into smaller documents')).toBeInTheDocument()
      })
    })
  })

  describe('Inline Error Rendering', () => {
    it('should show error inline next to specific file', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('invalid.txt', 1024, 'text/plain')] } })
      await waitFor(() => {
        expect(screen.getByTestId('inline-validation-error')).toBeInTheDocument()
      })
    })

    it('should show error icon for invalid files', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('invalid.txt', 1024, 'text/plain')] } })
      await waitFor(() => {
        const fileItem = screen.getByTestId('file-item')
        expect(fileItem).toHaveStyle({ background: 'var(--color-error-light)' })
      })
    })
  })

  describe('Partial Batch Upload', () => {
    it('should keep valid files when some fail validation', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      const files = [createMockFile('valid.pdf'), createMockFile('invalid.txt', 1024, 'text/plain')]
      fireEvent.change(fileInput, { target: { files } })
      await waitFor(() => {
        expect(screen.getByText('valid.pdf')).toBeInTheDocument()
        expect(screen.getByText('invalid.txt')).toBeInTheDocument()
      })
    })

    it('should allow removing invalid files', async () => {
      const user = userEvent.setup()
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('invalid.txt', 1024, 'text/plain')] } })
      await waitFor(() => { expect(screen.getByText('invalid.txt')).toBeInTheDocument() })
      const removeBtn = screen.getByTestId('remove-file-btn')
      await user.click(removeBtn)
      await waitFor(() => { expect(screen.queryByText('invalid.txt')).not.toBeInTheDocument() })
    })
  })

  describe('Accessibility', () => {
    it('should have role="alert" on validation error', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('invalid.txt', 1024, 'text/plain')] } })
      await waitFor(() => {
        const error = screen.getByTestId('inline-validation-error')
        expect(error).toHaveAttribute('role', 'alert')
      })
    })

    it('should have aria-live="assertive" for error announcements', async () => {
      renderWithProviders(<DocumentUploadPage />)
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('invalid.txt', 1024, 'text/plain')] } })
      await waitFor(() => {
        const error = screen.getByTestId('inline-validation-error')
        expect(error).toHaveAttribute('aria-live', 'assertive')
      })
    })
  })
})
