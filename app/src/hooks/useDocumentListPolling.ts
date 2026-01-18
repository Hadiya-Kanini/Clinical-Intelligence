import { useCallback, useEffect, useRef, useState } from 'react'
import { listDocuments, type DocumentListItem } from '../lib/documentApi'

const DEFAULT_POLL_INTERVAL_MS = 5000

interface UseDocumentListPollingOptions {
  pollIntervalMs?: number
  enabled?: boolean
  page?: number
  pageSize?: number
  search?: string
}

interface UseDocumentListPollingResult {
  items: DocumentListItem[]
  total: number
  loading: boolean
  error: string | null
  isRefreshing: boolean
  lastUpdatedAt: Date | null
  refreshNow: () => Promise<void>
}

/**
 * Hook for polling the documents list endpoint with automatic refresh.
 * Implements UXR-043: refresh at least every 5 seconds while documents are processing.
 * 
 * Features:
 * - 5-second polling interval (configurable)
 * - Auto-stops when all documents are terminal (Completed/Failed)
 * - No overlapping requests (single in-flight guard)
 * - Proper cleanup on unmount
 * - Manual refresh capability
 * - Preserves last successful data on error
 */
export function useDocumentListPolling(
  options: UseDocumentListPollingOptions = {}
): UseDocumentListPollingResult {
  const {
    pollIntervalMs = DEFAULT_POLL_INTERVAL_MS,
    enabled = true,
    page = 1,
    pageSize = 20,
    search = '',
  } = options

  const [items, setItems] = useState<DocumentListItem[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [lastUpdatedAt, setLastUpdatedAt] = useState<Date | null>(null)

  // Ref to track if a request is in-flight (prevents overlapping requests)
  const isRequestInFlightRef = useRef(false)
  // Ref to track if component is mounted
  const isMountedRef = useRef(true)

  const fetchDocuments = useCallback(async (isManualRefresh = false) => {
    // Prevent overlapping requests
    if (isRequestInFlightRef.current) {
      return
    }

    isRequestInFlightRef.current = true
    
    if (isManualRefresh) {
      setIsRefreshing(true)
    }

    try {
      const result = await listDocuments(page, pageSize, search)

      // Only update state if component is still mounted
      if (!isMountedRef.current) return

      if (result.success) {
        setItems(result.data.items)
        setTotal(result.data.total)
        setError(null)
        setLastUpdatedAt(new Date())
      } else {
        // Preserve last successful data, only update error
        setError(result.error.message || 'Failed to load documents')
      }
    } catch (err) {
      if (!isMountedRef.current) return
      setError('Network error. Please try again.')
    } finally {
      if (isMountedRef.current) {
        setLoading(false)
        setIsRefreshing(false)
      }
      isRequestInFlightRef.current = false
    }
  }, [page, pageSize, search])

  // Manual refresh function exposed to consumers
  const refreshNow = useCallback(async () => {
    await fetchDocuments(true)
  }, [fetchDocuments])

  // Check if any documents are in non-terminal status (Pending/Processing)
  const hasActiveDocuments = items.some(
    (doc) => doc.status === 'Pending' || doc.status === 'Processing'
  )

  // Initial fetch on mount
  useEffect(() => {
    isMountedRef.current = true
    fetchDocuments()

    return () => {
      isMountedRef.current = false
    }
  }, [fetchDocuments])

  // Polling effect - only runs when enabled and there are active documents
  useEffect(() => {
    if (!enabled || !hasActiveDocuments) {
      return
    }

    const intervalId = setInterval(() => {
      fetchDocuments()
    }, pollIntervalMs)

    return () => {
      clearInterval(intervalId)
    }
  }, [enabled, hasActiveDocuments, pollIntervalMs, fetchDocuments])

  return {
    items,
    total,
    loading,
    error,
    isRefreshing,
    lastUpdatedAt,
    refreshNow,
  }
}

export default useDocumentListPolling
