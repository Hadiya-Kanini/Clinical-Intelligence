/**
 * Patient API client for patient 360 view and patient data operations.
 */

import api, { type ApiResult } from './apiClient'

export interface PatientDemographics {
  id: string
  mrn: string | null
  name: string | null
  dateOfBirth: string | null
  gender: string | null
  contact: string | null
  address: string | null
}

export interface EntityCitation {
  id: string
  documentId: string
  documentName: string | null
  pageNumber: number | null
  section: string | null
  sourceText: string | null
  coordinates: string | null
}

export type DataStatus = 'verified' | 'unverified' | 'modified'

export interface ExtractedEntity {
  id: string
  category: string
  displayCategory: string | null
  name: string
  value: string | null
  units: string | null
  confidenceScore: number | null
  isVerified: boolean
  effectiveAt: string | null
  rationale: string | null
  dataStatus: DataStatus
  citations: EntityCitation[]
}

export interface PatientDocument {
  id: string
  originalName: string
  status: string
  uploadedAt: string
  groundedEntityCount: number
}

export interface Patient360Response {
  patientId: string
  mrn: string | null
  name: string | null
  dob: string | null
  address: string | null
  contact: string | null
  entities: ExtractedEntity[]
  documents: PatientDocument[]
  generatedAt: string
}

/**
 * Get patient 360 view with demographics, extracted entities, and documents.
 */
export async function getPatient360(patientId: string): Promise<ApiResult<Patient360Response>> {
  return api.get<Patient360Response>(`/api/v1/patients/${patientId}/360`)
}

export default {
  getPatient360,
}
