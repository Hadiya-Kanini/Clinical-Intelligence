/**
 * API wrapper for admin users management endpoints.
 * Provides typed access to the admin users list, create, update, and status toggle endpoints.
 */

import api, { type ApiResult } from './apiClient'

/**
 * Query parameters for listing users.
 */
export interface AdminUsersListQuery {
  q?: string
  sortBy?: 'name' | 'email' | 'role' | 'status'
  sortDir?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

/**
 * Individual user item in the admin users list.
 */
export interface AdminUserItem {
  id: string
  name: string
  email: string
  role: 'admin' | 'standard'
  status: 'active' | 'inactive' | 'locked'
}

/**
 * Response from the admin users list endpoint.
 */
export interface AdminUsersListResponse {
  items: AdminUserItem[]
  page: number
  pageSize: number
  total: number
}

/**
 * Request payload for creating a new user.
 */
export interface CreateUserRequest {
  name: string
  email: string
  role?: 'admin' | 'standard'
}

/**
 * Response from the create user endpoint.
 */
export interface CreateUserResponse {
  id: string
  name: string
  email: string
  role: 'admin' | 'standard'
  status: 'active'
  credentials_email_sent: boolean
  credentials_email_error_code?: string
}

/**
 * Request payload for updating an existing user.
 */
export interface UpdateUserRequest {
  name: string
  email: string
  role: 'admin' | 'standard'
}

/**
 * Build query string from parameters.
 */
function buildQueryString(params: AdminUsersListQuery): string {
  const searchParams = new URLSearchParams()
  
  if (params.q) {
    searchParams.set('q', params.q)
  }
  if (params.sortBy) {
    searchParams.set('sortBy', params.sortBy)
  }
  if (params.sortDir) {
    searchParams.set('sortDir', params.sortDir)
  }
  if (params.page !== undefined) {
    searchParams.set('page', params.page.toString())
  }
  if (params.pageSize !== undefined) {
    searchParams.set('pageSize', params.pageSize.toString())
  }
  
  const queryString = searchParams.toString()
  return queryString ? `?${queryString}` : ''
}

/**
 * Fetch paginated list of users with search, sort, and pagination.
 * Requires admin authentication.
 */
export async function listUsers(
  params: AdminUsersListQuery = {}
): Promise<ApiResult<AdminUsersListResponse>> {
  const queryString = buildQueryString(params)
  return api.get<AdminUsersListResponse>(`/api/v1/admin/users${queryString}`)
}

/**
 * Create a new user with credentials email.
 * Requires admin authentication.
 */
export async function createUser(
  request: CreateUserRequest
): Promise<ApiResult<CreateUserResponse>> {
  return api.post<CreateUserResponse>('/api/v1/admin/users', request)
}

/**
 * Update an existing user.
 * Requires admin authentication.
 */
export async function updateUser(
  userId: string,
  request: UpdateUserRequest
): Promise<ApiResult<AdminUserItem>> {
  return api.put<AdminUserItem>(`/api/v1/admin/users/${userId}`, request)
}

/**
 * Toggle user status between active and inactive.
 * Requires admin authentication.
 */
export async function toggleUserStatus(
  userId: string
): Promise<ApiResult<AdminUserItem>> {
  return api.patch<AdminUserItem>(`/api/v1/admin/users/${userId}/toggle-status`)
}

export const adminUsersApi = {
  listUsers,
  createUser,
  updateUser,
  toggleUserStatus,
}

export default adminUsersApi
