import { describe, it, expect } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import DocumentUploadPage from '../pages/DocumentUploadPage'

/**
 * Unit/Integration tests for DocumentUploadPage (SCR-005)
 * Tests cover:
 * - File queueing behavior
 * - Validation logic (file types, max size, max count)
 * - Queue list rendering (count + filenames)
 * - Edge cases
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

describe('DocumentUploadPage', () => {
  describe('Initial Rendering', () => {
    it('should render drop zone with correct initial state', () => {
      renderWithProviders(<DocumentUploadPage />)

      expect(screen.getByTestId('drop-zone')).toBeInTheDocument()
      expect(screen.getByTestId('drop-zone-text')).toHaveTextContent('Drag & drop files here')
      expect(screen.getByText('0 selected')).toBeInTheDocument()
      expect(screen.getByLabelText('Patient ID *')).toBeInTheDocument()
    })

    it('should render file input with correct attributes', () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      expect(fileInput).toHaveAttribute('type', 'file')
      expect(fileInput).toHaveAttribute('multiple')
      expect(fileInput).toHaveAttribute('accept', '.pdf,.docx')
    })

    it('should render Upload button as disabled initially', () => {
      renderWithProviders(<DocumentUploadPage />)

      const uploadButton = screen.getByRole('button', { name: 'Upload' })
      expect(uploadButton).toBeDisabled()
    })

    it('should render Clear button as disabled initially', () => {
      renderWithProviders(<DocumentUploadPage />)

      const clearButton = screen.getByRole('button', { name: 'Clear' })
      expect(clearButton).toBeDisabled()
    })
  })

  describe('File Selection via Input', () => {
    it('should queue files when selected via file input', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('test.pdf')

      fireEvent.change(fileInput, { target: { files: [file] } })

      await waitFor(() => {
        expect(screen.getByText('1 selected')).toBeInTheDocument()
        expect(screen.getByText('test.pdf')).toBeInTheDocument()
      })
    })

    it('should queue multiple files at once', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const files = [
        createMockFile('doc1.pdf'),
        createMockFile('doc2.docx', 1024, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'),
      ]

      fireEvent.change(fileInput, { target: { files } })

      await waitFor(() => {
        expect(screen.getByText('2 selected')).toBeInTheDocument()
        expect(screen.getByText('doc1.pdf')).toBeInTheDocument()
        expect(screen.getByText('doc2.docx')).toBeInTheDocument()
      })
    })

    it('should append files to existing queue', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')

      fireEvent.change(fileInput, { target: { files: [createMockFile('first.pdf')] } })

      await waitFor(() => {
        expect(screen.getByText('1 selected')).toBeInTheDocument()
      })

      fireEvent.change(fileInput, { target: { files: [createMockFile('second.pdf')] } })

      await waitFor(() => {
        expect(screen.getByText('2 selected')).toBeInTheDocument()
        expect(screen.getByText('first.pdf')).toBeInTheDocument()
        expect(screen.getByText('second.pdf')).toBeInTheDocument()
      })
    })
  })

  describe('File Validation', () => {
    it('should reject unsupported file types and show error', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const invalidFile = createMockFile('image.png', 1024, 'image/png')

      fireEvent.change(fileInput, { target: { files: [invalidFile] } })

      await waitFor(() => {
        expect(screen.getByText(/unsupported file type/i)).toBeInTheDocument()
        expect(screen.getByText('1 selected')).toBeInTheDocument()
      })
    })

    it('should reject files exceeding 50MB', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const largeFile = createMockFile('large.pdf', 51 * 1024 * 1024)

      fireEvent.change(fileInput, { target: { files: [largeFile] } })

      await waitFor(() => {
        expect(screen.getByText(/file exceeds 50mb limit/i)).toBeInTheDocument()
        expect(screen.getByText('1 selected')).toBeInTheDocument()
      })
    })

    it('should limit batch to 10 files maximum', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const files = Array.from({ length: 12 }, (_, i) => createMockFile(`file${i + 1}.pdf`))

      fireEvent.change(fileInput, { target: { files } })

      await waitFor(() => {
        expect(screen.getByText(/up to 10 files per batch/i)).toBeInTheDocument()
        expect(screen.getByText('10 selected')).toBeInTheDocument()
      })
    })

    it('should accept valid PDF files', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('valid.pdf', 1024, 'application/pdf')

      fireEvent.change(fileInput, { target: { files: [file] } })

      await waitFor(() => {
        expect(screen.getByText('1 selected')).toBeInTheDocument()
        expect(screen.getByText('valid.pdf')).toBeInTheDocument()
      })
    })

    it('should accept valid DOCX files', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const file = createMockFile('valid.docx', 1024, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document')

      fireEvent.change(fileInput, { target: { files: [file] } })

      await waitFor(() => {
        expect(screen.getByText('1 selected')).toBeInTheDocument()
        expect(screen.getByText('valid.docx')).toBeInTheDocument()
      })
    })
  })

  describe('Queue Management', () => {
    it('should remove file from queue when Remove button is clicked', async () => {
      const user = userEvent.setup()
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('removable.pdf')] } })

      await waitFor(() => {
        expect(screen.getByText('1 selected')).toBeInTheDocument()
      })

      const removeButton = screen.getByTestId('remove-file-btn')
      await user.click(removeButton)

      await waitFor(() => {
        expect(screen.getByText('0 selected')).toBeInTheDocument()
        expect(screen.queryByText('removable.pdf')).not.toBeInTheDocument()
      })
    })

    it('should clear all files and patient ID when Clear button is clicked', async () => {
      const user = userEvent.setup()
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      const files = [createMockFile('file1.pdf'), createMockFile('file2.pdf')]
      fireEvent.change(fileInput, { target: { files } })

      // Add patient ID
      const patientIdInput = screen.getByLabelText('Patient ID *')
      await user.type(patientIdInput, 'patient-123')

      await waitFor(() => {
        expect(screen.getByText('2 selected')).toBeInTheDocument()
        expect(patientIdInput).toHaveValue('patient-123')
      })

      const clearButton = screen.getByRole('button', { name: 'Clear' })
      await user.click(clearButton)

      await waitFor(() => {
        expect(screen.getByText('0 selected')).toBeInTheDocument()
        expect(screen.queryByText('file1.pdf')).not.toBeInTheDocument()
        expect(screen.queryByText('file2.pdf')).not.toBeInTheDocument()
        expect(patientIdInput).toHaveValue('')
      })
    })
  })

  describe('Upload Button State', () => {
    it('should keep Upload button disabled when files are queued but no patient ID', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('test.pdf')] } })

      await waitFor(() => {
        const uploadButton = screen.getByRole('button', { name: 'Upload' })
        expect(uploadButton).toBeDisabled()
      })
    })

    it('should enable Upload button when files are queued and patient ID is provided', async () => {
      const user = userEvent.setup()
      renderWithProviders(<DocumentUploadPage />)

      // Add files
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('test.pdf')] } })

      // Add patient ID
      const patientIdInput = screen.getByLabelText('Patient ID *')
      await user.type(patientIdInput, 'patient-123')

      await waitFor(() => {
        const uploadButton = screen.getByRole('button', { name: 'Upload' })
        expect(uploadButton).toBeEnabled()
      })
    })

    it('should enable Clear button when files are queued', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile('test.pdf')] } })

      await waitFor(() => {
        const clearButton = screen.getByRole('button', { name: 'Clear' })
        expect(clearButton).toBeEnabled()
      })
    })
  })

  describe('Long Filename Handling', () => {
    it('should display title attribute for long filenames', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const longFileName = 'this_is_a_very_long_filename_that_should_be_truncated_in_the_ui.pdf'
      const fileInput = screen.getByTestId('file-input')
      fireEvent.change(fileInput, { target: { files: [createMockFile(longFileName)] } })

      await waitFor(() => {
        const fileNameElement = screen.getByTestId('file-name')
        expect(fileNameElement).toHaveAttribute('title', longFileName)
      })
    })
  })

  describe('Drag and Drop Events', () => {
    it('should update text on drag enter', () => {
      renderWithProviders(<DocumentUploadPage />)

      const dropZone = screen.getByTestId('drop-zone')

      fireEvent.dragEnter(dropZone, {
        dataTransfer: { files: [] },
      })

      expect(screen.getByTestId('drop-zone-text')).toHaveTextContent('Drop files here')
    })

    it('should reset text on drag leave', () => {
      renderWithProviders(<DocumentUploadPage />)

      const dropZone = screen.getByTestId('drop-zone')

      fireEvent.dragEnter(dropZone, { dataTransfer: { files: [] } })
      fireEvent.dragLeave(dropZone, { dataTransfer: { files: [] } })

      expect(screen.getByTestId('drop-zone-text')).toHaveTextContent('Drag & drop files here')
    })

    it('should queue files on drop', async () => {
      renderWithProviders(<DocumentUploadPage />)

      const dropZone = screen.getByTestId('drop-zone')
      const file = createMockFile('dropped.pdf')

      fireEvent.drop(dropZone, {
        dataTransfer: { files: [file] },
      })

      await waitFor(() => {
        expect(screen.getByText('1 selected')).toBeInTheDocument()
        expect(screen.getByText('dropped.pdf')).toBeInTheDocument()
      })
    })

    it('should reset highlight after drop', () => {
      renderWithProviders(<DocumentUploadPage />)

      const dropZone = screen.getByTestId('drop-zone')
      const file = createMockFile('test.pdf')

      fireEvent.dragEnter(dropZone, { dataTransfer: { files: [] } })
      fireEvent.drop(dropZone, { dataTransfer: { files: [file] } })

      expect(screen.getByTestId('drop-zone-text')).toHaveTextContent('Drag & drop files here')
    })
  })

  describe('Accessibility', () => {
    it('should have accessible drop zone with role and aria-label', () => {
      renderWithProviders(<DocumentUploadPage />)

      const dropZone = screen.getByTestId('drop-zone')
      expect(dropZone).toHaveAttribute('role', 'region')
      expect(dropZone).toHaveAttribute('aria-label', 'File drop zone')
    })

    it('should have accessible file input with aria-label', () => {
      renderWithProviders(<DocumentUploadPage />)

      const fileInput = screen.getByTestId('file-input')
      expect(fileInput).toHaveAttribute('aria-label', 'Select files to upload')
    })
  })
})
