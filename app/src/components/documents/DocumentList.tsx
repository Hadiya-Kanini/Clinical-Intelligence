import { useState } from 'react';
import { useDocuments } from '../../hooks/useDocuments';
import { DocumentListItem } from './DocumentListItem';
import { DocumentListPagination } from './DocumentListPagination';
import type { Document } from '../../types/document';

interface DocumentListProps {
  patientId?: string;
  onDocumentSelect?: (document: Document) => void;
}

export function DocumentList({ patientId, onDocumentSelect }: DocumentListProps): JSX.Element {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  
  const { documents, totalCount, totalPages, isLoading, error, refetch } = useDocuments({
    patientId,
    page,
    pageSize,
    pollInterval: 5000,
  });
  
  if (isLoading && documents.length === 0) {
    return (
      <div className="document-list document-list--loading" role="status" aria-live="polite">
        <span className="document-list__spinner" aria-hidden="true" />
        <span>Loading documents...</span>
      </div>
    );
  }
  
  if (error) {
    return (
      <div className="document-list document-list--error" role="alert">
        <p className="document-list__error-message">{error}</p>
        <button 
          type="button"
          onClick={refetch}
          className="document-list__retry-button"
        >
          Retry
        </button>
      </div>
    );
  }
  
  if (documents.length === 0) {
    return (
      <div className="document-list document-list--empty">
        <span className="document-list__empty-icon" aria-hidden="true">📁</span>
        <p className="document-list__empty-title">No documents found</p>
        <p className="document-list__empty-subtitle">
          Upload documents to get started
        </p>
      </div>
    );
  }
  
  return (
    <div className="document-list">
      <div className="document-list__header">
        <h2 className="document-list__title">Documents</h2>
        <p className="document-list__count">{totalCount} total documents</p>
      </div>
      
      <div className="document-list__table-container">
        <table className="document-list__table" role="grid" aria-label="Documents list">
          <thead>
            <tr>
              <th scope="col" className="document-list__header-cell">
                Document
              </th>
              <th scope="col" className="document-list__header-cell">
                Status
              </th>
              <th scope="col" className="document-list__header-cell">
                Uploaded
              </th>
            </tr>
          </thead>
          <tbody>
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
