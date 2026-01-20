import { useEffect, useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import Alert from '../components/ui/Alert'
import Badge from '../components/ui/Badge'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Modal from '../components/ui/Modal'
import Table from '../components/ui/Table'
import { getPatient360, type Patient360Response } from '../lib/patientApi'
import Patient360View from '../components/Patient360View'

type SourceCitation = {
  documentId: string
  documentName: string
  pageNumber: number
  section?: string
  sourceText: string
}

type Conflict = {
  id: string
  field: string
  leftValue: string
  rightValue: string
  leftCitation: SourceCitation
  rightCitation: SourceCitation
}

type CodeSuggestion = {
  id: string
  code: string
  description: string
  status: 'pending' | 'accepted' | 'rejected'
  citation: SourceCitation
}

export default function Patient360Page(): JSX.Element {
  const { patientId } = useParams()

  const [activeTab, setActiveTab] = useState<'overview' | 'codes'>('overview')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [patientData, setPatientData] = useState<Patient360Response | null>(null)
  const [conflicts, setConflicts] = useState<Conflict[]>([])

  const [codes, setCodes] = useState<CodeSuggestion[]>([])

  // Fetch patient data on mount
  useEffect(() => {
    async function fetchPatientData() {
      if (!patientId) {
        setError('No patient ID provided')
        setLoading(false)
        return
      }

      setLoading(true)
      setError(null)

      const result = await getPatient360(patientId)

      if (result.success) {
        setPatientData(result.data)
        setLoading(false)
      } else {
        setError(result.error.message)
        setLoading(false)
      }
    }

    fetchPatientData()
  }, [patientId])

  const [conflictModalOpen, setConflictModalOpen] = useState(false)
  const [selectedConflict, setSelectedConflict] = useState<Conflict | null>(null)
  const [selectedResolution, setSelectedResolution] = useState<'left' | 'right' | ''>('')

  const exportBlocked = conflicts.length > 0
  const conflictCount = conflicts.length
  const pendingCodeCount = useMemo(() => codes.filter((c) => c.status === 'pending').length, [codes])

  function openResolve(conflict: Conflict): void {
    setSelectedConflict(conflict)
    setSelectedResolution('')
    setConflictModalOpen(true)
  }

  function resolveSelected(): void {
    if (!selectedConflict) return
    if (!selectedResolution) return

    setConflicts((current) => current.filter((c) => c.id !== selectedConflict.id))
    setConflictModalOpen(false)
  }

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
        <div>Loading patient data...</div>
      </div>
    )
  }

  if (error) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
        <Alert variant="error">{error}</Alert>
      </div>
    )
  }

  if (!patientData) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
        <Alert variant="error">Patient not found</Alert>
      </div>
    )
  }

  const renderRightPane = () => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ display: 'flex', gap: 'var(--space-2)', alignItems: 'center' }}>
          {exportBlocked ? (
            <Badge variant="warning">{conflictCount} conflict(s) must be resolved</Badge>
          ) : (
            <Badge variant="success">All conflicts resolved</Badge>
          )}
        </div>
      </div>

      <Card
        title="Patient profile"
        headerRight={
          <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
            <Badge variant={exportBlocked ? 'warning' : 'success'}>{exportBlocked ? 'Conflicts' : 'Verified'}</Badge>
            <Badge variant="info">Pending codes: {pendingCodeCount}</Badge>
          </div>
        }
      >
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)' }}>
          <div>
            <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>MRN</div>
            <div style={{ fontWeight: 600 }}>{patientData.mrn || 'N/A'}</div>
          </div>
          <div>
            <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>DOB</div>
            <div style={{ fontWeight: 600 }}>{patientData.dob || 'N/A'}</div>
          </div>
          <div>
            <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>Name</div>
            <div style={{ fontWeight: 600 }}>{patientData.name || 'N/A'}</div>
          </div>
          <div>
            <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>Gender</div>
            <div style={{ fontWeight: 600 }}>{'N/A'}</div>
          </div>
        </div>
      </Card>

      <Card
        title="Review"
        headerRight={
          <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
            <button
              type="button"
              className={`ui-shell__navLink${activeTab === 'overview' ? ' is-active' : ''}`}
              style={{ border: 0, background: 'transparent', cursor: 'pointer' }}
              onClick={() => setActiveTab('overview')}
            >
              Data
            </button>
            <button
              type="button"
              className={`ui-shell__navLink${activeTab === 'codes' ? ' is-active' : ''}`}
              style={{ border: 0, background: 'transparent', cursor: 'pointer' }}
              onClick={() => setActiveTab('codes')}
            >
              Codes
            </button>
          </div>
        }
      >
            {activeTab === 'overview' ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
                {/* Patient Profile Section */}
                <Card title="Patient Profile">
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 'var(--space-4)' }}>
                    <div>
                      <h4 style={{ margin: '0 0 var(--space-2) 0', fontSize: 'var(--font-size-body-large)', fontWeight: 600 }}>Demographics</h4>
                      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-1)' }}>
                        <div><strong>Name:</strong> {patientData.name || 'N/A'}</div>
                        <div><strong>MRN:</strong> {patientData.mrn || 'N/A'}</div>
                        <div><strong>DOB:</strong> {patientData.dob || 'N/A'}</div>
                        <div><strong>Contact:</strong> {patientData.contact || 'N/A'}</div>
                        <div><strong>Address:</strong> {patientData.address || 'N/A'}</div>
                      </div>
                    </div>
                    <div>
                      <h4 style={{ margin: '0 0 var(--space-2) 0', fontSize: 'var(--font-size-body-large)', fontWeight: 600 }}>Document Information</h4>
                      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-1)' }}>
                        <div><strong>Total Documents:</strong> {patientData.documents.length}</div>
                        <div><strong>Last Upload:</strong> {patientData.documents.length > 0 ? new Date(patientData.documents[0].uploadedAt).toLocaleDateString() : 'N/A'}</div>
                      </div>
                    </div>
                  </div>
                </Card>

                {/* Clinical Content Sections - Using our new Patient360View component */}
                <Patient360View patientId={patientId!} />

                {/* Conflicts Section */}
                <Card title="Conflicts">
                  {conflicts.length === 0 ? (
                    <Alert variant="success">No conflicts detected.</Alert>
                  ) : (
                    <Table>
                      <thead>
                        <tr>
                          <th>Field</th>
                          <th>Values</th>
                          <th style={{ width: 160 }}>Action</th>
                        </tr>
                      </thead>
                      <tbody>
                        {conflicts.map((c) => (
                          <tr key={c.id}>
                            <td>{c.field}</td>
                            <td>
                              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-3)' }}>
                                <div style={{ padding: 'var(--space-2)', border: '1px solid var(--color-border)', borderRadius: 'var(--radius-md)' }}>
                                  <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>
                                    {c.leftCitation.documentName} (p.{c.leftCitation.pageNumber})
                                  </div>
                                  <div style={{ fontWeight: 600 }}>{c.leftValue}</div>
                                  <div style={{ fontSize: 'var(--font-size-body-small)', fontStyle: 'italic', marginTop: 'var(--space-1)' }}>
                                    "{c.leftCitation.sourceText}"
                                  </div>
                                </div>
                                <div style={{ padding: 'var(--space-2)', border: '1px solid var(--color-border)', borderRadius: 'var(--radius-md)' }}>
                                  <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>
                                    {c.rightCitation.documentName} (p.{c.rightCitation.pageNumber})
                                  </div>
                                  <div style={{ fontWeight: 600 }}>{c.rightValue}</div>
                                  <div style={{ fontSize: 'var(--font-size-body-small)', fontStyle: 'italic', marginTop: 'var(--space-1)' }}>
                                    "{c.rightCitation.sourceText}"
                                  </div>
                                </div>
                              </div>
                            </td>
                            <td>
                              <Button
                                variant="secondary"
                                onClick={() => {
                                  openResolve(c)
                                }}
                              >
                                Resolve
                              </Button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </Table>
                  )}
                </Card>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-3)' }}>
                  <Button variant="secondary" disabled={exportBlocked} title={exportBlocked ? 'Resolve conflicts first' : undefined}>
                    Finalize & Export
                  </Button>
                </div>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
                <Table>
                  <thead>
                    <tr>
                      <th>Code</th>
                      <th>Description</th>
                      <th>Source</th>
                      <th>Status</th>
                      <th style={{ width: 220 }}>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {codes.map((s) => (
                      <tr key={s.id}>
                        <td style={{ fontFamily: 'var(--font-family-monospace)' }}>{s.code}</td>
                        <td>{s.description}</td>
                        <td>
                          <div style={{ fontSize: 'var(--font-size-body-small)' }}>
                            <div style={{ color: 'var(--color-text-muted)' }}>
                              {s.citation.documentName} (p.{s.citation.pageNumber})
                            </div>
                            <div style={{ fontStyle: 'italic', marginTop: 'var(--space-1)' }}>
                              "{s.citation.sourceText.length > 50 ? s.citation.sourceText.slice(0, 50) + '...' : s.citation.sourceText}"
                            </div>
                          </div>
                        </td>
                        <td>
                          <Badge variant={s.status === 'accepted' ? 'success' : s.status === 'rejected' ? 'error' : 'neutral'}>
                            {s.status}
                          </Badge>
                        </td>
                        <td>
                          <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                            <Button
                              variant="secondary"
                              onClick={() =>
                                setCodes((current) => current.map((c) => (c.id === s.id ? { ...c, status: 'accepted' } : c)))
                              }
                              disabled={s.status === 'accepted'}
                            >
                              Accept
                            </Button>
                            <Button
                              variant="secondary"
                              onClick={() =>
                                setCodes((current) => current.map((c) => (c.id === s.id ? { ...c, status: 'rejected' } : c)))
                              }
                              disabled={s.status === 'rejected'}
                            >
                              Reject
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
            )}
          </Card>
    </div>
  )

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      {renderRightPane()}
      <Modal
        open={conflictModalOpen}
        title="Resolve conflict"
        onClose={() => setConflictModalOpen(false)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setConflictModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={resolveSelected} disabled={!selectedResolution}>
              Resolve
            </Button>
          </>
        }
      >
        {selectedConflict ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
            <div>
              <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>Field</div>
              <div style={{ fontWeight: 600 }}>{selectedConflict.field}</div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)' }}>
              <label style={{ display: 'block', cursor: 'pointer' }}>
                <input
                  type="radio"
                  name="resolution"
                  value="left"
                  checked={selectedResolution === 'left'}
                  onChange={() => setSelectedResolution('left')}
                />{' '}
                Choose value from {selectedConflict.leftCitation.documentName}
                <div style={{ marginTop: 'var(--space-2)', padding: 'var(--space-3)', border: '1px solid var(--color-border)', borderRadius: 'var(--radius-md)' }}>
                  <div style={{ fontWeight: 600 }}>{selectedConflict.leftValue}</div>
                  <div style={{ fontSize: 'var(--font-size-body-small)', color: 'var(--color-text-muted)', marginTop: 'var(--space-1)' }}>
                    Page {selectedConflict.leftCitation.pageNumber}, {selectedConflict.leftCitation.section}
                  </div>
                  <div style={{ fontSize: 'var(--font-size-body-small)', fontStyle: 'italic', marginTop: 'var(--space-1)' }}>
                    "{selectedConflict.leftCitation.sourceText}"
                  </div>
                </div>
              </label>

              <label style={{ display: 'block', cursor: 'pointer' }}>
                <input
                  type="radio"
                  name="resolution"
                  value="right"
                  checked={selectedResolution === 'right'}
                  onChange={() => setSelectedResolution('right')}
                />{' '}

                Choose value from {selectedConflict.rightCitation.documentName}
                <div style={{ marginTop: 'var(--space-2)', padding: 'var(--space-3)', border: '1px solid var(--color-border)', borderRadius: 'var(--radius-md)' }}>
                  <div style={{ fontWeight: 600 }}>{selectedConflict.rightValue}</div>
                  <div style={{ fontSize: 'var(--font-size-body-small)', color: 'var(--color-text-muted)', marginTop: 'var(--space-1)' }}>
                    Page {selectedConflict.rightCitation.pageNumber}, {selectedConflict.rightCitation.section}
                  </div>
                  <div style={{ fontSize: 'var(--font-size-body-small)', fontStyle: 'italic', marginTop: 'var(--space-1)' }}>
                    "{selectedConflict.rightCitation.sourceText}"
                  </div>
                </div>
              </label>
            </div>

            <Alert variant="info">Click on document name to navigate to source location in the PDF viewer.</Alert>
          </div>
        ) : null}
      </Modal>
    </div>
  )
}
