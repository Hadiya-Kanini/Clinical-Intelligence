/**
 * Centralized API client for authenticated requests to the backend.
 * Handles session-expired responses and triggers forced logout.
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
}

/**
 * Result type for API responses.
 */
export type ApiResult<T> =
  | { success: true; data: T }
  | { success: false; error: ApiErrorResponse['error']; status: number }

/**
 * Session expired error code returned by the backend.
 */
const SESSION_EXPIRED_CODE = 'session_expired'

/**
 * Clear local auth state and trigger cross-tab logout.
 */
function clearAuthState(): void {
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
 * Make an authenticated API request.
 * 
 * Features:
 * - Automatically includes credentials for cookie-based auth
 * - Handles session-expired responses by clearing auth state
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
  const { body, headers: customHeaders, ...restOptions } = options

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...customHeaders,
  }

  const config: RequestInit = {
    ...restOptions,
    headers,
    credentials: 'include', // Include cookies for authentication
  }

  if (body !== undefined) {
    config.body = JSON.stringify(body)
  }

  try {
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

    return { success: false, error, status: response.status }
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
