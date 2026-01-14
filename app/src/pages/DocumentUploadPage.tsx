import type { DragEvent } from 'react'
import { useMemo, useState } from 'react'
import Alert from '../components/ui/Alert'
import Button from '../components/ui/Button'
import ProgressBar from '../components/ui/ProgressBar'
import Card from '../components/ui/Card'

type UploadItem = {
  id: string
  file: File
  progress: number
  status: 'queued' | 'uploading' | 'success' | 'error'
  error?: string
}

function formatBytes(bytes: number): string {
  if (!Number.isFinite(bytes) || bytes <= 0) return '0 B'
  const units = ['B', 'KB', 'MB', 'GB']
  const idx = Math.min(units.length - 1, Math.floor(Math.log(bytes) / Math.log(1024)))
  const value = bytes / Math.pow(1024, idx)
  return `${value.toFixed(idx === 0 ? 0 : 1)} ${units[idx]}`
}

export default function DocumentUploadPage(): JSX.Element {
  const [items, setItems] = useState<UploadItem[]>([])
  const [isDragging, setIsDragging] = useState(false)
  const [globalError, setGlobalError] = useState('')

  const totalCount = items.length

  const canUpload = useMemo(() => items.some((i) => i.status === 'queued' || i.status === 'error'), [items])

  function validateFiles(files: File[]): { valid: File[]; errors: string[] } {
    const errors: string[] = []

    if (files.length > 10) {
      errors.push('You can upload up to 10 files per batch.')
      files = files.slice(0, 10)
    }

    const valid = files.filter((file) => {
      const lowerName = file.name.toLowerCase()
      const allowed = lowerName.endsWith('.pdf') || lowerName.endsWith('.docx')
      if (!allowed) errors.push(`${file.name}: unsupported file type. Upload PDF or DOCX.`)
      if (file.size > 50 * 1024 * 1024) errors.push(`${file.name}: file is too large (max 50MB).`)
      return allowed && file.size <= 50 * 1024 * 1024
    })

    return { valid, errors }
  }

  function addFiles(files: File[]): void {
    const { valid, errors } = validateFiles(files)

    if (errors.length > 0) {
      setGlobalError(errors[0])
    }

    if (valid.length === 0) return

    setItems((current) => {
      const next = [...current]
      for (const file of valid) {
        next.push({
          id: `${Date.now()}-${Math.random().toString(16).slice(2)}`,
          file,
          progress: 0,
          status: 'queued',
        })
      }
      return next
    })
  }

  function handleInputChange(e: React.ChangeEvent<HTMLInputElement>): void {
    const files = Array.from(e.target.files || [])
    addFiles(files)
    e.target.value = ''
  }

  function handleDrop(e: DragEvent<HTMLDivElement>): void {
    e.preventDefault()
    setIsDragging(false)
    const files = Array.from(e.dataTransfer.files || [])
    addFiles(files)
  }

  function handleDragOver(e: DragEvent<HTMLDivElement>): void {
    e.preventDefault()
    if (!isDragging) setIsDragging(true)
  }

  function handleDragLeave(): void {
    setIsDragging(false)
  }

  function removeItem(id: string): void {
    setItems((current) => current.filter((i) => i.id !== id))
  }

  async function startUpload(): Promise<void> {
    setGlobalError('')

    const queue = items.filter((i) => i.status === 'queued' || i.status === 'error')
    if (queue.length === 0) return

    for (const item of queue) {
      setItems((current) =>
        current.map((i) => (i.id === item.id ? { ...i, status: 'uploading', error: undefined, progress: 0 } : i))
      )

      try {
        // UI-only simulation (wireframe behavior). Replace with real multipart upload later.
        for (let p = 10; p <= 100; p += 10) {
          await new Promise((r) => setTimeout(r, 120))
          setItems((current) => current.map((i) => (i.id === item.id ? { ...i, progress: p } : i)))
        }

        setItems((current) => current.map((i) => (i.id === item.id ? { ...i, status: 'success', progress: 100 } : i)))
      } catch {
        setItems((current) =>
          current.map((i) =>
            i.id === item.id ? { ...i, status: 'error', error: 'Upload failed. Please retry.' } : i
          )
        )
      }
    }
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-6)' }}>
      {globalError ? <Alert variant="error">{globalError}</Alert> : null}

      <Card
        title="Upload documents"
        headerRight={<div style={{ color: 'var(--color-text-muted)' }}>{totalCount} selected</div>}
      >
        <div
          onDrop={handleDrop}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          style={{
            minHeight: 200,
            border: `2px dashed ${isDragging ? 'var(--color-primary-500)' : 'var(--color-border)'}`,
            borderRadius: 'var(--radius-lg)',
            background: isDragging ? 'var(--color-primary-50)' : 'var(--color-neutral-50)',
            padding: 'var(--space-8)',
            textAlign: 'center',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 'var(--space-3)',
          }}
        >
          <div style={{ fontSize: 'var(--font-size-body-large)', fontWeight: 600 }}>Drag & drop files here</div>
          <div style={{ color: 'var(--color-text-muted)' }}>PDF or DOCX. Up to 10 files, max 50MB each.</div>
          <div>
            <label className="ui-button ui-button--secondary" style={{ cursor: 'pointer' }}>
              Select files
              <input
                type="file"
                multiple
                accept=".pdf,.docx"
                style={{ display: 'none' }}
                onChange={handleInputChange}
              />
            </label>
          </div>
        </div>

        <div style={{ marginTop: 'var(--space-6)', display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-3)' }}>
          <Button
            variant="secondary"
            onClick={() => {
              setItems([])
              setGlobalError('')
            }}
            disabled={items.length === 0}
          >
            Clear
          </Button>
          <Button onClick={startUpload} disabled={!canUpload}>
            Upload
          </Button>
        </div>
      </Card>

      {items.length > 0 ? (
        <Card title="Selected files">
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-3)' }}>
            {items.map((item) => (
              <div
                key={item.id}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 'var(--space-4)',
                  padding: 'var(--space-3)',
                  border: '1px solid var(--color-border)',
                  borderRadius: 'var(--radius-md)',
                }}
              >
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontWeight: 600, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {item.file.name}
                  </div>
                  <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>
                    {formatBytes(item.file.size)}
                  </div>
                </div>

                <div style={{ width: 240 }}>
                  <ProgressBar
                    value={item.progress}
                    label={
                      item.status === 'uploading'
                        ? `Uploading ${item.progress}%`
                        : item.status === 'success'
                          ? 'Uploaded'
                          : item.status === 'error'
                            ? item.error || 'Error'
                            : 'Ready'
                    }
                  />
                </div>

                <Button variant="secondary" onClick={() => removeItem(item.id)} disabled={item.status === 'uploading'}>
                  Remove
                </Button>
              </div>
            ))}
          </div>
        </Card>
      ) : null}
    </div>
  )
}
