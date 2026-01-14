import { useNavigate } from 'react-router-dom'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'

export default function DashboardPage(): JSX.Element {
  const navigate = useNavigate()

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
          Dashboard
        </h1>
        <p style={{ margin: 0, color: 'var(--color-text-muted)' }}>
          Upload documents, review extracted data, resolve conflicts, and export finalized records.
        </p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 'var(--space-6)' }}>
        <Card title="Uploads today">
          <div style={{ fontSize: 'var(--font-size-h2)', fontWeight: 'var(--font-weight-h2)' }}>3</div>
          <div style={{ color: 'var(--color-text-muted)' }}>Batches</div>
        </Card>
        <Card title="Processing">
          <div style={{ fontSize: 'var(--font-size-h2)', fontWeight: 'var(--font-weight-h2)' }}>2</div>
          <div style={{ color: 'var(--color-text-muted)' }}>In progress</div>
        </Card>
        <Card title="Conflicts">
          <div style={{ fontSize: 'var(--font-size-h2)', fontWeight: 'var(--font-weight-h2)' }}>1</div>
          <div style={{ color: 'var(--color-text-muted)' }}>Needs review</div>
        </Card>
        <Card title="Exports">
          <div style={{ fontSize: 'var(--font-size-h2)', fontWeight: 'var(--font-weight-h2)' }}>5</div>
          <div style={{ color: 'var(--color-text-muted)' }}>Last 7 days</div>
        </Card>
      </div>

      <Card title="Quick actions">
        <div style={{ display: 'flex', gap: 'var(--space-3)', flexWrap: 'wrap' }}>
          <Button onClick={() => navigate('/documents/upload')}>Upload documents</Button>
          <Button variant="secondary" onClick={() => navigate('/documents')}>
            View document list
          </Button>
          <Button variant="secondary" onClick={() => navigate('/patients/demo')}>
            Open Patient 360 (demo)
          </Button>
          <Button variant="secondary" onClick={() => navigate('/export')}>
            Export
          </Button>
        </div>
      </Card>
    </div>
  )
}
