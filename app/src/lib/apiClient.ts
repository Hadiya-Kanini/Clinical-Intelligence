/**
 * Centralized API client for authenticated requests to the backend.
 * Handles session-expired responses, CSRF token management, and triggers forced logout.
 */

import { resetInactivityTimer } from '../hooks/useInactivityTimeout'

/**
 * API error response structure from the backend.
 */
export interface ApiErrorResponse {
  error: {
    code: string
    message: string
    details: string[]
  }
}

/**
 * Options for API requests.
 */
export interface ApiRequestOptions extends Omit<RequestInit, 'body'> {
  body?: unknown
  skipCsrf?: boolean
}

/**
 * Result type for API responses.
 * On failure, includes optional retryAfterSeconds for 429 rate limiting responses.
 */
export type ApiResult<T> =
  | { success: true; data: T }
  | { success: false; error: ApiErrorResponse['error']; status: number; retryAfterSeconds?: number }

/**
 * Session expired error code returned by the backend.
 */
const SESSION_EXPIRED_CODE = 'session_expired'

/**
 * CSRF-related error codes returned by the backend.
 */
const CSRF_ERROR_CODES = ['csrf_token_missing', 'csrf_token_invalid', 'csrf_validation_failed']

/**
 * Header name for CSRF token.
 */
const CSRF_HEADER_NAME = 'X-CSRF-TOKEN'

/**
 * HTTP methods that require CSRF token.
 */
const STATE_CHANGING_METHODS = ['POST', 'PUT', 'PATCH', 'DELETE']

/**
 * In-memory CSRF token cache.
 */
let csrfToken: string | null = null
let csrfTokenFetchPromise: Promise<string | null> | null = null

/**
 * Clear local auth state, CSRF token, and trigger cross-tab logout.
 */
function clearAuthState(): void {
  csrfToken = null
  csrfTokenFetchPromise = null
  try {
    localStorage.removeItem('ci_auth')
    localStorage.removeItem('ci_token')
    localStorage.removeItem('ci_user_role')
    // Trigger storage event for cross-tab logout
    localStorage.setItem('ci_auth', '')
    localStorage.removeItem('ci_auth')
  } catch {
    // Ignore localStorage errors
  }
}

/**
 * Fetch CSRF token from the backend.
 * Uses a singleton promise to prevent duplicate requests.
 */
async function fetchCsrfToken(): Promise<string | null> {
  // Return cached token if available
  if (csrfToken) {
    return csrfToken
  }

  // Return existing fetch promise if in progress
  if (csrfTokenFetchPromise) {
    return csrfTokenFetchPromise
  }

  // Fetch new token
  csrfTokenFetchPromise = (async () => {
    try {
      const response = await fetch('/api/v1/auth/csrf', {
        method: 'GET',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
        },
      })

      if (response.ok) {
        const data = await response.json()
        csrfToken = data.token
        return csrfToken
      }

      // If unauthorized, don't cache the failure
      return null
    } catch {
      return null
    } finally {
      csrfTokenFetchPromise = null
    }
  })()

  return csrfTokenFetchPromise
}

/**
 * Clear cached CSRF token (called on CSRF errors to force refresh).
 */
export function clearCsrfToken(): void {
  csrfToken = null
  csrfTokenFetchPromise = null
}

/**
 * Check if the error is a CSRF-related error.
 */
function isCsrfError(error: ApiErrorResponse['error']): boolean {
  return CSRF_ERROR_CODES.includes(error.code)
}

/**
 * Check if the error response indicates a session-expired condition.
 */
function isSessionExpired(error: ApiErrorResponse['error']): boolean {
  return error.code === SESSION_EXPIRED_CODE
}

/**
 * Parse error response from the backend.
 */
async function parseErrorResponse(response: Response): Promise<ApiErrorResponse['error']> {
  try {
    const data = await response.json()
    if (data.error && typeof data.error.code === 'string') {
      return data.error
    }
  } catch {
    // Failed to parse JSON
  }

  // Return a generic error if parsing fails
  return {
    code: 'unknown_error',
    message: response.statusText || 'An unexpected error occurred',
    details: [],
  }
}

/**
 * Internal request function without CSRF retry logic.
 */
