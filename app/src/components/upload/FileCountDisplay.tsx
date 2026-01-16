interface FileCountDisplayProps {
  count: number
  limit: number
}

export default function FileCountDisplay({ count, limit }: FileCountDisplayProps): JSX.Element {
  const isOverLimit = count > limit

  return (
    <div
      data-testid="file-count-display"
      style={{
        display: 'flex',
        alignItems: 'center',
        gap: 'var(--space-2)',
        color: isOverLimit ? 'var(--color-warning-main)' : 'var(--color-text-muted)',
        fontSize: 'var(--font-size-body-small)',
      }}
    >
      <span style={{ fontWeight: 600 }}>{count}</span>
      <span>file{count !== 1 ? 's' : ''} selected</span>
      <span style={{ marginLeft: 'var(--space-1)' }}>(max {limit} per batch)</span>
    </div>
  )
}
