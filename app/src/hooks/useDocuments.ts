import { useState, useEffect, useCallback } from 'react';
import type { Document, DocumentListResponse } from '../types/document';

interface UseDocumentsOptions {
  patientId?: string;
  page?: number;
  pageSize?: number;
  pollInterval?: number;
}

interface UseDocumentsResult {
  documents: Document[];
  totalCount: number;
  totalPages: number;
  isLoading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function useDocuments(options: UseDocumentsOptions = {}): UseDocumentsResult {
  const { 
    patientId, 
    page = 1, 
    pageSize = 20,
    pollInterval = 5000 
  } = options;
  
  const [documents, setDocuments] = useState<Document[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const fetchDocuments = useCallback(async () => {
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });
      if (patientId) params.append('patientId', patientId);
      
      const response = await fetch(`/api/v1/documents?${params}`, {
        method: 'GET',
        credentials: 'include',
      });
      
      if (!response.ok) {
        throw new Error(`Failed to fetch documents: ${response.status}`);
      }
      
      const data: DocumentListResponse = await response.json();
      
      setDocuments(data.items);
      setTotalCount(data.totalCount);
      setTotalPages(data.totalPages);
      setError(null);
    } catch (err) {
      setError('Failed to load documents');
      console.error('Error fetching documents:', err);
    } finally {
      setIsLoading(false);
    }
  }, [patientId, page, pageSize]);
  
  useEffect(() => {
    fetchDocuments();
  }, [fetchDocuments]);
  
  useEffect(() => {
    const hasProcessingDocs = documents.some(
      d => d.status === 'Pending' || d.status === 'Processing'
    );
    
    if (!hasProcessingDocs || pollInterval <= 0) return;
    
    const interval = setInterval(fetchDocuments, pollInterval);
    return () => clearInterval(interval);
  }, [documents, pollInterval, fetchDocuments]);
  
  return {
    documents,
    totalCount,
    totalPages,
    isLoading,
    error,
    refetch: fetchDocuments,
  };
}
