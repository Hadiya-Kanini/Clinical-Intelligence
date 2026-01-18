import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Alert from '../components/ui/Alert'
import Badge from '../components/ui/Badge'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Table from '../components/ui/Table'
import { getPatients, type Patient } from '../lib/patientsApi'

export default function PatientsPage(): JSX.Element {
  const navigate = useNavigate()
  const [patients, setPatients] = useState<Patient[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)

  const pageSize = 20

  useEffect(() => {
    async function fetchPatients() {
      try {
        setLoading(true)
        setError(null)
        const result = await getPatients(page, pageSize, search || undefined)
        
        if (result.success) {
          setPatients(result.data.patients)
          setTotalCount(result.data.totalCount)
        } else {
          setError(result.error.message || 'Failed to load patients')
        }
      } catch (err) {
        setError('Network error. Please try again.')
      } finally {
        setLoading(false)
      }
    }

    fetchPatients()
  }, [page, search])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setPage(1) // Reset to first page when searching
  }

  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'N/A'
    return new Date(dateString).toLocaleDateString()
  }

  const totalPages = Math.ceil(totalCount / pageSize)

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      <div>
        <h1
          style={{
            fontSize: 'var(--font-size-h2)',
            fontWeight: 'var(--font-weight-h2)',
            lineHeight: 'var(--line-height-h2)',
            margin: '0 0 var(--space-2) 0',
          }}
        >
          Patients
        </h1>
        <p style={{ margin: 0, color: 'var(--color-text-muted)' }}>
          View all patients and access their 360-degree clinical data.
        </p>
      </div>

      {error && <Alert variant="error">{error}</Alert>}

      <Card title="Search Patients">
        <form onSubmit={handleSearch} style={{ display: 'flex', gap: 'var(--space-3)', alignItems: 'end' }}>
          <div style={{ flex: 1 }}>
            <label htmlFor="search" style={{ display: 'block', marginBottom: 'var(--space-1)', fontSize: 'var(--font-size-body-small)' }}>
              Search by name or MRN
            </label>
            <input
              id="search"
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Enter patient name or MRN..."
              style={{
                width: '100%',
                padding: 'var(--space-2)',
                border: '1px solid var(--color-border)',
                borderRadius: 'var(--radius-sm)',
                fontSize: 'var(--font-size-body)',
              }}
            />
          </div>
          <Button type="submit">Search</Button>
          <Button variant="secondary" type="button" onClick={() => { setSearch(''); setPage(1) }}>
            Clear
          </Button>
        </form>
      </Card>

      <Card title={`Patients (${totalCount})`}>
        {loading ? (
          <div style={{ textAlign: 'center', padding: 'var(--space-6)' }}>Loading patients...</div>
        ) : patients.length === 0 ? (
          <div style={{ textAlign: 'center', padding: 'var(--space-6)' }}>
            <p style={{ color: 'var(--color-text-muted)' }}>
              {search ? 'No patients found matching your search.' : 'No patients found. Upload documents to get started.'}
            </p>
            {!search && (
              <Button onClick={() => navigate('/documents/upload')} style={{ marginTop: 'var(--space-3)' }}>
                Upload Documents
              </Button>
            )}
          </div>
        ) : (
          <>
            <Table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>MRN</th>
                  <th>DOB</th>
                  <th>Contact</th>
                  <th>Documents</th>
                  <th>Last Upload</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {patients.map((patient) => (
                  <tr key={patient.id}>
                    <td style={{ fontWeight: 600 }}>
                      {patient.name || 'Unknown'}
                    </td>
                    <td>{patient.mrn || 'N/A'}</td>
                    <td>{formatDate(patient.dateOfBirth)}</td>
                    <td>{patient.contact || 'N/A'}</td>
                    <td>
                      <Badge variant={patient.documentCount > 0 ? 'info' : 'neutral'}>
                        {patient.documentCount}
                      </Badge>
                    </td>
                    <td>{formatDate(patient.lastDocumentUploadedAt)}</td>
                    <td>
                      <Button
                        onClick={() => navigate(`/patients/${patient.id}`)}
                        disabled={patient.documentCount === 0}
                      >
                        360 View
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>

            {totalPages > 1 && (
              <div style={{ display: 'flex', justifyContent: 'center', gap: 'var(--space-2)', marginTop: 'var(--space-4)' }}>
                <Button
                  variant="secondary"
                  onClick={() => setPage(p => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <span style={{ display: 'flex', alignItems: 'center', fontSize: 'var(--font-size-body-small)' }}>
                  Page {page} of {totalPages}
                </span>
                <Button
                  variant="secondary"
                  onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                >
                  Next
                </Button>
              </div>
            )}
          </>
        )}
      </Card>
    </div>
  )
}
