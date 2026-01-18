/**
 * Documents API client for document content retrieval.
 * Used by the source document viewer for citation navigation.
 */

import { type ApiResult } from './apiClient'

/**
 * Fetches document content as a Blob for viewing.
 * @param documentId - The document ID to fetch
 * @returns Promise with Blob result or error
 */
export async function getDocumentContent(documentId: string): Promise<ApiResult<Blob>> {
  try {
    const token = localStorage.getItem('ci_jwt_token')
    const headers: HeadersInit = {
      'Accept': 'application/pdf, application/octet-stream, */*',
    }
    
    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }

    const response = await fetch(`/api/v1/documents/${documentId}/content`, {
      method: 'GET',
      credentials: 'include',
      headers,
    })

    if (response.ok) {
      const blob = await response.blob()
      return { success: true, data: blob }
    }

    // Parse error response
    try {
      const errorData = await response.json()
      return {
        success: false,
        error: errorData.error || {
          code: 'unknown_error',
          message: 'Failed to fetch document content',
          details: [],
        },
        status: response.status,
      }
    } catch {
      return {
        success: false,
        error: {
          code: 'unknown_error',
          message: response.statusText || 'Failed to fetch document content',
          details: [],
        },
        status: response.status,
      }
    }
  } catch (err) {
    return {
      success: false,
      error: {
        code: 'network_error',
        message: err instanceof Error ? err.message : 'Network request failed',
        details: [],
      },
      status: 0,
    }
  }
}

/**
 * Creates a Blob URL for document content.
 * Remember to revoke the URL when done to prevent memory leaks.
 * @param blob - The document blob
 * @returns Object URL string
 */
export function createDocumentUrl(blob: Blob): string {
  return URL.createObjectURL(blob)
}

/**
 * Revokes a Blob URL to free memory.
 * @param url - The object URL to revoke
 */
export function revokeDocumentUrl(url: string): void {
  URL.revokeObjectURL(url)
}

export default {
  getDocumentContent,
  createDocumentUrl,
  revokeDocumentUrl,
}
