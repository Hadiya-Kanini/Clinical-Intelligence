import type { DocumentStatus } from '../../types/document';

interface DocumentStatusBadgeProps {
  status: DocumentStatus;
}

const statusConfig: Record<DocumentStatus, { label: string; className: string }> = {
  Pending: { 
    label: 'Pending', 
    className: 'document-status-badge document-status-badge--pending' 
  },
  Processing: { 
    label: 'Processing', 
    className: 'document-status-badge document-status-badge--processing' 
  },
  Completed: { 
    label: 'Completed', 
    className: 'document-status-badge document-status-badge--completed' 
  },
  Failed: { 
    label: 'Failed', 
    className: 'document-status-badge document-status-badge--failed' 
  },
  ValidationFailed: { 
    label: 'Validation Failed', 
    className: 'document-status-badge document-status-badge--validation-failed' 
  },
};

export function DocumentStatusBadge({ status }: DocumentStatusBadgeProps): JSX.Element {
  const config = statusConfig[status] || statusConfig.Pending;
  
  return (
    <span 
      className={config.className}
      role="status"
      aria-label={`Document status: ${config.label}`}
    >
      {config.label}
    </span>
  );
}
