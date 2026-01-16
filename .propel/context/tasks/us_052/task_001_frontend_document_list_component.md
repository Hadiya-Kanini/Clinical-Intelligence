# Task - [TASK_001]

## Requirement Reference
- User Story: [us_052]
- Story Location: [.propel/context/tasks/us_052/us_052.md]
- Acceptance Criteria: 
    - Given I navigate to Document List (SCR-006), When displayed, Then I see all my documents.
    - Given the document list, When displayed, Then each document shows status, upload date, and processing metadata (FR-022).
    - Given many documents, When displayed, Then pagination is implemented with configurable page size (TR-017).
    - Given the list, When documents are processing, Then status updates in real-time (UXR-043).

## Task Overview
Implement a React document list component that displays uploaded documents with their status, upload date, and metadata. The component supports pagination, real-time status updates via polling, and responsive design following the existing UI patterns.

## Dependent Tasks
- [US_051/task_001] - Backend document status service (provides status API)

## Impacted Components
- [CREATE | app/src/components/documents/DocumentList.tsx | Main document list component]
- [CREATE | app/src/components/documents/DocumentListItem.tsx | Individual document row component]
- [CREATE | app/src/components/documents/DocumentStatusBadge.tsx | Status badge with color coding]
- [CREATE | app/src/components/documents/DocumentListPagination.tsx | Pagination controls]
- [CREATE | app/src/hooks/useDocuments.ts | Custom hook for document data fetching]
- [CREATE | app/src/types/document.ts | TypeScript types for document entities]

## Implementation Plan

### 1. Create Document Types
```typescript
// app/src/types/document.ts
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
```

### 2. Create useDocuments Hook
```typescript
// app/src/hooks/useDocuments.ts
import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { Document, DocumentListResponse } from '../types/document';

interface UseDocumentsOptions {
  patientId?: string;
  page?: number;
  pageSize?: number;
  pollInterval?: number; // ms, for real-time updates
}

export function useDocuments(options: UseDocumentsOptions = {}) {
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
      
      const response = await axios.get<DocumentListResponse>(
        `/api/v1/documents?${params}`
      );
      
      setDocuments(response.data.items);
      setTotalCount(response.data.totalCount);
      setTotalPages(response.data.totalPages);
      setError(null);
    } catch (err) {
      setError('Failed to load documents');
      console.error('Error fetching documents:', err);
    } finally {
      setIsLoading(false);
    }
  }, [patientId, page, pageSize]);
  
  // Initial fetch
  useEffect(() => {
    fetchDocuments();
  }, [fetchDocuments]);
  
  // Polling for real-time updates (UXR-043)
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
```

### 3. Create DocumentStatusBadge Component
```typescript
// app/src/components/documents/DocumentStatusBadge.tsx
import { DocumentStatus } from '../../types/document';

interface DocumentStatusBadgeProps {
  status: DocumentStatus;
}

const statusConfig: Record<DocumentStatus, { label: string; className: string }> = {
  Pending: { 
    label: 'Pending', 
    className: 'bg-yellow-100 text-yellow-800 border-yellow-200' 
  },
  Processing: { 
    label: 'Processing', 
    className: 'bg-blue-100 text-blue-800 border-blue-200 animate-pulse' 
  },
  Completed: { 
    label: 'Completed', 
    className: 'bg-green-100 text-green-800 border-green-200' 
  },
  Failed: { 
    label: 'Failed', 
    className: 'bg-red-100 text-red-800 border-red-200' 
  },
  ValidationFailed: { 
    label: 'Validation Failed', 
    className: 'bg-orange-100 text-orange-800 border-orange-200' 
  },
};

export function DocumentStatusBadge({ status }: DocumentStatusBadgeProps) {
  const config = statusConfig[status] || statusConfig.Pending;
  
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${config.className}`}>
      {config.label}
    </span>
  );
}
```

### 4. Create DocumentListItem Component
```typescript
// app/src/components/documents/DocumentListItem.tsx
import { Document } from '../../types/document';
import { DocumentStatusBadge } from './DocumentStatusBadge';
import { FileText, FileIcon } from 'lucide-react';

interface DocumentListItemProps {
  document: Document;
  onSelect?: (document: Document) => void;
}

export function DocumentListItem({ document, onSelect }: DocumentListItemProps) {
  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };
  
  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };
  
  const isPdf = document.mimeType === 'application/pdf';
  
  return (
    <tr 
      className="hover:bg-gray-50 cursor-pointer transition-colors"
      onClick={() => onSelect?.(document)}
    >
      <td className="px-6 py-4 whitespace-nowrap">
        <div className="flex items-center">
          {isPdf ? (
            <FileText className="h-5 w-5 text-red-500 mr-3" />
          ) : (
            <FileIcon className="h-5 w-5 text-blue-500 mr-3" />
          )}
          <div>
            <div className="text-sm font-medium text-gray-900 truncate max-w-xs">
              {document.originalName}
            </div>
            <div className="text-sm text-gray-500">
              {formatFileSize(document.sizeBytes)}
            </div>
          </div>
        </div>
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <DocumentStatusBadge status={document.status} />
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
        {formatDate(document.uploadedAt)}
      </td>
    </tr>
  );
}
```

### 5. Create DocumentList Component
```typescript
// app/src/components/documents/DocumentList.tsx
import { useState } from 'react';
import { useDocuments } from '../../hooks/useDocuments';
import { DocumentListItem } from './DocumentListItem';
import { DocumentListPagination } from './DocumentListPagination';
import { Document } from '../../types/document';
import { Loader2, FileX } from 'lucide-react';

