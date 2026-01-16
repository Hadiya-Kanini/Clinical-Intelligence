import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'
import Alert from '../components/ui/Alert'
import { dashboardApi, type DashboardStats } from '../lib/dashboardApi'

export default function DashboardPage(): JSX.Element {
  const navigate = useNavigate()
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function fetchStats() {
      try {
        setLoading(true)
        setError(null)
        const result = await dashboardApi.getDashboardStats()
        
        if (result.success) {
          setStats(result.data)
        } else {
          setError(result.error.message || 'Failed to load dashboard statistics')
        }
      } catch (err) {
        setError('Network error. Please try again.')
      } finally {
        setLoading(false)
      }
    }

    fetchStats()
  }, [])

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

      {error && <Alert variant="error">{error}</Alert>}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 'var(--space-6)' }}>
        <Card title="Uploads today">
          <div style={{ fontSize: 'var(--font-size-h2)', fontWeight: 'var(--font-weight-h2)' }}>
            {loading ? '...' : stats?.uploadsToday ?? 0}
          </div>
          <div style={{ color: 'var(--color-text-muted)' }}>Batches</div>
        </Card>
        <Card title="Processing">
          <div style={{ fontSize: 'var(--font-size-h2)', fontWeight: 'var(--font-weight-h2)' }}>
            {loading ? '...' : stats?.processing ?? 0}
          </div>
          <div style={{ color: 'var(--color-text-muted)' }}>In progress</div>
        </Card>
        <Card title="Conflicts">
          <div style={{ fontSize: 'var(--font-size-h2)', fontWeight: 'var(--font-weight-h2)' }}>
            {loading ? '...' : stats?.conflicts ?? 0}
          </div>
          <div style={{ color: 'var(--color-text-muted)' }}>Needs review</div>
        </Card>
        <Card title="Exports">
          <div style={{ fontSize: 'var(--font-size-h2)', fontWeight: 'var(--font-weight-h2)' }}>
            {loading ? '...' : stats?.exportsLast7Days ?? 0}
          </div>
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
