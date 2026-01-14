import type { ChangeEvent, FormEvent } from 'react'
import { useState } from 'react'
import Alert from '../components/ui/Alert'
import Button from '../components/ui/Button'
import Card from '../components/ui/Card'

type Format = 'csv' | 'json' | 'clipboard'

type Status = 'idle' | 'exporting' | 'success' | 'error' | 'blocked'

export default function ExportPage(): JSX.Element {
  const [format, setFormat] = useState<Format>('csv')
  const [status, setStatus] = useState<Status>('idle')

  const isBlocked = status === 'blocked'

  function handleChange(e: ChangeEvent<HTMLInputElement>): void {
    setFormat(e.target.value as Format)
  }

  async function handleSubmit(e: FormEvent<HTMLFormElement>): Promise<void> {
    e.preventDefault()

    if (isBlocked) return

    setStatus('exporting')

    try {
      await new Promise((r) => setTimeout(r, 800))
      setStatus('success')

      if (format === 'clipboard') {
        try {
          await navigator.clipboard.writeText('{ "export": "demo" }')
        } catch {
        }
      }
    } catch {
      setStatus('error')
    }
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      {status === 'success' ? <Alert variant="success">Export complete.</Alert> : null}
      {status === 'error' ? <Alert variant="error">Export failed. Please try again.</Alert> : null}
      {status === 'blocked' ? (
        <Alert variant="warning">Export is blocked until all conflicts are resolved.</Alert>
      ) : null}

      <Card title="Export">
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-2)' }}>
            <label>
              <input type="radio" name="format" value="csv" checked={format === 'csv'} onChange={handleChange} /> CSV
            </label>
            <label>
              <input type="radio" name="format" value="json" checked={format === 'json'} onChange={handleChange} /> JSON
            </label>
            <label>
              <input
                type="radio"
                name="format"
                value="clipboard"
                checked={format === 'clipboard'}
                onChange={handleChange}
              />{' '}
              Clipboard
            </label>
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-3)' }}>
            <Button variant="secondary" type="button" onClick={() => setStatus('blocked')}>
              Simulate blocked
            </Button>
            <Button type="submit" loading={status === 'exporting'} disabled={status === 'exporting' || isBlocked}>
              {status === 'exporting' ? 'Exporting…' : 'Export'}
            </Button>
          </div>
        </form>
      </Card>
    </div>
  )
}
