# Task - TASK_064_002

## Requirement Reference
- User Story: us_064
- Story Location: .propel/context/tasks/us_064/us_064.md
- Acceptance Criteria: 
    - Given retrieved chunks, When extraction runs, Then all categories are extracted in one Gemini call.
    - Given the extraction prompt, When designed, Then it includes structured output format for all categories.

## Task Overview
Implement the AI worker’s Gemini 2.5 Flash client call for entity extraction as a single request per extraction run. This task focuses on the outbound call behavior (one call only), request configuration, and resilience around rate limits/timeouts.

## Dependent Tasks
- [TASK_064_001 - Worker: single-call entity extraction prompt and schema alignment]
- [US_063 - Implement cosine similarity search for top-K retrieval]

## Impacted Components
- [MODIFY: worker/requirements.txt]
- [CREATE: worker/entity_extraction/gemini_client.py]
- [CREATE: worker/entity_extraction/extractor.py]
- [MODIFY: worker/config.py]
- [MODIFY: worker/main.py]
- [CREATE: worker/tests/test_gemini_extractor_single_call.py]

## Implementation Plan
- Add required Gemini SDK dependency to `worker/requirements.txt` (select one SDK and standardize):
  - Prefer the official Gemini API Python SDK used by the project.
- Extend worker configuration to support extraction settings:
  - Model name (default `gemini-2.5-flash`)
  - Request timeouts
  - Retry/backoff configuration for transient failures (esp. HTTP 429)
- Implement `GeminiClient` wrapper that:
  - Accepts `api_key` from `WorkerConfig`
  - Exposes `generate_content(prompt: str) -> str` (raw response text)
  - Enforces that extraction uses exactly one API call per run (no per-category calls)
- Implement `extract_entities_single_call(...)` orchestration that:
  - Accepts retrieved chunks + provenance
  - Uses `prompt_builder` (TASK_064_001) to build one prompt
  - Calls Gemini once
  - Returns raw response for parsing/validation (TASK_064_003)
- Rate limiting + resilience:
  - On 429, retry with exponential backoff up to a bounded max attempt count
  - On timeout/network errors, retry bounded times
  - Ensure error messages do not log raw chunk text (PHI) by default
- Add unit tests:
  - Stub/mock Gemini client and assert exactly one call is made per extraction invocation
  - Validate retry behavior triggers on simulated 429 and stops after max retries

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/requirements.txt | Add Gemini SDK dependency and any retry/backoff dependency selected for worker resilience |
| CREATE | worker/entity_extraction/gemini_client.py | Gemini client wrapper to perform a single extraction request with bounded retries and safe logging |
| CREATE | worker/entity_extraction/extractor.py | Orchestration function that builds one prompt and calls Gemini once per extraction run |
| MODIFY | worker/config.py | Add extraction-related config (model name, timeouts, retry limits) alongside `GEMINI_API_KEY` |
| MODIFY | worker/main.py | Add a minimal integration seam that can invoke single-call extraction given retrieved chunks |
| CREATE | worker/tests/test_gemini_extractor_single_call.py | Unit tests ensuring exactly one Gemini call per extraction run and bounded retry behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://ai.google.dev/gemini-api/docs
- https://cloud.google.com/docs/quota

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate that a single extraction invocation results in exactly one Gemini request.
- Validate bounded retries for rate limits/timeouts without unbounded looping.
- Validate logs/errors do not include raw chunk text by default.

## Implementation Checklist
- [ ] Add Gemini SDK dependency and validate import at runtime
- [ ] Extend `WorkerConfig` to include model/timeouts/retry settings
- [ ] Implement `GeminiClient` wrapper with safe error handling
- [ ] Implement `extract_entities_single_call(...)` orchestration using the prompt builder
- [ ] Add bounded retry/backoff on 429 and transient network/timeouts
- [ ] Add unit tests asserting exactly one outbound call per extraction
- [ ] Add unit tests validating bounded retry behavior and safe error messages
