/**
 * Dashboard API for fetching statistics and metrics.
 */

import api, { type ApiResult } from './apiClient'

export interface DashboardStats {
  uploadsToday: number
  processing: number
  conflicts: number
  exportsLast7Days: number
}

/**
 * Fetch dashboard statistics.
 * Requires authentication.
 */
export async function getDashboardStats(): Promise<ApiResult<DashboardStats>> {
  return api.get<DashboardStats>('/api/v1/dashboard/stats')
}

export const dashboardApi = {
  getDashboardStats,
}

export default dashboardApi
