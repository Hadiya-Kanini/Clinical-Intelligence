"""
Document chunk storage for persisting chunks with embeddings to PostgreSQL.

Implements idempotent upsert logic using (DocumentId, ChunkHash) dedupe constraint.
"""

from dataclasses import dataclass
from typing import List, Optional
import uuid
import logging

try:
    import psycopg
    from psycopg import sql
except ImportError as e:
    raise ImportError(
        "Missing dependency 'psycopg'. Install with: pip install psycopg[binary]"
    ) from e


logger = logging.getLogger(__name__)


@dataclass
class ChunkRecord:
    """Represents a document chunk record for persistence."""
    document_id: str
    text_content: str
    embedding: List[float]
    chunk_hash: str
    page: Optional[int] = None
    section: Optional[str] = None
    coordinates: Optional[str] = None
    token_count: Optional[int] = None
    chunk_id: Optional[str] = None


class DocumentChunkStore:
    """
    Encapsulates insert/upsert logic for document_chunks table.
    
    Uses PostgreSQL's ON CONFLICT for idempotent upserts based on
    the unique constraint on (DocumentId, ChunkHash).
    """

    UPSERT_SQL = """
        INSERT INTO document_chunks (
            "Id", "DocumentId", "Page", "Section", "Coordinates",
            "TextContent", "Embedding", "TokenCount", "ChunkHash"
        ) VALUES (
            %(id)s, %(document_id)s, %(page)s, %(section)s, %(coordinates)s,
            %(text_content)s, %(embedding)s::vector, %(token_count)s, %(chunk_hash)s
        )
        ON CONFLICT ("DocumentId", "ChunkHash") DO UPDATE SET
            "TextContent" = EXCLUDED."TextContent",
            "Embedding" = EXCLUDED."Embedding",
            "TokenCount" = EXCLUDED."TokenCount",
            "Page" = EXCLUDED."Page",
            "Section" = EXCLUDED."Section",
            "Coordinates" = EXCLUDED."Coordinates"
    """

    PGVECTOR_CHECK_SQL = """
        SELECT EXISTS (
            SELECT 1 FROM pg_extension WHERE extname = 'vector'
        )
    """

    def __init__(self, connection_string: str):
        """
        Initialize the document chunk store.
        
        Args:
            connection_string: PostgreSQL connection string.
        
        Raises:
            ValueError: If connection_string is empty or None.
        """
        if not connection_string or not connection_string.strip():
            raise ValueError("DATABASE_CONNECTION_STRING is required")
        self._connection_string = connection_string

    def verify_pgvector_extension(self) -> bool:
        """
        Verify that the pgvector extension is installed.
        
        Returns:
            True if pgvector is available, False otherwise.
        """
        try:
            with psycopg.connect(self._connection_string) as conn:
                with conn.cursor() as cur:
                    cur.execute(self.PGVECTOR_CHECK_SQL)
                    result = cur.fetchone()
                    return result[0] if result else False
        except Exception as e:
            logger.warning("Failed to verify pgvector extension: %s", e)
            return False

    def persist_chunks(self, chunks: List[ChunkRecord]) -> int:
        """
        Persist a batch of chunks to the database.
        
        Uses a single transaction for the entire batch. If any insert fails,
        the entire transaction is rolled back.
        
        Args:
            chunks: List of ChunkRecord objects to persist.
        
        Returns:
            Number of chunks processed (inserted or updated).
        
        Raises:
            psycopg.Error: If database operation fails.
            ValueError: If chunks list is empty or contains invalid data.
        """
        if not chunks:
            return 0

        processed_count = 0

        try:
            with psycopg.connect(self._connection_string) as conn:
                with conn.cursor() as cur:
                    for chunk in chunks:
                        self._validate_chunk(chunk)
                        
                        chunk_id = chunk.chunk_id or str(uuid.uuid4())
                        embedding_str = self._format_embedding(chunk.embedding)
                        
                        params = {
                            "id": chunk_id,
                            "document_id": chunk.document_id,
                            "page": chunk.page,
                            "section": chunk.section,
                            "coordinates": chunk.coordinates,
                            "text_content": chunk.text_content,
                            "embedding": embedding_str,
                            "token_count": chunk.token_count,
                            "chunk_hash": chunk.chunk_hash,
                        }
                        
                        cur.execute(self.UPSERT_SQL, params)
                        processed_count += 1
                    
                    conn.commit()
                    
        except psycopg.Error as e:
            logger.error(
                "Database error persisting chunks (batch rolled back): %s",
                str(e)[:200]
            )
            raise

        logger.info("Persisted %d chunks successfully", processed_count)
        return processed_count

    def persist_chunks_for_document(
        self,
        document_id: str,
        chunks: List[ChunkRecord]
    ) -> int:
        """
        Persist chunks for a specific document.
        
        Convenience method that ensures all chunks have the correct document_id.
        
        Args:
            document_id: The document ID to associate with all chunks.
            chunks: List of ChunkRecord objects to persist.
        
        Returns:
            Number of chunks processed.
        """
        for chunk in chunks:
            chunk.document_id = document_id
        
        return self.persist_chunks(chunks)

    def _validate_chunk(self, chunk: ChunkRecord) -> None:
        """
        Validate a chunk record before persistence.
        
        Args:
            chunk: The chunk to validate.
        
        Raises:
            ValueError: If chunk is invalid.
        """
        if not chunk.document_id:
            raise ValueError("Chunk document_id is required")
        if not chunk.text_content:
            raise ValueError("Chunk text_content is required")
        if not chunk.chunk_hash:
            raise ValueError("Chunk chunk_hash is required")
        if not chunk.embedding:
            raise ValueError("Chunk embedding is required")
        if len(chunk.embedding) != 768:
            raise ValueError(
                f"Embedding must be 768 dimensions, got {len(chunk.embedding)}"
            )

    def _format_embedding(self, embedding: List[float]) -> str:
        """
        Format embedding as PostgreSQL vector string.
        
        Args:
            embedding: List of float values.
        
        Returns:
            String representation for pgvector (e.g., "[0.1,0.2,...]").
        """
        return "[" + ",".join(str(v) for v in embedding) + "]"
