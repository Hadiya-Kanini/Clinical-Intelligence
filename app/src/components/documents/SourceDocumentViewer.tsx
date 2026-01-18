import { useCallback, useEffect, useRef, useState } from 'react'
import Alert from '../ui/Alert'
import { getDocumentContent, createDocumentUrl, revokeDocumentUrl } from '../../lib/documentsApi'
import type { EntityCitation } from '../../lib/patientApi'

type SourceDocumentViewerProps = {
  activeCitation: EntityCitation | null
  onError?: (error: string) => void
  scrollContainerRef?: React.RefObject<HTMLDivElement>
}

type ViewerState = 'idle' | 'loading' | 'loaded' | 'error'

export default function SourceDocumentViewer({
  activeCitation,
  onError,
  scrollContainerRef,
}: SourceDocumentViewerProps): JSX.Element {
  const [viewerState, setViewerState] = useState<ViewerState>('idle')
  const [documentUrl, setDocumentUrl] = useState<string | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [currentDocumentId, setCurrentDocumentId] = useState<string | null>(null)
  const containerRef = useRef<HTMLDivElement>(null)
  const iframeRef = useRef<HTMLIFrameElement>(null)

  const loadDocument = useCallback(async (documentId: string) => {
    if (documentId === currentDocumentId && documentUrl) {
      return
    }

    setViewerState('loading')
    setErrorMessage(null)

    if (documentUrl) {
      revokeDocumentUrl(documentUrl)
      setDocumentUrl(null)
    }

    const result = await getDocumentContent(documentId)

    if (result.success) {
      const url = createDocumentUrl(result.data)
      setDocumentUrl(url)
      setCurrentDocumentId(documentId)
      setViewerState('loaded')
    } else {
      const message = result.error.message || 'Failed to load document'
      setErrorMessage(message)
      setViewerState('error')
      onError?.(message)
    }
  }, [currentDocumentId, documentUrl, onError])

  useEffect(() => {
    if (activeCitation?.documentId) {
      loadDocument(activeCitation.documentId)
    }
  }, [activeCitation?.documentId, loadDocument])

  useEffect(() => {
    if (viewerState === 'loaded' && activeCitation?.pageNumber && iframeRef.current) {
      const pageParam = `#page=${activeCitation.pageNumber}`
      if (documentUrl && !documentUrl.includes('#page=')) {
        const newUrl = documentUrl + pageParam
        iframeRef.current.src = newUrl
      }
    }
  }, [viewerState, activeCitation?.pageNumber, documentUrl])

  useEffect(() => {
    return () => {
      if (documentUrl) {
        revokeDocumentUrl(documentUrl)
      }
    }
  }, [documentUrl])

  const effectiveRef = scrollContainerRef || containerRef

  if (viewerState === 'idle' || !activeCitation) {
    return (
      <div
        ref={effectiveRef as React.RefObject<HTMLDivElement>}
        style={{
          height: '100%',
          minHeight: 520,
          border: '1px solid var(--color-border)',
          borderRadius: 'var(--radius-md)',
          background: 'var(--color-neutral-50)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: 'var(--color-text-muted)',
        }}
        data-testid="source-viewer-placeholder"
      >
        <div style={{ textAlign: 'center', padding: 'var(--space-4)' }}>
          <p style={{ margin: 0, marginBottom: 'var(--space-2)' }}>
            Select a citation to view the source document
          </p>
          <p style={{ margin: 0, fontSize: 'var(--font-size-body-small)' }}>
            Click on a document reference to navigate to the cited location
          </p>
        </div>
      </div>
    )
  }

  if (viewerState === 'loading') {
    return (
      <div
        ref={effectiveRef as React.RefObject<HTMLDivElement>}
        style={{
          height: '100%',
          minHeight: 520,
          border: '1px solid var(--color-border)',
          borderRadius: 'var(--radius-md)',
          background: 'var(--color-neutral-50)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
        data-testid="source-viewer-loading"
      >
        <div style={{ textAlign: 'center' }}>
          <p style={{ margin: 0, color: 'var(--color-text-muted)' }}>
            Loading document...
          </p>
        </div>
      </div>
    )
  }

  if (viewerState === 'error') {
    return (
      <div
        ref={effectiveRef as React.RefObject<HTMLDivElement>}
        style={{
          height: '100%',
          minHeight: 520,
          display: 'flex',
          flexDirection: 'column',
          gap: 'var(--space-4)',
        }}
        data-testid="source-viewer-error"
      >
        <Alert variant="error">
          {errorMessage || 'Unable to load document. The file may be unavailable.'}
        </Alert>
        <div
          style={{
            flex: 1,
            border: '1px solid var(--color-border)',
            borderRadius: 'var(--radius-md)',
            background: 'var(--color-neutral-50)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'var(--color-text-muted)',
          }}
        >
          Document unavailable
        </div>
      </div>
    )
  }

  const highlightIndicator = activeCitation.coordinates ? (
    <div
      style={{
        position: 'absolute',
        top: 'var(--space-2)',
        right: 'var(--space-2)',
        padding: 'var(--space-1) var(--space-2)',
        background: 'var(--color-warning-100)',
        color: 'var(--color-warning-700)',
        borderRadius: 'var(--radius-sm)',
        fontSize: 'var(--font-size-body-small)',
        fontWeight: 500,
        zIndex: 10,
      }}
    >
      Highlighted: Page {activeCitation.pageNumber}
      {activeCitation.section && `, ${activeCitation.section}`}
    </div>
  ) : activeCitation.pageNumber ? (
    <div
      style={{
        position: 'absolute',
        top: 'var(--space-2)',
        right: 'var(--space-2)',
        padding: 'var(--space-1) var(--space-2)',
        background: 'var(--color-info-100)',
        color: 'var(--color-info-700)',
        borderRadius: 'var(--radius-sm)',
        fontSize: 'var(--font-size-body-small)',
        fontWeight: 500,
        zIndex: 10,
      }}
    >
      Page {activeCitation.pageNumber}
      {activeCitation.section && ` - ${activeCitation.section}`}
    </div>
  ) : null

  return (
    <div
      ref={effectiveRef as React.RefObject<HTMLDivElement>}
      style={{
        height: '100%',
        minHeight: 520,
        position: 'relative',
        border: '1px solid var(--color-border)',
        borderRadius: 'var(--radius-md)',
        overflow: 'hidden',
      }}
      data-testid="source-viewer-loaded"
    >
      {highlightIndicator}
      <iframe
        ref={iframeRef}
        src={documentUrl || undefined}
        title={`Source document: ${activeCitation.documentName || 'Document'}`}
        style={{
          width: '100%',
          height: '100%',
          border: 'none',
        }}
      />
    </div>
  )
}
