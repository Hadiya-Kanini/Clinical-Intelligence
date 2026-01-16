interface BatchLimitWarningProps {
  totalSelected: number
  limit: number
  excessCount: number
}

export default function BatchLimitWarning({
  totalSelected,
  limit,
  excessCount,
}: BatchLimitWarningProps): JSX.Element {
  return (
    <div
      data-testid="batch-limit-warning"
      role="alert"
      style={{
        display: 'flex',
        alignItems: 'flex-start',
        gap: 'var(--space-3)',
        padding: 'var(--space-4)',
        backgroundColor: 'var(--color-warning-light)',
        border: '1px solid var(--color-warning-main)',
        borderRadius: 'var(--radius-md)',
        marginTop: 'var(--space-4)',
      }}
    >
      <svg
        width="20"
        height="20"
        viewBox="0 0 20 20"
        fill="none"
        style={{ flexShrink: 0, marginTop: '2px' }}
        aria-hidden="true"
      >
        <path
          d="M8.57465 3.21665L1.51632 14.1667C1.37079 14.4187 1.29379 14.7044 1.29298 14.9954C1.29216 15.2864 1.36756 15.5725 1.51167 15.8254C1.65579 16.0782 1.86359 16.2891 2.11423 16.4371C2.36487 16.585 2.64972 16.6647 2.94048 16.6683H17.0588C17.3496 16.6647 17.6344 16.585 17.8851 16.4371C18.1357 16.2891 18.3435 16.0782 18.4876 15.8254C18.6317 15.5725 18.7071 15.2864 18.7063 14.9954C18.7055 14.7044 18.6285 14.4187 18.483 14.1667L11.4247 3.21665C11.2761 2.97174 11.0669 2.76925 10.8171 2.62871C10.5672 2.48817 10.2851 2.41431 9.99798 2.41431C9.71089 2.41431 9.42876 2.48817 9.17892 2.62871C8.92908 2.76925 8.71986 2.97174 8.57132 3.21665H8.57465Z"
          stroke="var(--color-warning-main)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M10 7.5V10.8333"
          stroke="var(--color-warning-main)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M10 14.1667H10.0083"
          stroke="var(--color-warning-main)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </svg>
      <div>
        <div style={{ fontWeight: 600, color: 'var(--color-warning-dark)' }}>
          Batch limit exceeded
        </div>
        <p
          style={{
            margin: 'var(--space-1) 0 0 0',
            color: 'var(--color-warning-dark)',
            fontSize: 'var(--font-size-body-small)',
          }}
        >
          You selected {totalSelected} files, but only {limit} files can be uploaded per batch. The
          first {limit} files will be uploaded. The remaining {excessCount} file
          {excessCount !== 1 ? 's' : ''} will need to be uploaded in a separate batch.
        </p>
      </div>
    </div>
  )
}
