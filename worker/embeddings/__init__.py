"""Embeddings module for Gemini embedding generation with rate limiting and retry."""

from .gemini_embeddings_client import GeminiEmbeddingsClient
from .rate_limiter import RateLimiter
from .embedding_generation import (
    generate_embeddings,
    EmbeddingResultItem,
    EmbeddingBatchResult,
    normalize_embedding,
)

__all__ = [
    "GeminiEmbeddingsClient",
    "RateLimiter",
    "generate_embeddings",
    "EmbeddingResultItem",
    "EmbeddingBatchResult",
    "normalize_embedding",
]
