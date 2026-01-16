/**
 * Document API client for batch upload operations and document management.
 */

import api, { type ApiResult } from './apiClient'

export interface BatchUploadResponse {
  batchId: string
  patientId: string
  totalFilesReceived: number
  filesAccepted: number
  filesRejected: number
  batchLimitExceeded: boolean
  batchLimitWarning?: string
  fileResults: FileUploadResult[]
  acknowledgedAt: string
}

export interface FileUploadResult {
  fileName: string
  documentId?: string
  isAccepted: boolean
  status: string
  validationErrors: string[]
  rejectionReason?: string
}

export interface UploadAcknowledgmentResponse {
  documentId: string
  fileName: string
  fileSize: number
  status: string
  isValid: boolean
  validationErrors: string[]
  acknowledgedAt: string
  errorCode?: number
  errorType?: string
}

export interface DocumentListItem {
  id: string
  fileName: string
  uploadedAt: string
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed'
  patientId: string
  patientName?: string
  fileSize?: number
  errorMessage?: string
}

export interface DocumentListResponse {
  items: DocumentListItem[]
  total: number
  page: number
  pageSize: number
}

/**
 * Upload multiple documents in a batch.
 * Enforces 10-file limit per batch (FR-014).
 */
export async function uploadDocumentBatch(
  patientId: string,
  files: File[],
  signal?: AbortSignal
): Promise<{ success: true; data: BatchUploadResponse } | { success: false; error: string }> {
  try {
    const formData = new FormData()
    formData.append('patientId', patientId)

    files.forEach((file) => {
      formData.append('files', file)
    })

    // Fetch CSRF token first
    const csrfResponse = await fetch('/api/v1/auth/csrf', {
      method: 'GET',
      credentials: 'include',
    })

    let csrfToken: string | null = null
    if (csrfResponse.ok) {
      const csrfData = await csrfResponse.json()
      csrfToken = csrfData.token
    }

    const headers: HeadersInit = {}
    if (csrfToken) {
      headers['X-CSRF-TOKEN'] = csrfToken
    }

    const response = await fetch('/api/v1/documents/batch', {
      method: 'POST',
      credentials: 'include',
      headers,
      body: formData,
      signal,
    })

    if (response.ok) {
      const data = await response.json()
      return { success: true, data }
    }

    const errorData = await response.json().catch(() => null)
    const errorMessage = errorData?.error?.message || `Upload failed with status ${response.status}`
    return { success: false, error: errorMessage }
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') {
      return { success: false, error: 'Upload cancelled' }
    }
    return { success: false, error: error instanceof Error ? error.message : 'Upload failed' }
  }
}

/**
 * Upload a single document.
 */
export async function uploadDocument(
  patientId: string,
  file: File,
  signal?: AbortSignal
): Promise<{ success: true; data: UploadAcknowledgmentResponse } | { success: false; error: string }> {
  try {
    const formData = new FormData()
    formData.append('patientId', patientId)
    formData.append('file', file)

    // Fetch CSRF token first
    const csrfResponse = await fetch('/api/v1/auth/csrf', {
      method: 'GET',
      credentials: 'include',
    })

    let csrfToken: string | null = null
    if (csrfResponse.ok) {
      const csrfData = await csrfResponse.json()
      csrfToken = csrfData.token
    }

    const headers: HeadersInit = {}
    if (csrfToken) {
      headers['X-CSRF-TOKEN'] = csrfToken
    }

    const response = await fetch('/api/v1/documents/upload', {
      method: 'POST',
      credentials: 'include',
      headers,
      body: formData,
      signal,
    })

    if (response.ok) {
      const data = await response.json()
      return { success: true, data }
    }

    const errorData = await response.json().catch(() => null)
    const errorMessage = errorData?.error?.message || `Upload failed with status ${response.status}`
    return { success: false, error: errorMessage }
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') {
      return { success: false, error: 'Upload cancelled' }
    }
    return { success: false, error: error instanceof Error ? error.message : 'Upload failed' }
  }
}

/**
 * Retrieve document content by ID.
 */
export async function getDocumentContent(documentId: string): Promise<Blob | null> {
  try {
    const response = await fetch(`/api/v1/documents/${documentId}/content`, {
      method: 'GET',
      credentials: 'include',
    })

    if (response.ok) {
      return await response.blob()
    }

    return null
  } catch {
    return null
  }
}

/**
 * List documents with pagination and filtering.
 */
export async function listDocuments(
  page: number = 1,
  pageSize: number = 20,
  search?: string,
  status?: string
): Promise<ApiResult<DocumentListResponse>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  })
  
  if (search) {
    params.append('search', search)
  }
  
  if (status) {
    params.append('status', status)
  }

  const queryString = params.toString()
  return api.get<DocumentListResponse>(`/api/v1/documents?${queryString}`)
}
