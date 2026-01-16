import type { DragEvent } from 'react'
import { useMemo, useRef, useState } from 'react'
import Alert from '../components/ui/Alert'
import Button from '../components/ui/Button'
import ProgressBar from '../components/ui/ProgressBar'
import Card from '../components/ui/Card'
import ValidationError, { type ValidationErrorInfo, type ValidationErrorType, getValidationError } from '../components/ui/ValidationError'
import { uploadDocumentBatch } from '../lib/documentApi'

type UploadItem = {
  id: string
  file: File
  progress: number
  status: 'queued' | 'uploading' | 'success' | 'error' | 'cancelled' | 'validation_failed'
  error?: string
  validationError?: ValidationErrorInfo
  abortController?: AbortController
  lastProgressTime?: number
  isStalled?: boolean
  documentId?: string
  uploadResponse?: any
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
  const [selectedPatientId, setSelectedPatientId] = useState<string>('')
  const dragCounter = useRef(0)

  const totalCount = items.length

  const canUpload = useMemo(() => {
    const hasValidItems = items.some((i) => i.status === 'queued' || i.status === 'error' || i.status === 'cancelled')
    const hasPatientId = selectedPatientId !== ''
    return hasValidItems && hasPatientId
  }, [items, selectedPatientId])

  const invalidItemsCount = useMemo(() => items.filter((i) => i.status === 'validation_failed').length, [items])

  function validateFile(file: File): ValidationErrorType | null {
    const lowerName = file.name.toLowerCase()
    const allowed = lowerName.endsWith('.pdf') || lowerName.endsWith('.docx')

    if (!allowed) {
      return 'invalid_type'
    }

    if (file.size > 50 * 1024 * 1024) {
      return 'file_too_large'
    }

    if (file.size === 0) {
      return 'empty_file'
    }

    return null
  }

  function validateFiles(files: File[]): { valid: File[]; invalid: Array<{ file: File; errorType: ValidationErrorType }>; batchError?: string } {
    let batchError: string | undefined

    if (files.length > 10) {
      batchError = 'You can upload up to 10 files per batch.'
      files = files.slice(0, 10)
    }

    const valid: File[] = []
    const invalid: Array<{ file: File; errorType: ValidationErrorType }> = []

    for (const file of files) {
      const errorType = validateFile(file)
      if (errorType) {
        invalid.push({ file, errorType })
      } else {
        valid.push(file)
      }
    }

    return { valid, invalid, batchError }
  }

  function addFiles(files: File[]): void {
    const { valid, invalid, batchError } = validateFiles(files)

    if (batchError) {
      setGlobalError(batchError)
    } else {
      setGlobalError('')
    }

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

      for (const { file, errorType } of invalid) {
        next.push({
          id: `${Date.now()}-${Math.random().toString(16).slice(2)}`,
          file,
          progress: 0,
          status: 'validation_failed',
          validationError: getValidationError(errorType),
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
    e.stopPropagation()
    dragCounter.current = 0
    setIsDragging(false)
    const files = Array.from(e.dataTransfer.files || [])
    addFiles(files)
  }

  function handleDragEnter(e: DragEvent<HTMLDivElement>): void {
    e.preventDefault()
    e.stopPropagation()
    dragCounter.current += 1
    if (dragCounter.current === 1) {
      setIsDragging(true)
    }
  }

  function handleDragOver(e: DragEvent<HTMLDivElement>): void {
    e.preventDefault()
    e.stopPropagation()
  }

  function handleDragLeave(e: DragEvent<HTMLDivElement>): void {
    e.preventDefault()
    e.stopPropagation()
    dragCounter.current -= 1
    if (dragCounter.current === 0) {
      setIsDragging(false)
    }
  }

  function removeItem(id: string): void {
    setItems((current) => current.filter((i) => i.id !== id))
  }

  function cancelUpload(id: string): void {
    setItems((current) =>
      current.map((i) => {
        if (i.id === id && i.status === 'uploading') {
          i.abortController?.abort()
          return { ...i, status: 'cancelled', abortController: undefined, isStalled: false }
        }
        return i
      })
    )
  }

  function retryUpload(id: string): void {
    setItems((current) =>
      current.map((i) => {
        if (i.id === id && (i.status === 'error' || i.status === 'cancelled')) {
          return { ...i, status: 'queued', progress: 0, error: undefined, isStalled: false }
        }
        return i
      })
    )
  }

  async function startUpload(): Promise<void> {
    setGlobalError('')

    const queue = items.filter((i) => i.status === 'queued' || i.status === 'error' || i.status === 'cancelled')
    if (queue.length === 0 || !selectedPatientId) return

    // Get all valid files for batch upload
    const validFiles = queue.map(item => item.file)
    
    // Create abort controller for the batch
    const abortController = new AbortController()
    const startTime = Date.now()

    // Update all items to uploading status
    setItems((current) =>
      current.map((i) =>
        queue.some(q => q.id === i.id)
          ? { ...i, status: 'uploading', error: undefined, progress: 0, abortController, lastProgressTime: startTime, isStalled: false }
          : i
      )
    )

    // Simulate progress during upload (since batch API doesn't provide progress)
    const progressInterval = setInterval(() => {
      setItems((current) =>
        current.map((i) => {
          if (i.status === 'uploading' && i.progress < 90) {
            return { ...i, progress: Math.min(i.progress + 10, 90), lastProgressTime: Date.now() }
          }
          return i
        })
      )
    }, 200)

    try {
      // Use batch upload API
      const result = await uploadDocumentBatch(selectedPatientId, validFiles, abortController.signal)
      
      // Clear progress simulation
      clearInterval(progressInterval)
      
      if (result.success) {
        const batchResponse = result.data
        
        // Update items based on batch response
        setItems((current) =>
          current.map((i) => {
            const queueItem = queue.find(q => q.id === i.id)
            if (!queueItem) return i
            
            const fileResult = batchResponse.fileResults.find(fr => fr.fileName === queueItem.file.name)
            
            if (fileResult) {
              if (fileResult.isAccepted) {
                return { 
                  ...i, 
                  status: 'success', 
                  progress: 100, 
                  abortController: undefined, 
                  isStalled: false,
                  documentId: fileResult.documentId,
                  uploadResponse: fileResult
                }
              } else {
                return { 
                  ...i, 
                  status: 'error', 
                  error: fileResult.rejectionReason || fileResult.validationErrors.join(', ') || 'Upload failed',
                  abortController: undefined, 
                  isStalled: false 
                }
              }
            }
            
            return { ...i, status: 'error', error: 'File not found in batch response', abortController: undefined, isStalled: false }
          })
        )
      } else {
        // Handle API error
        setItems((current) =>
          current.map((i) =>
            queue.some(q => q.id === i.id)
              ? { ...i, status: 'error', error: result.error, abortController: undefined, isStalled: false }
              : i
          )
        )
      }
    } catch (err) {
      // Clear progress simulation
      clearInterval(progressInterval)
      
      const isCancelled = abortController.signal.aborted
      
      if (!isCancelled) {
        const errorMessage = err instanceof Error ? err.message : 'Upload failed'
        setItems((current) =>
          current.map((i) =>
            queue.some(q => q.id === i.id)
              ? { ...i, status: 'error', error: errorMessage, abortController: undefined, isStalled: false }
              : i
          )
        )
      } else {
        // Handle cancellation
        setItems((current) =>
          current.map((i) =>
            queue.some(q => q.id === i.id)
              ? { ...i, status: 'cancelled', abortController: undefined, isStalled: false }
              : i
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
        <div style={{ marginBottom: 'var(--space-4)' }}>
          <label htmlFor="patient-id" style={{ display: 'block', fontWeight: 600, marginBottom: 'var(--space-2)' }}>
            Patient ID *
          </label>
          <input
            id="patient-id"
            type="text"
            value={selectedPatientId}
            onChange={(e) => setSelectedPatientId(e.target.value)}
            placeholder="Enter patient ID"
            style={{
              width: '100%',
              padding: 'var(--space-2)',
              border: '1px solid var(--color-border)',
              borderRadius: 'var(--radius-sm)',
              fontSize: 'var(--font-size-body)',
            }}
            required
          />
        </div>
        <div
          data-testid="drop-zone"
          role="region"
          aria-label="File drop zone"
          onDrop={handleDrop}
          onDragEnter={handleDragEnter}
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
          <div data-testid="drop-zone-text" style={{ fontSize: 'var(--font-size-body-large)', fontWeight: 600 }}>
            {isDragging ? 'Drop files here' : 'Drag & drop files here'}
          </div>
          <div style={{ color: 'var(--color-text-muted)' }}>PDF or DOCX. Up to 10 files, max 50MB each.</div>
          <div>
            <label className="ui-button ui-button--secondary" style={{ cursor: 'pointer' }}>
              Select files
              <input
                data-testid="file-input"
                type="file"
                multiple
                accept=".pdf,.docx"
                style={{ display: 'none' }}
                onChange={handleInputChange}
                aria-label="Select files to upload"
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
              setSelectedPatientId('')
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
        <Card
          title="Selected files"
          headerRight={
            invalidItemsCount > 0 ? (
              <span style={{ color: 'var(--color-error-main)', fontSize: 'var(--font-size-body-small)' }}>
                {invalidItemsCount} file{invalidItemsCount > 1 ? 's' : ''} with errors
              </span>
            ) : null
          }
          data-testid="selected-files-card"
        >
          <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-3)' }}>
            {items.map((item) => (
              <div
                key={item.id}
                data-testid="file-item"
                style={{
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 'var(--space-2)',
                  padding: 'var(--space-3)',
                  border: `1px solid ${item.status === 'validation_failed' ? 'var(--color-error-main)' : 'var(--color-border)'}`,
                  borderRadius: 'var(--radius-md)',
                  background: item.status === 'validation_failed' ? 'var(--color-error-light)' : 'transparent',
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-4)' }}>
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)' }}>
                      {item.status === 'validation_failed' ? (
                        <span style={{ color: 'var(--color-error-main)', flexShrink: 0 }} aria-hidden="true">
                          <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor">
                            <path fillRule="evenodd" clipRule="evenodd" d="M8 1C4.13401 1 1 4.13401 1 8C1 11.866 4.13401 15 8 15C11.866 15 15 11.866 15 8C15 4.13401 11.866 1 8 1ZM7.25 4.5V8.5H8.75V4.5H7.25ZM8 11.5C8.55228 11.5 9 11.0523 9 10.5C9 9.94772 8.55228 9.5 8 9.5C7.44772 9.5 7 9.94772 7 10.5C7 11.0523 7.44772 11.5 8 11.5Z" />
                          </svg>
                        </span>
                      ) : null}
                      <div
                        data-testid="file-name"
                        title={item.file.name}
                        style={{ fontWeight: 600, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: '300px' }}
                      >
                        {item.file.name}
                      </div>
                    </div>
                    <div style={{ color: 'var(--color-text-muted)', fontSize: 'var(--font-size-body-small)' }}>
                      {formatBytes(item.file.size)}
                    </div>
                  </div>

                  {item.status !== 'validation_failed' ? (
                    <div style={{ width: 240 }} aria-live="polite">
                      <ProgressBar
                        value={item.progress}
                        showPercentage={item.status === 'uploading'}
                        variant={
                          item.status === 'success'
                            ? 'success'
                            : item.status === 'error'
                              ? 'error'
                              : item.status === 'cancelled'
                                ? 'cancelled'
                                : 'default'
                        }
                        label={
                          item.status === 'uploading'
                            ? item.isStalled
                              ? 'Stalled...'
                              : 'Uploading'
                            : item.status === 'success'
                              ? 'Uploaded'
                              : item.status === 'error'
                                ? item.error || 'Error'
                                : item.status === 'cancelled'
                                  ? 'Cancelled'
                                  : 'Ready'
                        }
                        data-testid="upload-progress-bar"
                      />
                    </div>
                  ) : null}

                  <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
                    {item.status === 'uploading' ? (
                      <Button
                        variant="danger"
                        onClick={() => cancelUpload(item.id)}
                        data-testid="cancel-upload-btn"
                        aria-label={`Cancel upload for ${item.file.name}`}
                      >
                        Cancel
                      </Button>
                    ) : null}

                    {item.status === 'error' || item.status === 'cancelled' ? (
                      <Button
                        variant="secondary"
                        onClick={() => retryUpload(item.id)}
                        data-testid="retry-upload-btn"
                        aria-label={`Retry upload for ${item.file.name}`}
                      >
                        Retry
                      </Button>
                    ) : null}

                    <Button
                      variant="secondary"
                      onClick={() => removeItem(item.id)}
                      disabled={item.status === 'uploading'}
                      data-testid="remove-file-btn"
                      aria-label={`Remove ${item.file.name}`}
                    >
                      Remove
                    </Button>
                  </div>
                </div>

                {item.status === 'validation_failed' && item.validationError ? (
                  <ValidationError error={item.validationError} data-testid="inline-validation-error" />
                ) : null}
              </div>
            ))}
          </div>
        </Card>
      ) : null}
    </div>
  )
}
