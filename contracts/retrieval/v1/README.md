# Retrieval Result Contract v1

This contract defines the schema for top-K chunk retrieval results from pgvector cosine similarity search.

## Overview

The retrieval result schema (`retrieval_result.schema.json`) describes the output format for document chunk similarity searches used in the RAG (Retrieval-Augmented Generation) pipeline.

## Score Semantics

- **Score Type**: Cosine similarity (not distance)
- **Score Range**: 0.0 to 1.0
- **Interpretation**: Higher scores indicate greater similarity
  - `1.0` = Identical vectors
  - `0.0` = Orthogonal vectors (no similarity)
- **Calculation**: `score = 1 - cosine_distance`

## K Clamping Rules

The number of results (`k`) is clamped to ensure consistent performance:

- **Minimum K**: 10
- **Maximum K**: 15
- **Default K**: 15

If a request specifies `k < 10`, it will be clamped to 10.
If a request specifies `k > 15`, it will be clamped to 15.

## Tie-Breaking

When multiple chunks have identical similarity scores, results are ordered deterministically by:

1. **Primary**: Cosine distance (ascending - most similar first)
2. **Secondary**: Chunk ID (ascending - for deterministic ordering)

This ensures consistent, reproducible results across identical queries.

## Empty Results

Empty results are valid and represented as:

```json
{
  "schema_version": "1.0",
  "query_embedding_dimensions": 768,
  "k_requested": 15,
  "k_returned": 0,
  "results": []
}
```

## Field Descriptions

### Root Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `schema_version` | string | Yes | Schema version (currently "1.0") |
| `query_embedding_dimensions` | integer | Yes | Must be 768 |
| `k_requested` | integer | Yes | Original k value requested |
| `k_returned` | integer | Yes | Actual number of results returned |
| `query_hash` | string | No | SHA-256 hash of query text for audit |
| `results` | array | Yes | Array of RetrievedChunk objects |

### RetrievedChunk Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `chunk_id` | string (uuid) | Yes | Unique chunk identifier |
| `document_id` | string (uuid) | Yes | Source document identifier |
| `text_content` | string | Yes | Chunk text content |
| `page` | integer/null | No | Page number (1-indexed) |
| `section` | string/null | No | Section identifier |
| `coordinates` | string/null | No | Position coordinates |
| `rank` | integer | Yes | Result rank (1 = most similar) |
| `score` | number | Yes | Similarity score (0.0-1.0) |

## Example

```json
{
  "schema_version": "1.0",
  "query_embedding_dimensions": 768,
  "k_requested": 15,
  "k_returned": 3,
  "results": [
    {
      "chunk_id": "550e8400-e29b-41d4-a716-446655440001",
      "document_id": "550e8400-e29b-41d4-a716-446655440000",
      "text_content": "Patient presents with chest pain...",
      "page": 1,
      "section": "Chief Complaint",
      "coordinates": null,
      "rank": 1,
      "score": 0.92
    },
    {
      "chunk_id": "550e8400-e29b-41d4-a716-446655440002",
      "document_id": "550e8400-e29b-41d4-a716-446655440000",
      "text_content": "History of cardiac issues...",
      "page": 2,
      "section": "Medical History",
      "coordinates": null,
      "rank": 2,
      "score": 0.85
    },
    {
      "chunk_id": "550e8400-e29b-41d4-a716-446655440003",
      "document_id": "550e8400-e29b-41d4-a716-446655440000",
      "text_content": "ECG shows normal sinus rhythm...",
      "page": 3,
      "section": "Diagnostics",
      "coordinates": null,
      "rank": 3,
      "score": 0.78
    }
  ]
}
```

## Access Control

Retrieval results are subject to Row-Level Security (RLS) per DR-005. Users can only retrieve chunks from documents they are authorized to access.
