# Chunking Contract - v1

## Overview

This contract defines the schema for chunked text output produced by semantic chunking of patient-level merged text. It preserves per-chunk provenance so downstream embedding and storage can maintain source references even when chunks span document boundaries.

## Schema: `chunked_text.schema.json`

### Purpose

Defines the structure for chunked text output from the AI Worker, including:
- Patient identification
- Chunking configuration (size targets, overlap)
- Chunk content with token counts
- Per-chunk provenance for citations

### Key Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `schema_version` | string | Yes | Schema version (currently "1.0") |
| `patient_id` | string (UUID) | Yes | Patient identifier |
| `chunking_timestamp` | string | No | ISO 8601 timestamp of chunking |
| `chunking_config` | object | No | Configuration used for chunking |
| `source_documents` | array | No | Ordered list of source document IDs |
| `chunks` | array | Yes | Array of text chunks with provenance |

### Chunk Structure

Each chunk contains:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `chunk_index` | integer | Yes | Zero-based chunk sequence index |
| `text` | string | Yes | The chunk text content |
| `token_count` | integer | No | Number of tokens in the chunk |
| `chunk_hash` | string | No | Hash for deduplication/integrity |
| `provenance` | array | Yes | Source provenance entries (min 1) |

### Provenance Entry Structure

Each provenance entry links chunk content to source documents:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `document_id` | string | Yes | Source document identifier |
| `page` | integer | No | 1-indexed page number |
| `section` | string | No | Section identifier |
| `coordinates` | object | No | Bounding box (x0, y0, x1, y1) |
| `start_offset` | integer | No | Character offset start in chunk |
| `end_offset` | integer | No | Character offset end in chunk |

## Multi-Document Chunk Representation

When a chunk spans multiple source documents (cross-document boundary), the `provenance` array contains multiple entries:

```json
{
  "chunk_index": 5,
  "text": "...end of doc1 content. Beginning of doc2 content...",
  "token_count": 750,
  "provenance": [
    {
      "document_id": "doc-001",
      "page": 3,
      "start_offset": 0,
      "end_offset": 150
    },
    {
      "document_id": "doc-002",
      "page": 1,
      "start_offset": 151,
      "end_offset": 500
    }
  ]
}
```

## Chunking Configuration

The `chunking_config` object documents the parameters used:

| Field | Description | TR-005 Requirement |
|-------|-------------|-------------------|
| `chunk_size_target_tokens` | Maximum tokens per chunk | 1000 |
| `chunk_size_min_tokens` | Minimum tokens per chunk | 500 |
| `chunk_overlap_tokens` | Overlap between adjacent chunks | 100 |

## Usage

### Producer: AI Worker

The AI Worker produces chunked text after:
1. Receiving merged patient text (US_059)
2. Applying `RecursiveCharacterTextSplitter` with token-aware sizing
3. Computing provenance for each chunk from source segments

### Consumer: Embedding Generation (US_061)

Embedding generation can:
- Process each chunk independently
- Preserve `chunk_index` for ordering
- Pass through provenance for downstream storage

### Consumer: Storage (US_062)

Storage layer can:
- Use `document_id` from provenance for citations
- Use `chunk_hash` for deduplication
- Store `token_count` for retrieval optimization

## Versioning

- **v1.0**: Initial release with multi-document provenance support

## Related Contracts

- `contracts/text_merge/v1/merged_text.schema.json`: Input format for chunking
- `contracts/embeddings/v1/embedding_result.schema.json`: Output format after embedding generation
