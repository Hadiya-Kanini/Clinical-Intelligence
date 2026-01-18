"""Embedding generation orchestration with normalization, retry, and rate limiting."""

import math
from dataclasses import dataclass, field
from datetime import datetime, timezone
from typing import List, Optional, Dict, Any, Callable

from tenacity import (
    retry,
    stop_after_attempt,
    wait_exponential,
    retry_if_exception_type,
)

from .gemini_embeddings_client import GeminiEmbeddingsClient
from .rate_limiter import RateLimiter


TRANSIENT_ERROR_CODES = [429, 500, 502, 503, 504]


@dataclass
class ProvenanceEntry:
    """Provenance metadata for embedding result."""
    
    document_id: str
    page: Optional[int] = None
    section: Optional[str] = None
    coordinates: Optional[dict] = None
    start_offset: Optional[int] = None
    end_offset: Optional[int] = None
    
    def to_dict(self) -> dict:
        result = {"document_id": self.document_id}
        if self.page is not None:
            result["page"] = self.page
        if self.section is not None:
            result["section"] = self.section
        if self.coordinates is not None:
            result["coordinates"] = self.coordinates
        if self.start_offset is not None:
            result["start_offset"] = self.start_offset
        if self.end_offset is not None:
            result["end_offset"] = self.end_offset
        return result


@dataclass
class EmbeddingResultItem:
    """Embedding result for a single chunk."""
    
    chunk_index: int
    status: str
    embedding: Optional[List[float]] = None
    normalized: Optional[bool] = None
    embedding_model: Optional[str] = None
    embedding_dimensions: Optional[int] = None
    document_id: Optional[str] = None
    text_content: Optional[str] = None
    token_count: Optional[int] = None
    chunk_hash: Optional[str] = None
    provenance: Optional[List[ProvenanceEntry]] = None
    error_code: Optional[str] = None
    error_message: Optional[str] = None
    
    def to_dict(self) -> dict:
        result = {
            "chunk_index": self.chunk_index,
            "status": self.status
        }
        if self.embedding is not None:
            result["embedding"] = self.embedding
        if self.normalized is not None:
            result["normalized"] = self.normalized
        if self.embedding_model is not None:
            result["embedding_model"] = self.embedding_model
        if self.embedding_dimensions is not None:
            result["embedding_dimensions"] = self.embedding_dimensions
        if self.document_id is not None:
            result["document_id"] = self.document_id
        if self.text_content is not None:
            result["text_content"] = self.text_content
        if self.token_count is not None:
            result["token_count"] = self.token_count
        if self.chunk_hash is not None:
            result["chunk_hash"] = self.chunk_hash
        if self.provenance is not None:
            result["provenance"] = [p.to_dict() for p in self.provenance]
        if self.error_code is not None:
            result["error_code"] = self.error_code
        if self.error_message is not None:
            result["error_message"] = self.error_message
        return result


@dataclass
class EmbeddingBatchResult:
    """Complete embedding batch result aligned to embedding_result.schema.json."""
    
    patient_id: str
    results: List[EmbeddingResultItem] = field(default_factory=list)
    schema_version: str = "1.0"
    embedding_timestamp: Optional[str] = None
    embedding_config: Optional[dict] = None
    source_documents: Optional[List[str]] = None
    
    def __post_init__(self):
        if self.embedding_timestamp is None:
            self.embedding_timestamp = datetime.now(timezone.utc).isoformat()
    
    def to_dict(self) -> dict:
        result = {
            "schema_version": self.schema_version,
            "patient_id": self.patient_id,
            "results": [r.to_dict() for r in self.results]
        }
        if self.embedding_timestamp is not None:
            result["embedding_timestamp"] = self.embedding_timestamp
        if self.embedding_config is not None:
            result["embedding_config"] = self.embedding_config
        if self.source_documents is not None:
            result["source_documents"] = self.source_documents
        return result


def normalize_embedding(embedding: List[float]) -> List[float]:
    """
    Apply L2 normalization to an embedding vector.
    
    Args:
        embedding: Raw embedding vector.
    
    Returns:
        Normalized embedding with unit length.
    """
    norm = math.sqrt(sum(x * x for x in embedding))
    if norm > 0:
        return [x / norm for x in embedding]
    return embedding


def _is_transient_error(exception: Exception) -> bool:
    """Check if an exception is a transient error that should be retried."""
    error_str = str(exception).lower()
    if "429" in error_str or "rate limit" in error_str:
        return True
    if any(str(code) in error_str for code in [500, 502, 503, 504]):
        return True
    if "timeout" in error_str or "connection" in error_str:
        return True
    return False