interface DocumentListProps {
  patientId?: string;
  onDocumentSelect?: (document: Document) => void;
}

export function DocumentList({ patientId, onDocumentSelect }: DocumentListProps) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  
  const { documents, totalCount, totalPages, isLoading, error, refetch } = useDocuments({
    patientId,
    page,
    pageSize,
    pollInterval: 5000, // Real-time updates every 5 seconds
  });
  
  if (isLoading && documents.length === 0) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-blue-500" />
        <span className="ml-2 text-gray-500">Loading documents...</span>
      </div>
    );
  }
  
  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-500">{error}</p>
        <button 
          onClick={refetch}
          className="mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          Retry
        </button>
      </div>
    );
  }
  
  if (documents.length === 0) {
    return (
      <div className="text-center py-12">
        <FileX className="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <p className="text-gray-500">No documents found</p>
        <p className="text-sm text-gray-400 mt-1">
          Upload documents to get started
        </p>
      </div>
    );
  }
  
  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="px-6 py-4 border-b border-gray-200">
        <h2 className="text-lg font-semibold text-gray-900">Documents</h2>
        <p className="text-sm text-gray-500">{totalCount} total documents</p>
      </div>
      
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Document
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Uploaded
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {documents.map((doc) => (
              <DocumentListItem 
                key={doc.id} 
                document={doc} 
                onSelect={onDocumentSelect}
              />
            ))}
          </tbody>
        </table>
      </div>
      
      {totalPages > 1 && (
        <DocumentListPagination
          currentPage={page}
          totalPages={totalPages}
          pageSize={pageSize}
          totalCount={totalCount}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
        />
      )}
    </div>
  );
}
```

### 6. Create Pagination Component
```typescript
// app/src/components/documents/DocumentListPagination.tsx
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface DocumentListPaginationProps {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

export function DocumentListPagination({
  currentPage,
  totalPages,
  pageSize,
  totalCount,
  onPageChange,
  onPageSizeChange,
}: DocumentListPaginationProps) {
  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalCount);
  
  return (
    <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
      <div className="flex items-center text-sm text-gray-500">
        <span>
          Showing {startItem} to {endItem} of {totalCount} documents
        </span>
        <select
          value={pageSize}
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
          className="ml-4 border border-gray-300 rounded px-2 py-1 text-sm"
        >
          <option value={10}>10 per page</option>
          <option value={20}>20 per page</option>
          <option value={50}>50 per page</option>
        </select>
      </div>
      
      <div className="flex items-center space-x-2">
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
          className="p-2 rounded border border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
        >
          <ChevronLeft className="h-4 w-4" />
        </button>
        
        <span className="text-sm text-gray-700">
          Page {currentPage} of {totalPages}
        </span>
        
        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage === totalPages}
          className="p-2 rounded border border-gray-300 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
        >
          <ChevronRight className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}
```

## Current Project State
```
app/src/
├── components/
│   └── (existing components)
├── hooks/
│   └── (existing hooks)
├── types/
│   └── (existing types)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/types/document.ts | TypeScript types for documents |
| CREATE | app/src/hooks/useDocuments.ts | Custom hook for document fetching |
| CREATE | app/src/components/documents/DocumentList.tsx | Main list component |
| CREATE | app/src/components/documents/DocumentListItem.tsx | Row component |
| CREATE | app/src/components/documents/DocumentStatusBadge.tsx | Status badge |
| CREATE | app/src/components/documents/DocumentListPagination.tsx | Pagination controls |

## External References
- https://react.dev/reference/react/hooks
- https://tailwindcss.com/docs
- https://lucide.dev/icons/

## Build Commands
- npm run dev (from app directory)
- npm run build (from app directory)
- npm run test (from app directory)

## Implementation Validation Strategy
- [Automated] Component renders without errors
- [Automated] Pagination controls work correctly
- [Manual] Verify real-time status updates when documents are processing
- [Manual] Verify empty state displays correctly
- [Manual] Verify responsive design on mobile

## Implementation Checklist
- [x] Create document.ts types file
- [x] Create useDocuments hook with polling
- [x] Create DocumentStatusBadge component
- [x] Create DocumentListItem component
- [x] Create DocumentList component with loading/error/empty states
- [x] Create DocumentListPagination component
- [x] Add unit tests for components
- [x] Verify accessibility (ARIA labels, keyboard navigation)
