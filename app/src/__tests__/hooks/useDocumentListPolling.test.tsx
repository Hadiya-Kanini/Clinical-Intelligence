import { renderHook, act, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useDocumentListPolling } from '../../hooks/useDocumentListPolling'
import * as documentApi from '../../lib/documentApi'

// Mock the documentApi module
vi.mock('../../lib/documentApi', () => ({
  listDocuments: vi.fn(),
}))

const mockListDocuments = documentApi.listDocuments as ReturnType<typeof vi.fn>

describe('useDocumentListPolling (US_057)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.useFakeTimers({ shouldAdvanceTime: true })
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  const createMockResponse = (items: documentApi.DocumentListItem[] = []) => ({
    success: true as const,
    data: {
      items,
      total: items.length,
      page: 1,
      pageSize: 20,
    },
  })

  const createMockDocument = (overrides: Partial<documentApi.DocumentListItem> = {}): documentApi.DocumentListItem => ({
    id: 'doc-123',
    fileName: 'test.pdf',
    uploadedAt: '2024-01-15T10:30:00Z',
    status: 'Completed',
    patientId: 'patient-456',
    ...overrides,
  })

  it('fetches documents on initial mount', async () => {
    mockListDocuments.mockResolvedValue(createMockResponse([createMockDocument()]))

    const { result } = renderHook(() => useDocumentListPolling())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(mockListDocuments).toHaveBeenCalledTimes(1)
    expect(result.current.items).toHaveLength(1)
    expect(result.current.error).toBeNull()
  })

  it('sets loading state correctly', async () => {
    mockListDocuments.mockResolvedValue(createMockResponse([]))

    const { result } = renderHook(() => useDocumentListPolling())

    // Initially loading
    expect(result.current.loading).toBe(true)

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })
  })

  it('stops polling when all documents are terminal (Completed/Failed)', async () => {
    const completedDoc = createMockDocument({ status: 'Completed' })
    mockListDocuments.mockResolvedValue(createMockResponse([completedDoc]))

    const { result } = renderHook(() => useDocumentListPolling({ pollIntervalMs: 5000 }))

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(mockListDocuments).toHaveBeenCalledTimes(1)

    // Advance timer - should NOT trigger another fetch since all docs are terminal
    await act(async () => {
      vi.advanceTimersByTime(6000)
    })

    // Still only 1 call
    expect(mockListDocuments).toHaveBeenCalledTimes(1)
  })

  it('exposes refreshNow function for manual refresh', async () => {
    mockListDocuments.mockResolvedValue(createMockResponse([createMockDocument()]))

    const { result } = renderHook(() => useDocumentListPolling())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(mockListDocuments).toHaveBeenCalledTimes(1)

    // Trigger manual refresh
    await act(async () => {
      await result.current.refreshNow()
    })

    expect(mockListDocuments).toHaveBeenCalledTimes(2)
  })

  it('updates lastUpdatedAt after successful fetch', async () => {
    mockListDocuments.mockResolvedValue(createMockResponse([createMockDocument()]))

    const { result } = renderHook(() => useDocumentListPolling())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.lastUpdatedAt).toBeInstanceOf(Date)
  })

  it('sets error on failed fetch', async () => {
    mockListDocuments.mockResolvedValue({ success: false, error: { message: 'Network error' } })

    const { result } = renderHook(() => useDocumentListPolling())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.error).toBe('Network error')
  })

  it('passes search parameter to API', async () => {
    mockListDocuments.mockResolvedValue(createMockResponse([]))

    renderHook(() => useDocumentListPolling({ search: 'test-search' }))

    await waitFor(() => {
      expect(mockListDocuments).toHaveBeenCalledWith(1, 20, 'test-search')
    })
  })

  it('cleans up on unmount', async () => {
    const processingDoc = createMockDocument({ status: 'Processing' })
    mockListDocuments.mockResolvedValue(createMockResponse([processingDoc]))

    const { result, unmount } = renderHook(() => useDocumentListPolling({ pollIntervalMs: 5000 }))

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const callCountBeforeUnmount = mockListDocuments.mock.calls.length

    // Unmount the hook
    unmount()

    // Advance timer after unmount
    await act(async () => {
      vi.advanceTimersByTime(6000)
    })

    // Should not have additional calls after unmount
    expect(mockListDocuments.mock.calls.length).toBe(callCountBeforeUnmount)
  })
})
