/**
 * Document types for the Clinical Intelligence application.
 */

export type DocumentStatus = 
  | 'Pending' 
  | 'Processing' 
  | 'Completed' 
  | 'Failed' 
  | 'ValidationFailed';

export interface Document {
  id: string;
  patientId: string;
  originalName: string;
  mimeType: string;
  sizeBytes: number;
  status: DocumentStatus;
  uploadedAt: string;
  storagePath: string;
}

export interface DocumentListResponse {
  items: Document[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DocumentStatusResult {
  documentId: string;
  status: DocumentStatus;
  statusChangedAt: string | null;
  errorMessage: string | null;
  processingTimeMs: number | null;
}
