# Text Extraction Contract - v1

## Overview

This contract defines the schema for extracted text segments produced by the AI Worker during document processing. It provides a stable, versioned format for text extraction output that preserves positional metadata (page, section, coordinates) for downstream chunking and citation workflows.

## Schema: `extracted_text.schema.json`

### Purpose

Defines the structure for extracted text from PDF and DOCX documents, including:
- Document identification
- Extraction timestamp
- Text segments with positional metadata

### Key Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `schema_version` | string | Yes | Schema version (currently "1.0") |
| `document_id` | string | Yes | Source document identifier |
| `extraction_timestamp` | string | No | ISO 8601 timestamp of extraction |
| `segments` | array | Yes | Array of extracted text segments |

### Segment Structure

Each segment contains:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `text` | string | Yes | The extracted text content |
| `document_location.page` | integer | No | 1-indexed page number |
| `document_location.section` | string | No | Section identifier or heading |
| `document_location.coordinates` | object | No | Bounding box (x0, y0, x1, y1) |
| `segment_index` | integer | No | Zero-based extraction order index |

## Usage

### Producer: AI Worker

The AI Worker produces extracted text conforming to this schema after processing PDF or DOCX documents via `PyPDFLoader` or `Docx2txtLoader`.

### Consumer: Backend API

The Backend API consumes extracted text to persist `DocumentChunk` rows with:
- `TextContent` ← `segment.text`
- `Page` ← `segment.document_location.page`
- `Section` ← `segment.document_location.section`
- `Coordinates` ← JSON-serialized `segment.document_location.coordinates`

## Versioning

- **v1.0**: Initial release with page, section, and coordinates support

## Related Contracts

- `contracts/jobs/v1/job.schema.json`: Job payload includes `storage_path` and `mime_type` for extraction routing
- `contracts/text_merge/v1/merged_text.schema.json`: Multi-document merge output preserving per-document provenance
