import { render, screen, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import SourceDocumentViewer from '../../components/documents/SourceDocumentViewer'
import * as documentsApi from '../../lib/documentsApi'

vi.mock('../../lib/documentsApi', () => ({
  getDocumentContent: vi.fn(),
  createDocumentUrl: vi.fn(),
  revokeDocumentUrl: vi.fn(),
}))

describe('SourceDocumentViewer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders placeholder when no citation is active', () => {
    render(<SourceDocumentViewer activeCitation={null} />)
    
    expect(screen.getByTestId('source-viewer-placeholder')).toBeInTheDocument()
    expect(screen.getByText('Select a citation to view the source document')).toBeInTheDocument()
  })

  it('shows loading state when fetching document', async () => {
    const mockCitation = {
      id: 'citation-1',
      documentId: 'doc-123',
      documentName: 'Test Document.pdf',
      pageNumber: 5,
      section: 'Section A',
      sourceText: 'Sample text',
      coordinates: null,
    }

    vi.mocked(documentsApi.getDocumentContent).mockImplementation(
      () => new Promise(() => {})
    )

    render(<SourceDocumentViewer activeCitation={mockCitation} />)
    
    await waitFor(() => {
      expect(screen.getByTestId('source-viewer-loading')).toBeInTheDocument()
    })
  })

  it('displays document in iframe when loaded successfully', async () => {
    const mockCitation = {
      id: 'citation-1',
      documentId: 'doc-123',
      documentName: 'Test Document.pdf',
      pageNumber: 5,
      section: 'Section A',
      sourceText: 'Sample text',
      coordinates: null,
    }

    const mockBlob = new Blob(['test content'], { type: 'application/pdf' })
    vi.mocked(documentsApi.getDocumentContent).mockResolvedValue({
      success: true,
      data: mockBlob,
    })
    vi.mocked(documentsApi.createDocumentUrl).mockReturnValue('blob:http://localhost/test-url')

    render(<SourceDocumentViewer activeCitation={mockCitation} />)
    
    await waitFor(() => {
      expect(screen.getByTestId('source-viewer-loaded')).toBeInTheDocument()
    })

    const iframe = screen.getByTitle('Source document: Test Document.pdf')
    expect(iframe).toBeInTheDocument()
  })

  it('shows error state when document fetch fails', async () => {
    const mockCitation = {
      id: 'citation-1',
      documentId: 'doc-123',
      documentName: 'Test Document.pdf',
      pageNumber: 5,
      section: null,
      sourceText: null,
      coordinates: null,
    }

    vi.mocked(documentsApi.getDocumentContent).mockResolvedValue({
      success: false,
      error: { code: 'NOT_FOUND', message: 'Document not found', details: [] },
      status: 404,
    })

    const onError = vi.fn()
    render(<SourceDocumentViewer activeCitation={mockCitation} onError={onError} />)
    
    await waitFor(() => {
      expect(screen.getByTestId('source-viewer-error')).toBeInTheDocument()
    })

    expect(onError).toHaveBeenCalledWith('Document not found')
  })

  it('displays page indicator when citation has page number', async () => {
    const mockCitation = {
      id: 'citation-1',
      documentId: 'doc-123',
      documentName: 'Test Document.pdf',
      pageNumber: 10,
      section: 'Results',
      sourceText: 'Sample text',
      coordinates: null,
    }

    const mockBlob = new Blob(['test content'], { type: 'application/pdf' })
    vi.mocked(documentsApi.getDocumentContent).mockResolvedValue({
      success: true,
      data: mockBlob,
    })
    vi.mocked(documentsApi.createDocumentUrl).mockReturnValue('blob:http://localhost/test-url')

    render(<SourceDocumentViewer activeCitation={mockCitation} />)
    
    await waitFor(() => {
      expect(screen.getByText(/Page 10/)).toBeInTheDocument()
    })
  })

  it('revokes blob URL on unmount', async () => {
    const mockCitation = {
      id: 'citation-1',
      documentId: 'doc-123',
      documentName: 'Test Document.pdf',
      pageNumber: 1,
      section: null,
      sourceText: null,
      coordinates: null,
    }

    const mockBlob = new Blob(['test content'], { type: 'application/pdf' })
    vi.mocked(documentsApi.getDocumentContent).mockResolvedValue({
      success: true,
      data: mockBlob,
    })
    vi.mocked(documentsApi.createDocumentUrl).mockReturnValue('blob:http://localhost/test-url')

    const { unmount } = render(<SourceDocumentViewer activeCitation={mockCitation} />)
    
    await waitFor(() => {
      expect(screen.getByTestId('source-viewer-loaded')).toBeInTheDocument()
    })

    unmount()

    expect(documentsApi.revokeDocumentUrl).toHaveBeenCalledWith('blob:http://localhost/test-url')
  })
})
