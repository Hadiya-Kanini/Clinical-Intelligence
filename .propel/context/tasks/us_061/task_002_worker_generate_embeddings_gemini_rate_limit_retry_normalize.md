# Task - TASK_061_002

## Requirement Reference
- User Story: us_061
- Story Location: .propel/context/tasks/us_061/us_061.md
- Acceptance Criteria: 
    - Given text chunks, When processed, Then 768-dimensional embeddings are generated using text-embedding-004 model (FR-033).
    - Given embedding generation, When API is called, Then rate limits are respected (15 RPM free tier).
    - Given API errors, When they occur, Then retry with exponential backoff is applied.

## Task Overview
Implement embedding generation in the AI Worker using the Google Gemini Embeddings API with a configurable model and output dimensionality set to 768. The worker must apply exponential backoff retries for transient failures and enforce a client-side rate limit compatible with a 15 RPM free-tier quota, while producing contract-compliant embedding outputs for each input chunk.

## Dependent Tasks
- .propel/context/tasks/us_060/task_002_worker_semantic_chunking_with_overlap.md (TASK_060_002)
- .propel/context/tasks/us_060/task_003_worker_chunking_pipeline_wiring_and_schema_validation.md (TASK_060_003)
- .propel/context/tasks/us_061/task_001_contracts_embedding_output_schema.md (TASK_061_001)

## Impacted Components
- [MODIFY: worker/requirements.txt]
- [MODIFY: worker/config.py]
- [CREATE: worker/embeddings/gemini_embeddings_client.py]
- [CREATE: worker/embeddings/rate_limiter.py]
- [CREATE: worker/embeddings/embedding_generation.py]
- [CREATE: worker/tests/test_embedding_generation.py]

## Implementation Plan
- Add worker dependencies required to call the Gemini Embeddings API:
  - Select the project’s preferred Google client library for embeddings (API-key based) and pin it in `worker/requirements.txt`.
- Extend `worker/config.py` to support embedding configuration via environment variables (with safe defaults):
  - `GEMINI_EMBEDDING_MODEL` (default should follow US_061 requirement, e.g., `text-embedding-004`)
  - `GEMINI_EMBEDDING_OUTPUT_DIMENSIONS` (default `768`)
  - `GEMINI_RPM_LIMIT` (default `15`)
  - Keep `GEMINI_API_KEY` as required (already present).
- Implement `worker/embeddings/gemini_embeddings_client.py`:
  - Provide a small, testable wrapper responsible only for calling the embeddings endpoint and returning raw vectors.
  - Ensure calls are parameterized by model and output dimensionality.
- Implement `worker/embeddings/rate_limiter.py`:
  - Add a simple RPM limiter (token bucket or “sleep-until-next-slot”) ensuring the worker never exceeds the configured RPM.
  - Ensure limiter behavior is deterministic in tests (inject a clock/sleeper).
- Implement `worker/embeddings/embedding_generation.py`:
  - Accept chunk outputs from US_060 (contract-aligned chunk objects).
  - For each chunk, call the Gemini embeddings client to generate a 768-length vector.
  - Normalize embeddings for 768-dimensional output (L2 normalization) and set `normalized=true` in the output contract.
  - Apply exponential backoff retries on transient errors (e.g., 429, 5xx) with a maximum attempt count; surface permanent failures as contract-level failed results.
  - Edge case: if a chunk text exceeds API limits, fail that chunk result explicitly with a clear error code/message (do not silently truncate) and rely on upstream chunk sizing to prevent this.
- Add unit tests in `worker/tests/test_embedding_generation.py` validating:
  - Success path generates vectors of length 768.
  - Rate limiting prevents >15 requests per minute when generating embeddings for N chunks.
  - Retry behavior is invoked on transient errors and stops after max attempts.
  - Normalization is applied (norm approximately 1.0 for successful 768-d outputs).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/requirements.txt | Add/confirm Google Gemini embeddings client dependency and any required HTTP/retry helpers compatible with the worker runtime |
| MODIFY | worker/config.py | Add embedding model/dimension/RPM configuration (env-driven) while keeping `GEMINI_API_KEY` required |
| CREATE | worker/embeddings/gemini_embeddings_client.py | Minimal client wrapper for calling the Gemini embeddings API with configurable model + output dimensionality |
| CREATE | worker/embeddings/rate_limiter.py | RPM limiter utility used by embedding generation to respect free-tier rate limits |
| CREATE | worker/embeddings/embedding_generation.py | Orchestrates per-chunk embedding generation, normalization, retry/backoff, and mapping to the embeddings contract output |
| CREATE | worker/tests/test_embedding_generation.py | Unit tests covering 768-length output, rate limiting behavior, retry/backoff behavior, and normalization |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://ai.google.dev/gemini-api/docs/embeddings
- https://ai.google.dev/gemini-api/docs/rate-limits

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate embedding generation returns 768-length vectors and sets `normalized=true` after L2 normalization.
- Validate the embedding generator respects the configured RPM limit under batch loads.
- Validate retry/backoff behavior produces successful results for transient failures and produces contract-level failed results for permanent failures.

## Implementation Checklist
- [ ] Update `worker/requirements.txt` with the Gemini embeddings client dependency
- [ ] Extend `worker/config.py` to include embedding model, dimensions (768), and RPM limit configuration
- [ ] Implement `worker/embeddings/gemini_embeddings_client.py` wrapper around the embeddings endpoint
- [ ] Implement `worker/embeddings/rate_limiter.py` to enforce 15 RPM (configurable)
- [ ] Implement `worker/embeddings/embedding_generation.py` with normalization + retry/backoff
- [ ] Add unit tests for success, rate limiting, retry behavior, and normalization
