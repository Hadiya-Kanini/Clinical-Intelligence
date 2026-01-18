import { useMemo, useState } from 'react'
import Alert from '../components/ui/Alert'
import Badge from '../components/ui/Badge'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Table from '../components/ui/Table'
import { type DocumentListItem } from '../lib/documentApi'
import { useDocumentListPolling } from '../hooks/useDocumentListPolling'

type DocumentRow = {
  id: string
  name: string
  uploadedAt: string
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed'
  patientId: string
  // Processing metadata (US_056)
  processingTimeMs?: number | null
  startedAt?: string | null
  completedAt?: string | null
  errorMessage?: string | null
  retryCount?: number | null
}

function statusVariant(status: DocumentListItem['status']): 'info' | 'warning' | 'success' | 'error' {
  if (status === 'Completed') return 'success'
  if (status === 'Failed') return 'error'
  if (status === 'Processing') return 'warning'
  return 'info'
}

function formatProcessingTime(ms: number | null | undefined): string {
  if (ms == null) return '-'
  if (ms < 1000) return `${ms}ms`
  const seconds = (ms / 1000).toFixed(1)
  return `${seconds}s`
}

function formatTimestamp(isoString: string | null | undefined): string {
  if (!isoString) return '-'
  try {
    return new Date(isoString).toLocaleString()
  } catch {
    return '-'
  }
}

function formatLastUpdated(date: Date | null): string {
  if (!date) return ''
  return `Last updated: ${date.toLocaleTimeString()}`
}

export default function DocumentListPage(): JSX.Element {
  const [filter, setFilter] = useState<string>('')

  // Use polling hook for real-time updates (US_057)
  const {
    items: documents,
    loading,
    error,
    isRefreshing,
    lastUpdatedAt,
    refreshNow,
  } = useDocumentListPolling({
    enabled: true,
    search: filter,
  })

  const rows = documents.map((doc): DocumentRow => ({
    id: doc.id,
    name: doc.fileName,
    uploadedAt: new Date(doc.uploadedAt).toLocaleString(),
    status: doc.status,
    patientId: doc.patientId,
    // Processing metadata (US_056)
    processingTimeMs: doc.processingTimeMs,
    startedAt: doc.startedAt,
    completedAt: doc.completedAt,
    errorMessage: doc.errorMessage,
    retryCount: doc.retryCount,
  }))

  const filtered = useMemo(() => {
    const q = filter.trim().toLowerCase()
    if (!q) return rows
    return rows.filter((r) => r.name.toLowerCase().includes(q) || r.id.toLowerCase().includes(q))
  }, [filter, rows])

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      {error ? <Alert variant="error">{error}</Alert> : null}

      <Card
        title="Document list"
        headerRight={
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
            {lastUpdatedAt && (
              <span style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>
                {formatLastUpdated(lastUpdatedAt)}
              </span>
            )}
            <Button 
              variant="secondary" 
              onClick={() => refreshNow()} 
              disabled={isRefreshing}
            >
              {isRefreshing ? 'Refreshing...' : 'Refresh'}
            </Button>
            <input
              value={filter}
              onChange={(e) => setFilter(e.target.value)}
              placeholder="Search documents"
              className="ui-textfield__input"
              style={{ width: 260 }}
            />
            <Button variant="secondary" onClick={() => setFilter('')} disabled={!filter.trim()}>
              Clear
            </Button>
          </div>
        }
      >
        {loading ? (
          <div style={{ textAlign: 'center', padding: 'var(--space-6)' }}>Loading documents...</div>
        ) : filtered.length === 0 ? (
          <Alert variant="info">No documents found. Upload your first document to get started.</Alert>
        ) : (
          <Table>
            <thead>
              <tr>
                <th>Document</th>
                <th>Uploaded</th>
                <th>Status</th>
                <th>Processing</th>
                <th style={{ width: 180 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((row) => (
                <tr key={row.id}>
                  <td>
                    <div style={{ fontWeight: 600 }}>{row.name}</div>
                    <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>{row.id}</div>
                    {row.status === 'Failed' && row.errorMessage && (
                      <div 
                        style={{ 
                          color: 'var(--color-error)', 
                          fontSize: 'var(--font-size-body-small)',
                          marginTop: 'var(--space-1)',
                          maxWidth: 300,
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap'
                        }}
                        title={row.errorMessage}
                      >
                        {row.errorMessage}
                      </div>
                    )}
                  </td>
                  <td>{row.uploadedAt}</td>
                  <td>
                    <Badge variant={statusVariant(row.status)}>{row.status}</Badge>
                    {row.retryCount != null && row.retryCount > 0 && (
                      <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)', marginTop: 'var(--space-1)' }}>
                        Retries: {row.retryCount}
                      </div>
                    )}
                  </td>
                  <td>
                    <div style={{ fontSize: 'var(--font-size-body-small)' }}>
                      <div>Duration: {formatProcessingTime(row.processingTimeMs)}</div>
                      {row.startedAt && (
                        <div style={{ color: 'var(--color-text-muted)' }}>
                          Started: {formatTimestamp(row.startedAt)}
                        </div>
                      )}
                      {row.completedAt && (
                        <div style={{ color: 'var(--color-text-muted)' }}>
                          Completed: {formatTimestamp(row.completedAt)}
                        </div>
                      )}
                    </div>
                  </td>
                  <td>
                    <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                      <Button variant="secondary">
                        View
                      </Button>
                      <Button variant="secondary">
                        Download
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        )}
      </Card>
    </div>
  )
}
