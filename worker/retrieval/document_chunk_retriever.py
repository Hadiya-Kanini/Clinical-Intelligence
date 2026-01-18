"""
Document chunk retrieval using pgvector cosine similarity.

Implements top-K retrieval with deterministic ordering and K clamping.
"""

from dataclasses import dataclass
from typing import List, Optional
import logging

try:
    import psycopg
except ImportError as e:
    raise ImportError(
        "Missing dependency 'psycopg'. Install with: pip install psycopg[binary]"
    ) from e


logger = logging.getLogger(__name__)


MIN_K = 10
MAX_K = 15
DEFAULT_K = 15
EMBEDDING_DIMENSIONS = 768


@dataclass
class RetrievedChunk:
    """Represents a retrieved document chunk from similarity search."""
    chunk_id: str
    document_id: str
    text_content: str
    page: Optional[int]
    section: Optional[str]
    coordinates: Optional[str]
    score: float
    rank: int


class DocumentChunkRetriever:
    """
    Retrieves document chunks using pgvector cosine similarity.
    
    Uses parameterized queries and relies on DR-005/RLS for access control.
    """

    RETRIEVAL_SQL = """
        SELECT
            "Id" as chunk_id,
            "DocumentId" as document_id,
            "TextContent" as text_content,
            "Page" as page,
            "Section" as section,
            "Coordinates" as coordinates,
            1 - ("Embedding" <=> %(embedding)s::vector) as score
        FROM document_chunks
        WHERE "Embedding" IS NOT NULL
        ORDER BY "Embedding" <=> %(embedding)s::vector ASC, "Id" ASC
        LIMIT %(k)s
    """

    RETRIEVAL_BY_DOCUMENT_SQL = """
        SELECT
            "Id" as chunk_id,
            "DocumentId" as document_id,
            "TextContent" as text_content,
            "Page" as page,
            "Section" as section,
            "Coordinates" as coordinates,
            1 - ("Embedding" <=> %(embedding)s::vector) as score
        FROM document_chunks
        WHERE "Embedding" IS NOT NULL
          AND "DocumentId" = %(document_id)s
        ORDER BY "Embedding" <=> %(embedding)s::vector ASC, "Id" ASC
        LIMIT %(k)s
    """

    def __init__(self, connection_string: str):
        """
        Initialize the document chunk retriever.
        
        Args:
            connection_string: PostgreSQL connection string.
        
        Raises:
            ValueError: If connection_string is empty or None.
        """
        if not connection_string or not connection_string.strip():
            raise ValueError("DATABASE_CONNECTION_STRING is required")
        self._connection_string = connection_string

    def retrieve_top_k_chunks(
        self,
        query_embedding: List[float],
        k: int = DEFAULT_K,
        document_id: Optional[str] = None,
        similarity_threshold: Optional[float] = None
    ) -> List[RetrievedChunk]:
        """
        Retrieve the top-K most similar document chunks.
        
        Args:
            query_embedding: 768-dimensional query embedding vector.
            k: Number of results to return (clamped to 10-15 range).
            document_id: Optional document ID to scope the search.
            similarity_threshold: Optional minimum similarity score (0-1).
        
        Returns:
            List of RetrievedChunk objects ordered by similarity (most similar first).
        
        Raises:
            ValueError: If embedding is invalid (wrong dimensions or not numeric).
            psycopg.Error: If database operation fails.
        """
        self._validate_embedding(query_embedding)
        clamped_k = self._clamp_k(k)
        
        embedding_str = self._format_embedding(query_embedding)
        
        logger.debug(
            "Retrieving top %d chunks (requested: %d, document_id: %s)",
            clamped_k, k, document_id
        )

        try:
            with psycopg.connect(self._connection_string) as conn:
                with conn.cursor() as cur:
                    if document_id:
                        cur.execute(
                            self.RETRIEVAL_BY_DOCUMENT_SQL,
                            {
                                "embedding": embedding_str,
                                "k": clamped_k,
                                "document_id": document_id
                            }
                        )
                    else:
                        cur.execute(
                            self.RETRIEVAL_SQL,
                            {
                                "embedding": embedding_str,
                                "k": clamped_k
                            }
                        )
                    
                    rows = cur.fetchall()
                    
                    results = []
                    for rank, row in enumerate(rows, start=1):
                        chunk = RetrievedChunk(
                            chunk_id=str(row[0]),
                            document_id=str(row[1]),
                            text_content=row[2],
                            page=row[3],
                            section=row[4],
                            coordinates=row[5],
                            score=float(row[6]) if row[6] is not None else 0.0,
                            rank=rank
                        )
                        
                        if similarity_threshold is not None:
                            if chunk.score >= similarity_threshold:
                                results.append(chunk)
                        else:
                            results.append(chunk)
                    
                    logger.debug("Retrieved %d chunks", len(results))
                    return results
                    
        except psycopg.Error as e:
            logger.error(
                "Database error retrieving chunks: %s",
                str(e)[:200]
            )
            raise

    def _validate_embedding(self, embedding: List[float]) -> None:
        """
        Validate the query embedding.
        
        Args:
            embedding: The embedding to validate.
        
        Raises:
            ValueError: If embedding is invalid.
        """
        if not embedding:
            raise ValueError("Query embedding is required")
        
        if len(embedding) != EMBEDDING_DIMENSIONS:
            raise ValueError(
                f"Query embedding must be {EMBEDDING_DIMENSIONS} dimensions, "
                f"got {len(embedding)}"
            )
        
        for i, val in enumerate(embedding):
            if not isinstance(val, (int, float)):
                raise ValueError(
                    f"Embedding value at index {i} is not numeric: {type(val)}"
                )

    def _clamp_k(self, k: int) -> int:
        """Clamp k to the valid range [MIN_K, MAX_K]."""
        if k < MIN_K:
            return MIN_K
        if k > MAX_K:
            return MAX_K
        return k

    def _format_embedding(self, embedding: List[float]) -> str:
        """Format embedding as PostgreSQL vector string."""
        return "[" + ",".join(str(v) for v in embedding) + "]"