async function executeRequest<T = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {},
  includeCsrf: boolean = true
): Promise<ApiResult<T>> {
  const { body, headers: customHeaders, skipCsrf, ...restOptions } = options
  const method = (restOptions.method || 'GET').toUpperCase()

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...customHeaders,
  }

  // Add CSRF token for state-changing methods (unless explicitly skipped)
  if (includeCsrf && !skipCsrf && STATE_CHANGING_METHODS.includes(method)) {
    const token = await fetchCsrfToken()
    if (token) {
      ;(headers as Record<string, string>)[CSRF_HEADER_NAME] = token
    }
  }

  const config: RequestInit = {
    ...restOptions,
    headers,
    credentials: 'include', // Include cookies for authentication
  }

  if (body !== undefined) {
    config.body = JSON.stringify(body)
  }

  const response = await fetch(endpoint, config)

  // Reset inactivity timer on any response (activity occurred)
  resetInactivityTimer()

  if (response.ok) {
    // Handle empty responses (e.g., 204 No Content)
    const contentType = response.headers.get('content-type')
    if (contentType?.includes('application/json')) {
      const data = await response.json()
      return { success: true, data: data as T }
    }
    return { success: true, data: {} as T }
  }

  // Parse error response
  const error = await parseErrorResponse(response)

  // Handle session-expired condition
  if (response.status === 401 && isSessionExpired(error)) {
    clearAuthState()
    // The storage event listener in AppShell will handle navigation
  }

  // Extract Retry-After header for 429 responses (US_028)
  let retryAfterSeconds: number | undefined
  if (response.status === 429) {
    const retryAfterHeader = response.headers.get('Retry-After')
    if (retryAfterHeader) {
      const parsed = parseInt(retryAfterHeader, 10)
      if (!isNaN(parsed) && parsed > 0) {
        retryAfterSeconds = parsed
      }
    }
  }

  return { success: false, error, status: response.status, retryAfterSeconds }
}

/**
 * Make an authenticated API request.
 * 
 * Features:
 * - Automatically includes credentials for cookie-based auth
 * - Automatically includes CSRF token for state-changing requests (POST, PUT, PATCH, DELETE)
 * - Handles session-expired responses by clearing auth state
 * - Handles CSRF token errors with one-time retry after token refresh
 * - Resets inactivity timer on successful requests
 * - Normalizes error responses
 * 
 * @param endpoint - API endpoint path (e.g., '/api/v1/auth/me')
 * @param options - Request options
 * @returns Promise with typed result
 */
export async function apiRequest<T = unknown>(
  endpoint: string,
  options: ApiRequestOptions = {}
): Promise<ApiResult<T>> {
  try {
    const result = await executeRequest<T>(endpoint, options)

    // If CSRF error, refresh token and retry once
    if (!result.success && result.status === 403 && isCsrfError(result.error)) {
      clearCsrfToken()
      const retryResult = await executeRequest<T>(endpoint, options)
      return retryResult
    }

    return result
  } catch (err) {
    // Network error or other fetch failure
    const error: ApiErrorResponse['error'] = {
      code: 'network_error',
      message: err instanceof Error ? err.message : 'Network request failed',
      details: [],
    }
    return { success: false, error, status: 0 }
  }
}

/**
 * Convenience methods for common HTTP verbs.
 */
export const api = {
  get: <T = unknown>(endpoint: string, options?: ApiRequestOptions) =>
    apiRequest<T>(endpoint, { ...options, method: 'GET' }),

  post: <T = unknown>(endpoint: string, body?: unknown, options?: ApiRequestOptions) =>
    apiRequest<T>(endpoint, { ...options, method: 'POST', body }),

  put: <T = unknown>(endpoint: string, body?: unknown, options?: ApiRequestOptions) =>
    apiRequest<T>(endpoint, { ...options, method: 'PUT', body }),

  patch: <T = unknown>(endpoint: string, body?: unknown, options?: ApiRequestOptions) =>
    apiRequest<T>(endpoint, { ...options, method: 'PATCH', body }),

  delete: <T = unknown>(endpoint: string, options?: ApiRequestOptions) =>
    apiRequest<T>(endpoint, { ...options, method: 'DELETE' }),
}

export default api
