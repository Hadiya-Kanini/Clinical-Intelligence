import { render, screen, fireEvent } from '@testing-library/react'
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

describe('DocumentListPage - Real-time Updates (US_057)', () => {
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
    mockRefreshNow.mockResolvedValue(undefined)
  })

  it('renders Refresh button in header', () => {
    setupMockHook([createMockDocument()])
    render(<DocumentListPage />)
    expect(screen.getByRole('button', { name: /refresh/i })).toBeInTheDocument()
  })

  it('triggers immediate refresh when Refresh button is clicked', () => {
    setupMockHook([createMockDocument()])
    render(<DocumentListPage />)
    const refreshButton = screen.getByRole('button', { name: /refresh/i })
    fireEvent.click(refreshButton)
    expect(mockRefreshNow).toHaveBeenCalledTimes(1)
  })

  it('disables Refresh button while refreshing', () => {
    setupMockHook([createMockDocument()], { isRefreshing: true })
    render(<DocumentListPage />)
    expect(screen.getByRole('button', { name: /refreshing/i })).toBeDisabled()
  })

  it('displays last updated timestamp', () => {
    setupMockHook([createMockDocument()], { lastUpdatedAt: new Date() })
    render(<DocumentListPage />)
    expect(screen.getByText(/last updated:/i)).toBeInTheDocument()
  })

  it('displays Processing status badge', () => {
    setupMockHook([createMockDocument({ status: 'Processing' })])
    render(<DocumentListPage />)
    // Processing appears as both column header and status badge
    expect(screen.getAllByText('Processing').length).toBeGreaterThanOrEqual(2)
  })

  it('displays Completed status badge', () => {
    setupMockHook([createMockDocument({ status: 'Completed' })])
    render(<DocumentListPage />)
    expect(screen.getByText('Completed')).toBeInTheDocument()
  })

  it('preserves data and shows error when API error occurs', () => {
    setupMockHook([createMockDocument({ fileName: 'important.pdf' })], { error: 'Network error' })
    render(<DocumentListPage />)
    expect(screen.getByText('Network error')).toBeInTheDocument()
    expect(screen.getByText('important.pdf')).toBeInTheDocument()
  })

  it('shows error Alert when API fails', () => {
    setupMockHook([], { error: 'Failed to load documents' })
    render(<DocumentListPage />)
    expect(screen.getByText('Failed to load documents')).toBeInTheDocument()
  })

  it('search filter works with documents', () => {
    setupMockHook([
      createMockDocument({ id: 'doc-1', fileName: 'alpha.pdf' }),
      createMockDocument({ id: 'doc-2', fileName: 'beta.pdf' }),
    ])
    render(<DocumentListPage />)
    expect(screen.getByText('alpha.pdf')).toBeInTheDocument()
    expect(screen.getByText('beta.pdf')).toBeInTheDocument()

    const searchInput = screen.getByPlaceholderText('Search documents')
    fireEvent.change(searchInput, { target: { value: 'alpha' } })

    expect(screen.getByText('alpha.pdf')).toBeInTheDocument()
    expect(screen.queryByText('beta.pdf')).not.toBeInTheDocument()
  })

  it('Clear button resets search filter', () => {
    setupMockHook([
      createMockDocument({ id: 'doc-1', fileName: 'alpha.pdf' }),
      createMockDocument({ id: 'doc-2', fileName: 'beta.pdf' }),
    ])
    render(<DocumentListPage />)

    const searchInput = screen.getByPlaceholderText('Search documents')
    fireEvent.change(searchInput, { target: { value: 'alpha' } })
    expect(screen.queryByText('beta.pdf')).not.toBeInTheDocument()

    const clearButton = screen.getByRole('button', { name: /clear/i })
    fireEvent.click(clearButton)

    expect(screen.getByText('alpha.pdf')).toBeInTheDocument()
    expect(screen.getByText('beta.pdf')).toBeInTheDocument()
  })

  it('shows loading state', () => {
    setupMockHook([], { loading: true })
    render(<DocumentListPage />)
    expect(screen.getByText('Loading documents...')).toBeInTheDocument()
  })

  it('shows empty state when no documents exist', () => {
    setupMockHook([])
    render(<DocumentListPage />)
    expect(screen.getByText(/no documents found/i)).toBeInTheDocument()
  })
})
