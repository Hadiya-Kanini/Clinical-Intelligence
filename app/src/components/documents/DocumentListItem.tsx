import type { Document } from '../../types/document';
import { DocumentStatusBadge } from './DocumentStatusBadge';

interface DocumentListItemProps {
  document: Document;
  onSelect?: (document: Document) => void;
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function DocumentListItem({ document, onSelect }: DocumentListItemProps): JSX.Element {
  const isPdf = document.mimeType === 'application/pdf';
  
  const handleClick = () => {
    onSelect?.(document);
  };

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      onSelect?.(document);
    }
  };
  
  return (
    <tr 
      className="document-list-item"
      onClick={handleClick}
      onKeyDown={handleKeyDown}
      tabIndex={onSelect ? 0 : undefined}
      role={onSelect ? 'button' : undefined}
      aria-label={`Document: ${document.originalName}`}
    >
      <td className="document-list-item__cell document-list-item__cell--name">
        <div className="document-list-item__file">
          <span 
            className={`document-list-item__icon ${isPdf ? 'document-list-item__icon--pdf' : 'document-list-item__icon--doc'}`}
            aria-hidden="true"
          >
            {isPdf ? '📄' : '📝'}
          </span>
          <div className="document-list-item__details">
            <span className="document-list-item__filename">
              {document.originalName}
            </span>
            <span className="document-list-item__size">
              {formatFileSize(document.sizeBytes)}
            </span>
          </div>
        </div>
      </td>
      <td className="document-list-item__cell document-list-item__cell--status">
        <DocumentStatusBadge status={document.status} />
      </td>
      <td className="document-list-item__cell document-list-item__cell--date">
        {formatDate(document.uploadedAt)}
      </td>
    </tr>
  );
}
