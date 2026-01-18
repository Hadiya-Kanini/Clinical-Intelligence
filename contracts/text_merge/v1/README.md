# Text Merge Contract - v1

## Overview

This contract defines the schema for merged text output when multiple documents belonging to the same patient are combined before semantic chunking. It preserves per-document provenance so downstream chunking can maintain source references even when chunks span document boundaries.

## Schema: `merged_text.schema.json`

### Purpose

Defines the structure for merged text from patient-level multi-document processing, including:
- Patient identification
- Source document ordering
- Merged segments with per-document provenance

### Key Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `schema_version` | string | Yes | Schema version (currently "1.0") |
| `patient_id` | string (UUID) | Yes | Patient identifier |
| `merge_timestamp` | string | No | ISO 8601 timestamp of merge |
| `source_documents` | array | Yes | Ordered list of source document IDs |
| `merged_segments` | array | Yes | Array of merged text segments |

### Merged Segment Structure

Each segment contains:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `text` | string | Yes | The text content |
| `document_id` | string | Yes | Source document identifier |
| `document_location.page` | integer | No | 1-indexed page number |
| `document_location.section` | string | No | Section identifier |
| `document_location.coordinates` | object | No | Bounding box |
| `segment_index` | integer | No | Zero-based merge order index |
| `is_document_boundary` | boolean | No | True if first segment of a document |

## Usage

### Producer: AI Worker

The AI Worker produces merged text after:
1. Receiving a job with `payload.patient_id` and `payload.document_ids`
2. Loading extracted text for each document
3. Merging segments in stable order while preserving provenance

### Consumer: Downstream Chunking

Chunking logic can:
- Use `document_id` to maintain citations
- Use `is_document_boundary` to handle cross-document chunks
- Preserve `document_location` for precise source references

## Versioning

- **v1.0**: Initial release with document provenance and boundary markers

## Related Contracts

- `contracts/jobs/v1/job.schema.json`: Job payload includes `patient_id`, `document_ids`, and `patient_identifiers`
- `contracts/text_extraction/v1/extracted_text.schema.json`: Input format for individual document extraction
