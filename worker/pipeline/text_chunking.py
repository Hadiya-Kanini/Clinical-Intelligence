"""Semantic text chunking with token-aware sizing and provenance preservation."""

import hashlib
from dataclasses import dataclass, field
from datetime import datetime, timezone
from typing import List, Optional, Dict, Any, Tuple

import tiktoken
from langchain_text_splitters import RecursiveCharacterTextSplitter

from .patient_text_merge import MergedTextResult, MergedTextSegment


DEFAULT_CHUNK_SIZE_TARGET_TOKENS = 1000
DEFAULT_CHUNK_SIZE_MIN_TOKENS = 500
DEFAULT_CHUNK_OVERLAP_TOKENS = 100
DEFAULT_TOKENIZER_MODEL = "cl100k_base"
CHARS_PER_TOKEN_ESTIMATE = 4


@dataclass
class ProvenanceEntry:
    """Provenance metadata linking chunk content to source document location."""
    
    document_id: str
    page: Optional[int] = None
    section: Optional[str] = None
    coordinates: Optional[dict] = None
    start_offset: Optional[int] = None
    end_offset: Optional[int] = None
    
    def to_dict(self) -> dict:
        """Convert to dictionary matching chunked_text.schema.json provenance structure."""
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
class Chunk:
    """A single text chunk with sizing and provenance metadata."""
    
    chunk_index: int
    text: str
    provenance: List[ProvenanceEntry] = field(default_factory=list)
    token_count: Optional[int] = None
    chunk_hash: Optional[str] = None
    
    def to_dict(self) -> dict:
        """Convert to dictionary matching chunked_text.schema.json chunk structure."""
        result = {
            "chunk_index": self.chunk_index,
            "text": self.text,
            "provenance": [p.to_dict() for p in self.provenance]
        }
        if self.token_count is not None:
            result["token_count"] = self.token_count
        if self.chunk_hash is not None:
            result["chunk_hash"] = self.chunk_hash
        return result


@dataclass
class ChunkedTextResult:
    """Complete chunked text result aligned to chunked_text.schema.json."""
    
    patient_id: str
    chunks: List[Chunk] = field(default_factory=list)
    schema_version: str = "1.0"
    chunking_timestamp: Optional[str] = None
    chunking_config: Optional[dict] = None
    source_documents: Optional[List[str]] = None
    
    def __post_init__(self):
        if self.chunking_timestamp is None:
            self.chunking_timestamp = datetime.now(timezone.utc).isoformat()
    
    def to_dict(self) -> dict:
        """Convert to dictionary matching the chunked_text.schema.json structure."""
        result = {
            "schema_version": self.schema_version,
            "patient_id": self.patient_id,
            "chunks": [c.to_dict() for c in self.chunks]
        }
        if self.chunking_timestamp is not None:
            result["chunking_timestamp"] = self.chunking_timestamp
        if self.chunking_config is not None:
            result["chunking_config"] = self.chunking_config
        if self.source_documents is not None:
            result["source_documents"] = self.source_documents
        return result


class TokenCounter:
    """Token counter using tiktoken for consistent token measurement."""
    
    def __init__(self, model: str = DEFAULT_TOKENIZER_MODEL):
        self._encoding = tiktoken.get_encoding(model)
    
    def count_tokens(self, text: str) -> int:
        """Count tokens in text."""
        if not text:
            return 0
        return len(self._encoding.encode(text))
    
    def length_function(self, text: str) -> int:
        """Length function for use with LangChain splitters."""
        return self.count_tokens(text)


def _compute_chunk_hash(text: str) -> str:
    """Compute SHA-256 hash of chunk text for deduplication/integrity."""
    return hashlib.sha256(text.encode("utf-8")).hexdigest()[:16]


def _build_provenance_mapping(
    merged_result: MergedTextResult
) -> Tuple[str, List[Tuple[int, int, MergedTextSegment]]]:
    """
    Build merged text and provenance mapping from merged segments.
    
    Returns:
        Tuple of (merged_text, list of (start_offset, end_offset, segment))
    """
    text_parts = []
    provenance_map = []
    current_offset = 0
    
    for segment in merged_result.merged_segments:
        segment_text = segment.text
        if not segment_text:
            continue
        
        start_offset = current_offset
        text_parts.append(segment_text)
        current_offset += len(segment_text)
        end_offset = current_offset
        
        provenance_map.append((start_offset, end_offset, segment))
        
        text_parts.append("\n\n")
        current_offset += 2
    
    merged_text = "".join(text_parts).rstrip("\n")
    
    return merged_text, provenance_map


