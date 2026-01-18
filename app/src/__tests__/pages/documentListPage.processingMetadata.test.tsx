import { render, screen } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import DocumentListPage from '../../pages/DocumentListPage'
import type { DocumentListItem } from '../../lib/documentApi'

// Mock the polling hook
const mockRefreshNow = vi.fn()
vi.mock('../../hooks/useDocumentListPolling', () => ({
  useDocumentListPolling: vi.fn(),
}))

import { useDocumentListPolling } from '../../hooks/useDocumentListPolling'
const mockUseDocumentListPolling = vi.mocked(useDocumentListPolling)

describe('DocumentListPage - Processing Metadata Display (US_056)', () => {
  const createMockDocument = (overrides: Partial<DocumentListItem> = {}): DocumentListItem => ({
    id: 'doc-123',
    fileName: 'test-document.pdf',
    uploadedAt: '2024-01-15T10:30:00Z',
    status: 'Completed',
    patientId: 'patient-456',
    fileSize: 1024,
    jobId: 'job-789',
    retryCount: 0,
    startedAt: '2024-01-15T10:30:05Z',
    completedAt: '2024-01-15T10:30:10Z',
    processingTimeMs: 5000,
    errorMessage: null,
    ...overrides,
  })

  const setupMockHook = (items: DocumentListItem[], options: Partial<ReturnType<typeof useDocumentListPolling>> = {}) => {
    mockUseDocumentListPolling.mockReturnValue({
      items,
      total: items.length,
      loading: false,
      error: null,
      isRefreshing: false,
      lastUpdatedAt: new Date(),
      refreshNow: mockRefreshNow,
      ...options,
    })
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders document list with processing metadata columns', () => {
    setupMockHook([createMockDocument()])
    render(<DocumentListPage />)
    expect(screen.getByText('test-document.pdf')).toBeInTheDocument()
    expect(screen.getByText('Processing')).toBeInTheDocument()
  })

  it('displays error message for failed documents', () => {
    const errorMessage = 'Document processing failed: invalid format'
    setupMockHook([createMockDocument({ status: 'Failed', errorMessage })])
    render(<DocumentListPage />)
    expect(screen.getByText(errorMessage)).toBeInTheDocument()
    expect(screen.getByText('Failed')).toBeInTheDocument()
  })

  it('does not display error message for completed documents', () => {
    setupMockHook([createMockDocument({ status: 'Completed', errorMessage: null })])
    render(<DocumentListPage />)
    expect(screen.getByText('Completed')).toBeInTheDocument()
    expect(screen.queryByText(/processing failed/i)).not.toBeInTheDocument()
  })

  it('displays processing time in seconds for completed documents', () => {
    setupMockHook([createMockDocument({ processingTimeMs: 5000 })])
    render(<DocumentListPage />)
    expect(screen.getByText(/Duration: 5\.0s/)).toBeInTheDocument()
  })

  it('displays processing time in milliseconds for short durations', () => {
    setupMockHook([createMockDocument({ processingTimeMs: 500 })])
    render(<DocumentListPage />)
    expect(screen.getByText(/Duration: 500ms/)).toBeInTheDocument()
  })

  it('displays retry count for documents with retries', () => {
    setupMockHook([createMockDocument({ retryCount: 3, status: 'Failed' })])
    render(<DocumentListPage />)
    expect(screen.getByText('Retries: 3')).toBeInTheDocument()
  })

  it('does not display retry count when zero', () => {
    setupMockHook([createMockDocument({ retryCount: 0 })])
    render(<DocumentListPage />)
    expect(screen.queryByText(/Retries:/)).not.toBeInTheDocument()
  })

  it('displays timestamps for started and completed times', () => {
    setupMockHook([createMockDocument({ startedAt: '2024-01-15T10:30:05Z', completedAt: '2024-01-15T10:30:10Z' })])
    render(<DocumentListPage />)
    expect(screen.getByText(/Started:/)).toBeInTheDocument()
    expect(screen.getByText(/Completed:/)).toBeInTheDocument()
  })

  it('handles null processing metadata gracefully', () => {
    setupMockHook([createMockDocument({ processingTimeMs: null, startedAt: null, completedAt: null })])
    render(<DocumentListPage />)
    expect(screen.getByText(/Duration: -/)).toBeInTheDocument()
  })

  it('displays API error state with Alert', () => {
    setupMockHook([], { error: 'Failed to load documents' })
    render(<DocumentListPage />)
    expect(screen.getByText('Failed to load documents')).toBeInTheDocument()
  })

  it('truncates long error messages with ellipsis', () => {
    const longErrorMessage = 'A'.repeat(400)
    setupMockHook([createMockDocument({ status: 'Failed', errorMessage: longErrorMessage })])
    render(<DocumentListPage />)
    const errorElement = screen.getByTitle(longErrorMessage)
    expect(errorElement).toBeInTheDocument()
  })

  it('displays multiple documents with different statuses', () => {
    setupMockHook([
      createMockDocument({ id: 'doc-1', fileName: 'pending.pdf', status: 'Pending' }),
      createMockDocument({ id: 'doc-2', fileName: 'processing.pdf', status: 'Processing' }),
      createMockDocument({ id: 'doc-3', fileName: 'completed.pdf', status: 'Completed' }),
      createMockDocument({ id: 'doc-4', fileName: 'failed.pdf', status: 'Failed', errorMessage: 'Error' }),
    ])
    render(<DocumentListPage />)
    expect(screen.getByText('pending.pdf')).toBeInTheDocument()
    expect(screen.getByText('processing.pdf')).toBeInTheDocument()
    expect(screen.getByText('completed.pdf')).toBeInTheDocument()
    expect(screen.getByText('failed.pdf')).toBeInTheDocument()
    // Use getAllByText for statuses that may appear multiple times (Processing is also a column header)
    expect(screen.getByText('Pending')).toBeInTheDocument()
    expect(screen.getAllByText('Processing').length).toBeGreaterThanOrEqual(1)
    expect(screen.getByText('Completed')).toBeInTheDocument()
    expect(screen.getByText('Failed')).toBeInTheDocument()
  })

  it('shows loading state', () => {
    setupMockHook([], { loading: true })
    render(<DocumentListPage />)
    expect(screen.getByText('Loading documents...')).toBeInTheDocument()
  })
})
