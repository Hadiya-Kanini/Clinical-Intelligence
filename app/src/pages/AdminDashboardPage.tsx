import Card from '../components/ui/Card'

export default function AdminDashboardPage(): JSX.Element {
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
          Admin Dashboard
        </h1>
        <p style={{ margin: 0, color: 'var(--color-text-muted)' }}>
          System health, security overview, and platform administration.
        </p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 'var(--space-6)' }}>
      <Card title="System status">
        <div style={{ color: 'var(--color-text-muted)' }}>All services operational (demo)</div>
      </Card>
      <Card title="Security">
        <div style={{ color: 'var(--color-text-muted)' }}>No critical alerts (demo)</div>
      </Card>
      <Card title="Users">
        <div style={{ color: 'var(--color-text-muted)' }}>Active users: 12 (demo)</div>
      </Card>
      <Card title="Processing queue">
        <div style={{ color: 'var(--color-text-muted)' }}>Jobs in progress: 2 (demo)</div>
      </Card>
      <Card title="Audit logs">
        <div style={{ color: 'var(--color-text-muted)' }}>Latest event: Login success (demo)</div>
      </Card>
      <Card title="Analytics">
        <div style={{ color: 'var(--color-text-muted)' }}>Coming soon</div>
      </Card>
      </div>
    </div>
  )
}
