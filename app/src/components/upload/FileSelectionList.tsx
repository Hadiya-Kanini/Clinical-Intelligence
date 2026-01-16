interface FileSelectionListProps {
  files: File[]
  limit: number
  uploadResults?: FileUploadResult[]
}

export interface FileUploadResult {
  fileName: string
  documentId?: string
  isAccepted: boolean
  status: string
  validationErrors: string[]
  rejectionReason?: string
}

function formatFileSize(bytes: number): string {
  if (!Number.isFinite(bytes) || bytes <= 0) return '0 B'
  const units = ['B', 'KB', 'MB', 'GB']
  const idx = Math.min(units.length - 1, Math.floor(Math.log(bytes) / Math.log(1024)))
  const value = bytes / Math.pow(1024, idx)
  return `${value.toFixed(idx === 0 ? 0 : 1)} ${units[idx]}`
}

function getStatusBadge(status: string, isAccepted: boolean): JSX.Element | null {
  if (status === 'BatchLimitExceeded') {
    return (
      <span
        style={{
          padding: 'var(--space-1) var(--space-2)',
          backgroundColor: 'var(--color-neutral-100)',
          color: 'var(--color-text-muted)',
          borderRadius: 'var(--radius-sm)',
          fontSize: 'var(--font-size-body-small)',
        }}
      >
        Skipped (batch limit)
      </span>
    )
  }

  if (isAccepted) {
    return (
      <span
        style={{
          padding: 'var(--space-1) var(--space-2)',
          backgroundColor: 'var(--color-success-light)',
          color: 'var(--color-success-dark)',
          borderRadius: 'var(--radius-sm)',
          fontSize: 'var(--font-size-body-small)',
        }}
      >
        Accepted
      </span>
    )
  }

  return (
    <span
      style={{
        padding: 'var(--space-1) var(--space-2)',
        backgroundColor: 'var(--color-error-light)',
        color: 'var(--color-error-dark)',
        borderRadius: 'var(--radius-sm)',
        fontSize: 'var(--font-size-body-small)',
      }}
    >
      Rejected
    </span>
  )
}

export default function FileSelectionList({
  files,
  limit,
  uploadResults,
}: FileSelectionListProps): JSX.Element {
  return (
    <div
      data-testid="file-selection-list"
      style={{
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--space-2)',
        marginTop: 'var(--space-4)',
      }}
    >
      {files.map((file, index) => {
        const isWithinLimit = index < limit
        const result = uploadResults?.find((r) => r.fileName === file.name)

        return (
          <div
            key={`${file.name}-${index}`}
            data-testid="file-selection-item"
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              padding: 'var(--space-3)',
              borderRadius: 'var(--radius-md)',
              border: '1px solid var(--color-border)',
              backgroundColor: isWithinLimit ? 'var(--color-neutral-0)' : 'var(--color-neutral-50)',
              opacity: isWithinLimit ? 1 : 0.6,
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
              <svg
                width="20"
                height="20"
                viewBox="0 0 20 20"
                fill="none"
                style={{ flexShrink: 0, color: 'var(--color-text-muted)' }}
                aria-hidden="true"
              >
                <path
                  d="M11.6667 1.66669H5.00004C4.55801 1.66669 4.13409 1.84228 3.82153 2.15484C3.50897 2.4674 3.33337 2.89133 3.33337 3.33335V16.6667C3.33337 17.1087 3.50897 17.5326 3.82153 17.8452C4.13409 18.1578 4.55801 18.3334 5.00004 18.3334H15C15.442 18.3334 15.866 18.1578 16.1785 17.8452C16.4911 17.5326 16.6667 17.1087 16.6667 16.6667V6.66669L11.6667 1.66669Z"
                  stroke="currentColor"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
                <path
                  d="M11.6666 1.66669V6.66669H16.6666"
                  stroke="currentColor"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
              <div>
                <div style={{ fontWeight: 500, color: 'var(--color-text-primary)' }}>
                  {file.name}
                </div>
                <div style={{ fontSize: 'var(--font-size-body-small)', color: 'var(--color-text-muted)' }}>
                  {formatFileSize(file.size)}
                </div>
              </div>
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
              {!isWithinLimit && !result && (
                <span
                  style={{
                    fontSize: 'var(--font-size-body-small)',
                    color: 'var(--color-text-muted)',
                  }}
                >
                  Will not be uploaded
                </span>
              )}
              {result && getStatusBadge(result.status, result.isAccepted)}
            </div>
          </div>
        )
      })}
    </div>
  )
}
