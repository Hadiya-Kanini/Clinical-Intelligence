import { useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import Alert from '../components/ui/Alert'
import Badge from '../components/ui/Badge'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Modal from '../components/ui/Modal'
import Table from '../components/ui/Table'

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
  const [conflicts, setConflicts] = useState<Conflict[]>([
    {
      id: 'C-01',
      field: 'Allergy: Penicillin',
      leftValue: 'No',
      rightValue: 'Yes (rash)',
      leftCitation: {
        documentId: 'DOC-001',
        documentName: 'Admission_Note.pdf',
        pageNumber: 2,
        section: 'Allergies',
        sourceText: 'No known drug allergies.',
      },
      rightCitation: {
        documentId: 'DOC-002',
        documentName: 'Discharge_Summary.pdf',
        pageNumber: 1,
        section: 'Allergies',
        sourceText: 'Penicillin allergy - rash reported.',
      },
    },
  ])

  const [codes, setCodes] = useState<CodeSuggestion[]>([
    {
      id: 'S-01',
      code: 'I10',
      description: 'Essential (primary) hypertension',
      status: 'pending',
      citation: {
        documentId: 'DOC-001',
        documentName: 'Admission_Note.pdf',
        pageNumber: 3,
        section: 'Diagnoses',
        sourceText: 'Patient has history of hypertension, currently on lisinopril.',
      },
    },
    {
      id: 'S-02',
      code: 'E11.9',
      description: 'Type 2 diabetes mellitus without complications',
      status: 'pending',
      citation: {
        documentId: 'DOC-002',
        documentName: 'Discharge_Summary.pdf',
        pageNumber: 2,
        section: 'Medical History',
        sourceText: 'Type 2 diabetes mellitus, well-controlled on metformin.',
      },
    },
  ])

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

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      {exportBlocked ? (
        <Alert variant="warning">{conflictCount} conflict(s) must be resolved before export.</Alert>
      ) : (
        <Alert variant="success">All conflicts resolved. You can finalize and export.</Alert>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-6)' }}>
        <Card title="Source document">
          <div
            style={{
              height: 520,
              border: '1px solid var(--color-border)',
              borderRadius: 'var(--radius-md)',
              background: 'var(--color-neutral-50)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--color-text-muted)',
            }}
          >
            PDF Viewer Placeholder (patient {patientId || 'unknown'})
          </div>
        </Card>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
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
                <div style={{ fontWeight: 600 }}>MRN-0001</div>
              </div>
              <div>
                <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>DOB</div>
                <div style={{ fontWeight: 600 }}>1978-05-04</div>
              </div>
              <div>
                <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>Name</div>
                <div style={{ fontWeight: 600 }}>Demo Patient</div>
              </div>
              <div>
                <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>Sex</div>
                <div style={{ fontWeight: 600 }}>F</div>
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
      </div>

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
