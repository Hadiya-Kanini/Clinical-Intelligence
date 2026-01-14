import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import Alert from '../components/ui/Alert'
import Badge from '../components/ui/Badge'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Table from '../components/ui/Table'

type DocStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed'

type DocumentRow = {
  id: string
  name: string
  uploadedAt: string
  status: DocStatus
  patientId: string
}

function statusVariant(status: DocStatus): 'info' | 'warning' | 'success' | 'error' {
  if (status === 'Completed') return 'success'
  if (status === 'Failed') return 'error'
  if (status === 'Processing') return 'warning'
  return 'info'
}

export default function DocumentListPage(): JSX.Element {
  const [filter, setFilter] = useState<string>('')
  const [error, setError] = useState<string>('')

  const rows: DocumentRow[] = [
    {
      id: 'DOC-1001',
      name: 'Discharge Summary.pdf',
      uploadedAt: '2026-01-13 10:41',
      status: 'Completed',
      patientId: 'demo',
    },
    {
      id: 'DOC-1002',
      name: 'Lab Results.docx',
      uploadedAt: '2026-01-13 10:45',
      status: 'Processing',
      patientId: 'demo',
    },
    {
      id: 'DOC-1003',
      name: 'Radiology Report.pdf',
      uploadedAt: '2026-01-13 10:52',
      status: 'Failed',
      patientId: 'demo',
    },
  ]

  const filtered = useMemo(() => {
    const q = filter.trim().toLowerCase()
    if (!q) return rows
    return rows.filter((r) => r.name.toLowerCase().includes(q) || r.id.toLowerCase().includes(q))
  }, [filter])

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      {error ? <Alert>{error}</Alert> : null}

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
        {filtered.length === 0 ? (
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
                      {row.status === 'Completed' ? (
                        <Link className="ui-link" to={`/patients/${row.patientId}`}>
                          View Patient 360
                        </Link>
                      ) : null}
                      {row.status === 'Failed' ? (
                        <Button
                          variant="secondary"
                          onClick={() => {
                            setError('Retry is not wired yet. This is the wireframe UI.')
                            setTimeout(() => setError(''), 2000)
                          }}
                        >
                          Retry
                        </Button>
                      ) : null}
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
