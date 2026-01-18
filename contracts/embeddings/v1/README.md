# Embeddings Contract - v1

## Overview

This contract defines the schema for embedding generation output produced by the AI Worker. It represents 768-dimensional embedding vectors along with full chunk provenance metadata required for downstream storage in `document_chunks` and traceability.

## Schema: `embedding_result.schema.json`

### Purpose

Defines the structure for embedding generation output, including:
- Patient identification
- Embedding configuration (model, dimensions)
- Per-chunk embedding results with success/failure status
- Chunk provenance for citations and storage

### Key Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `schema_version` | string | Yes | Schema version (currently "1.0") |
| `patient_id` | string (UUID) | Yes | Patient identifier |
| `embedding_timestamp` | string | No | ISO 8601 timestamp of embedding generation |
| `embedding_config` | object | No | Configuration used for embedding generation |
| `source_documents` | array | No | List of source document IDs |
| `results` | array | Yes | Array of embedding results per chunk |

### Embedding Result Item Structure

Each result item contains:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `chunk_index` | integer | Yes | Zero-based chunk sequence index |
| `status` | string | Yes | "success" or "failed" |
| `embedding` | array[768] | Yes (if success) | 768-dimensional float vector |
| `normalized` | boolean | Yes (if success) | Whether L2 normalization was applied |
| `embedding_model` | string | No | Model used (e.g., text-embedding-004) |
| `embedding_dimensions` | integer | No | Dimensionality (768) |
| `document_id` | string | No | Primary source document ID |
| `text_content` | string | No | Original chunk text (optional) |
| `token_count` | integer | No | Tokens in source chunk |
| `chunk_hash` | string | No | Hash for deduplication |
| `provenance` | array | No | Source provenance entries |
| `error_code` | string | No (if failed) | Error code for failures |
| `error_message` | string | No (if failed) | Error message for failures |

## Normalization Expectations

For 768-dimensional vectors:
- **L2 normalization is recommended** for cosine similarity search
- When `normalized=true`, the vector has unit length (L2 norm ≈ 1.0)
- pgvector's `<=>` operator works optimally with normalized vectors

```python
import numpy as np

def normalize_embedding(embedding: list[float]) -> list[float]:
    arr = np.array(embedding)
    norm = np.linalg.norm(arr)
    if norm > 0:
        arr = arr / norm
    return arr.tolist()
```

## Partial Failure Semantics

The contract supports mixed success/failure results:

```json
{
  "schema_version": "1.0",
  "patient_id": "00000000-0000-0000-0000-000000000001",
  "results": [
    {
      "chunk_index": 0,
      "status": "success",
      "embedding": [0.1, 0.2, ...],
      "normalized": true
    },
    {
      "chunk_index": 1,
      "status": "failed",
      "error_code": "RATE_LIMIT_EXCEEDED",
      "error_message": "API rate limit exceeded after max retries"
    }
  ]
}
```

### Retry Responsibility

- **Producer (AI Worker)**: Applies exponential backoff retries for transient errors (429, 5xx)
- **Consumer (Storage)**: May re-queue failed chunks for retry or mark as permanently failed
- **Contract boundary**: Failed results are explicitly represented, not silently dropped

## Usage

### Producer: AI Worker

The AI Worker produces embedding results after:
1. Receiving chunked text (US_060)
2. Calling Gemini Embeddings API for each chunk
3. Applying L2 normalization to 768-d vectors
4. Handling rate limits and retries

### Consumer: Storage (US_062)

Storage layer can:
- Insert successful embeddings into `document_chunks` table
- Use `chunk_hash` for deduplication
- Preserve `provenance` for citation support
- Handle failed results appropriately (retry queue or error log)

## Versioning

- **v1.0**: Initial release with 768-d vectors and partial failure support

## Related Contracts

- `contracts/chunking/v1/chunked_text.schema.json`: Input format for embedding generation
- Database schema: `document_chunks` table with pgvector column
