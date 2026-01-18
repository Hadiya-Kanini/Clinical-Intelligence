/**
 * Patients API client for patient management operations.
 */

import api, { type ApiResult } from './apiClient'

export interface Patient {
  id: string
  mrn: string | null
  name: string | null
  dateOfBirth: string | null
  gender: string | null
  contact: string | null
  address: string | null
  documentCount: number
  lastDocumentUploadedAt: string | null
}

export interface PatientsResponse {
  patients: Patient[]
  totalCount: number
  page: number
  pageSize: number
}

/**
 * Get paginated list of patients with document counts.
 */
export async function getPatients(
  page: number = 1,
  pageSize: number = 20,
  search?: string
): Promise<ApiResult<PatientsResponse>> {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  })
  
  if (search) {
    params.append('search', search)
  }

  return api.get<PatientsResponse>(`/api/v1/patients/dashboard?${params}`)
}

export default {
  getPatients,
}
