# Retrieval Contract v1 Migration

## Version Information

- **Version**: 1.0
- **Release Date**: 2026-01-16
- **Status**: Initial Release

## Overview

This document describes the initial release of the retrieval result contract for top-K chunk similarity search.

## Changes

### Initial Release (v1.0)

This is the initial version of the retrieval contract. No migration from previous versions is required.

#### New Features

1. **Retrieval Result Schema**: Defines the structure for similarity search results
2. **Score Semantics**: Cosine similarity (1 - distance) with range 0.0-1.0
3. **K Clamping**: Results clamped to 10-15 range for consistent performance
4. **Deterministic Ordering**: Tie-breaking by chunk ID for reproducible results

## Consumer Impact

### New Consumers

New consumers should implement against `contracts/retrieval/v1/retrieval_result.schema.json`.

Key implementation notes:
- Validate `schema_version` equals "1.0"
- Handle empty `results` array gracefully
- Use `rank` for display ordering (1 = most similar)
- Use `score` for similarity thresholds if needed

### Existing Consumers

No existing consumers - this is the initial release.

## Validation

Use JSON Schema Draft-07 validation against `retrieval_result.schema.json` to ensure compliance.

## Dependencies

- Requires `document_chunks` table with `vector(768)` column
- Requires pgvector extension with HNSW index for efficient retrieval
- Subject to DR-005 Row-Level Security policies