def _find_provenance_for_chunk(
    chunk_start: int,
    chunk_end: int,
    provenance_map: List[Tuple[int, int, MergedTextSegment]]
) -> List[ProvenanceEntry]:
    """
    Find all provenance entries that overlap with a chunk's character range.
    
    Args:
        chunk_start: Start character offset of chunk in merged text.
        chunk_end: End character offset of chunk in merged text.
        provenance_map: List of (start, end, segment) tuples.
    
    Returns:
        List of ProvenanceEntry objects for the chunk.
    """
    entries = []
    
    for seg_start, seg_end, segment in provenance_map:
        if seg_end <= chunk_start:
            continue
        if seg_start >= chunk_end:
            break
        
        overlap_start = max(seg_start, chunk_start)
        overlap_end = min(seg_end, chunk_end)
        
        if overlap_start < overlap_end:
            entry = ProvenanceEntry(
                document_id=segment.document_id,
                page=segment.page,
                section=segment.section,
                coordinates=segment.coordinates,
                start_offset=overlap_start - chunk_start,
                end_offset=overlap_end - chunk_start
            )
            entries.append(entry)
    
    return entries


def chunk_merged_text(
    merged_result: MergedTextResult,
    chunk_size_target_tokens: int = DEFAULT_CHUNK_SIZE_TARGET_TOKENS,
    chunk_size_min_tokens: int = DEFAULT_CHUNK_SIZE_MIN_TOKENS,
    chunk_overlap_tokens: int = DEFAULT_CHUNK_OVERLAP_TOKENS,
    tokenizer_model: str = DEFAULT_TOKENIZER_MODEL
) -> ChunkedTextResult:
    """
    Chunk merged patient text using RecursiveCharacterTextSplitter with token-aware sizing.
    
    Args:
        merged_result: MergedTextResult from patient text merge.
        chunk_size_target_tokens: Target maximum tokens per chunk (default 1000).
        chunk_size_min_tokens: Minimum tokens per chunk (default 500).
        chunk_overlap_tokens: Token overlap between adjacent chunks (default 100).
        tokenizer_model: Tiktoken model for token counting (default cl100k_base).
    
    Returns:
        ChunkedTextResult with chunks preserving provenance metadata.
    """
    token_counter = TokenCounter(tokenizer_model)
    
    merged_text, provenance_map = _build_provenance_mapping(merged_result)
    
    if not merged_text.strip():
        return ChunkedTextResult(
            patient_id=merged_result.patient_id,
            chunks=[],
            chunking_config={
                "chunk_size_target_tokens": chunk_size_target_tokens,
                "chunk_size_min_tokens": chunk_size_min_tokens,
                "chunk_overlap_tokens": chunk_overlap_tokens
            },
            source_documents=merged_result.source_documents
        )
    
    chunk_size_chars = chunk_size_target_tokens * CHARS_PER_TOKEN_ESTIMATE
    chunk_overlap_chars = chunk_overlap_tokens * CHARS_PER_TOKEN_ESTIMATE
    
    splitter = RecursiveCharacterTextSplitter(
        chunk_size=chunk_size_chars,
        chunk_overlap=chunk_overlap_chars,
        length_function=token_counter.length_function,
        separators=["\n\n", "\n", ". ", " ", ""],
        keep_separator=True
    )
    
    split_texts = splitter.split_text(merged_text)
    
    chunks = []
    current_offset = 0
    
    for idx, chunk_text in enumerate(split_texts):
        chunk_start = merged_text.find(chunk_text, current_offset)
        if chunk_start == -1:
            chunk_start = current_offset
        chunk_end = chunk_start + len(chunk_text)
        
        provenance_entries = _find_provenance_for_chunk(
            chunk_start, chunk_end, provenance_map
        )
        
        if not provenance_entries:
            provenance_entries = [ProvenanceEntry(
                document_id=merged_result.source_documents[0] if merged_result.source_documents else "unknown"
            )]
        
        token_count = token_counter.count_tokens(chunk_text)
        chunk_hash = _compute_chunk_hash(chunk_text)
        
        chunk = Chunk(
            chunk_index=idx,
            text=chunk_text,
            provenance=provenance_entries,
            token_count=token_count,
            chunk_hash=chunk_hash
        )
        chunks.append(chunk)
        
        current_offset = chunk_start + len(chunk_text) - (chunk_overlap_chars // 2)
        if current_offset < chunk_start:
            current_offset = chunk_start
    
    return ChunkedTextResult(
        patient_id=merged_result.patient_id,
        chunks=chunks,
        chunking_config={
            "chunk_size_target_tokens": chunk_size_target_tokens,
            "chunk_size_min_tokens": chunk_size_min_tokens,
            "chunk_overlap_tokens": chunk_overlap_tokens
        },
        source_documents=merged_result.source_documents
    )
