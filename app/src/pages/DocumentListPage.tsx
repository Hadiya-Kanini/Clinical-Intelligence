import { useEffect, useMemo, useState } from 'react'
import Alert from '../components/ui/Alert'
import Badge from '../components/ui/Badge'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Table from '../components/ui/Table'
import { listDocuments, type DocumentListItem } from '../lib/documentApi'

type DocumentRow = {
  id: string
  name: string
  uploadedAt: string
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed'
  patientId: string
}

function statusVariant(status: DocumentListItem['status']): 'info' | 'warning' | 'success' | 'error' {
  if (status === 'Completed') return 'success'
  if (status === 'Failed') return 'error'
  if (status === 'Processing') return 'warning'
  return 'info'
}

export default function DocumentListPage(): JSX.Element {
  const [filter, setFilter] = useState<string>('')
  const [error, setError] = useState<string>('')
  const [loading, setLoading] = useState(true)
  const [documents, setDocuments] = useState<DocumentListItem[]>([])
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const pageSize = 20

  useEffect(() => {
    async function fetchDocuments() {
      try {
        setLoading(true)
        setError('')
        
        const result = await listDocuments(page, pageSize, filter)
        
        if (result.success) {
          setDocuments(result.data.items)
          setTotal(result.data.total)
        } else {
          setError(result.error.message || 'Failed to load documents')
        }
      } catch (err) {
        setError('Network error. Please try again.')
      } finally {
        setLoading(false)
      }
    }

    fetchDocuments()
  }, [page, filter])

  const rows = documents.map((doc): DocumentRow => ({
    id: doc.id,
    name: doc.fileName,
    uploadedAt: new Date(doc.uploadedAt).toLocaleString(),
    status: doc.status,
    patientId: doc.patientId,
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
          <div style={{ display: 'flex', gap: 'var(--space-3)' }}>
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
                <th style={{ width: 220 }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((row) => (
                <tr key={row.id}>
                  <td>
                    <div style={{ fontWeight: 600 }}>{row.name}</div>
                    <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>{row.id}</div>
                  </td>
                  <td>{row.uploadedAt}</td>
                  <td>
                    <Badge variant={statusVariant(row.status)}>{row.status}</Badge>
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