class TransientAPIError(Exception):
    """Exception for transient API errors that should be retried."""
    pass


class PermanentAPIError(Exception):
    """Exception for permanent API errors that should not be retried."""
    pass


def _create_retry_decorator(max_attempts: int):
    """Create a retry decorator with configurable max attempts."""
    return retry(
        stop=stop_after_attempt(max_attempts),
        wait=wait_exponential(multiplier=1, min=1, max=60),
        retry=retry_if_exception_type(TransientAPIError),
        reraise=True
    )


def generate_embeddings(
    chunks: List[Dict[str, Any]],
    patient_id: str,
    client: GeminiEmbeddingsClient,
    rate_limiter: RateLimiter,
    max_retries: int = 3,
    include_text_content: bool = False,
    source_documents: Optional[List[str]] = None
) -> EmbeddingBatchResult:
    """
    Generate embeddings for a batch of chunks.
    
    Args:
        chunks: List of chunk dicts from chunking contract.
        patient_id: Patient UUID.
        client: Gemini embeddings client.
        rate_limiter: Rate limiter for API calls.
        max_retries: Maximum retry attempts for transient errors.
        include_text_content: Whether to include text in results.
        source_documents: Optional list of source document IDs.
    
    Returns:
        EmbeddingBatchResult with results for each chunk.
    """
    results = []
    retry_decorator = _create_retry_decorator(max_retries)
    
    for chunk in chunks:
        chunk_index = chunk.get("chunk_index", 0)
        text = chunk.get("text", "")
        token_count = chunk.get("token_count")
        chunk_hash = chunk.get("chunk_hash")
        provenance_dicts = chunk.get("provenance", [])
        
        provenance = []
        primary_doc_id = None
        for p in provenance_dicts:
            entry = ProvenanceEntry(
                document_id=p.get("document_id", ""),
                page=p.get("page"),
                section=p.get("section"),
                coordinates=p.get("coordinates"),
                start_offset=p.get("start_offset"),
                end_offset=p.get("end_offset")
            )
            provenance.append(entry)
            if primary_doc_id is None:
                primary_doc_id = entry.document_id
        
        try:
            @retry_decorator
            def embed_with_retry():
                rate_limiter.acquire()
                try:
                    return client.embed_content(text)
                except Exception as e:
                    if _is_transient_error(e):
                        raise TransientAPIError(str(e)) from e
                    raise PermanentAPIError(str(e)) from e
            
            raw_embedding = embed_with_retry()
            normalized_embedding = normalize_embedding(raw_embedding)
            
            result_item = EmbeddingResultItem(
                chunk_index=chunk_index,
                status="success",
                embedding=normalized_embedding,
                normalized=True,
                embedding_model=client.model,
                embedding_dimensions=client.output_dimensions,
                document_id=primary_doc_id,
                token_count=token_count,
                chunk_hash=chunk_hash,
                provenance=provenance if provenance else None
            )
            
            if include_text_content:
                result_item.text_content = text
            
        except TransientAPIError as e:
            result_item = EmbeddingResultItem(
                chunk_index=chunk_index,
                status="failed",
                document_id=primary_doc_id,
                token_count=token_count,
                chunk_hash=chunk_hash,
                provenance=provenance if provenance else None,
                error_code="TRANSIENT_ERROR_MAX_RETRIES",
                error_message=f"Failed after {max_retries} retries: {str(e)}"
            )
        
        except PermanentAPIError as e:
            result_item = EmbeddingResultItem(
                chunk_index=chunk_index,
                status="failed",
                document_id=primary_doc_id,
                token_count=token_count,
                chunk_hash=chunk_hash,
                provenance=provenance if provenance else None,
                error_code="PERMANENT_ERROR",
                error_message=str(e)
            )
        
        except Exception as e:
            result_item = EmbeddingResultItem(
                chunk_index=chunk_index,
                status="failed",
                document_id=primary_doc_id,
                token_count=token_count,
                chunk_hash=chunk_hash,
                provenance=provenance if provenance else None,
                error_code="UNEXPECTED_ERROR",
                error_message=str(e)
            )
        
        results.append(result_item)
    
    return EmbeddingBatchResult(
        patient_id=patient_id,
        results=results,
        embedding_config={
            "embedding_model": client.model,
            "embedding_dimensions": client.output_dimensions
        },
        source_documents=source_documents
    )
