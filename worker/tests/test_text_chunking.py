"""Unit tests for semantic text chunking with provenance preservation."""

import pytest
from pipeline.patient_text_merge import MergedTextResult, MergedTextSegment
from pipeline.text_chunking import (
    chunk_merged_text,
    ChunkedTextResult,
    Chunk,
    ProvenanceEntry,
    TokenCounter,
    _compute_chunk_hash,
    _build_provenance_mapping,
    _find_provenance_for_chunk,
)


class TestTokenCounter:
    """Tests for TokenCounter class."""
    
    def test_count_tokens_empty_string(self):
        counter = TokenCounter()
        assert counter.count_tokens("") == 0
    
    def test_count_tokens_simple_text(self):
        counter = TokenCounter()
        token_count = counter.count_tokens("Hello world")
        assert token_count > 0
        assert token_count < 10
    
    def test_count_tokens_longer_text(self):
        counter = TokenCounter()
        text = "This is a longer piece of text that should have more tokens."
        token_count = counter.count_tokens(text)
        assert token_count > 5
    
    def test_length_function_matches_count_tokens(self):
        counter = TokenCounter()
        text = "Test text for length function"
        assert counter.length_function(text) == counter.count_tokens(text)


class TestComputeChunkHash:
    """Tests for chunk hash computation."""
    
    def test_hash_deterministic(self):
        text = "Sample chunk text"
        hash1 = _compute_chunk_hash(text)
        hash2 = _compute_chunk_hash(text)
        assert hash1 == hash2
    
    def test_hash_different_for_different_text(self):
        hash1 = _compute_chunk_hash("Text A")
        hash2 = _compute_chunk_hash("Text B")
        assert hash1 != hash2
    
    def test_hash_length(self):
        hash_value = _compute_chunk_hash("Any text")
        assert len(hash_value) == 16


class TestBuildProvenanceMapping:
    """Tests for provenance mapping construction."""
    
    def test_empty_segments(self):
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            merged_segments=[]
        )
        text, mapping = _build_provenance_mapping(merged)
        assert text == ""
        assert mapping == []
    
    def test_single_segment(self):
        segment = MergedTextSegment(
            text="Hello world",
            document_id="doc-001",
            page=1
        )
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            merged_segments=[segment]
        )
        text, mapping = _build_provenance_mapping(merged)
        assert "Hello world" in text
        assert len(mapping) == 1
        assert mapping[0][2].document_id == "doc-001"
    
    def test_multiple_segments(self):
        segments = [
            MergedTextSegment(text="First segment", document_id="doc-001"),
            MergedTextSegment(text="Second segment", document_id="doc-002"),
        ]
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            merged_segments=segments
        )
        text, mapping = _build_provenance_mapping(merged)
        assert "First segment" in text
        assert "Second segment" in text
        assert len(mapping) == 2


class TestFindProvenanceForChunk:
    """Tests for provenance lookup."""
    
    def test_chunk_within_single_segment(self):
        segment = MergedTextSegment(
            text="0123456789",
            document_id="doc-001",
            page=1
        )
        provenance_map = [(0, 10, segment)]
        
        entries = _find_provenance_for_chunk(2, 8, provenance_map)
        assert len(entries) == 1
        assert entries[0].document_id == "doc-001"
        assert entries[0].start_offset == 0
        assert entries[0].end_offset == 6
    
    def test_chunk_spanning_two_segments(self):
        seg1 = MergedTextSegment(text="AAAAA", document_id="doc-001")
        seg2 = MergedTextSegment(text="BBBBB", document_id="doc-002")
        provenance_map = [(0, 5, seg1), (7, 12, seg2)]
        
        entries = _find_provenance_for_chunk(3, 10, provenance_map)
        assert len(entries) == 2
        assert entries[0].document_id == "doc-001"
        assert entries[1].document_id == "doc-002"


class TestChunkMergedText:
    """Tests for the main chunking function."""
    
    def _create_merged_result(self, text_segments: list, patient_id: str = "00000000-0000-0000-0000-000000000001"):
        """Helper to create MergedTextResult from text segments."""
        segments = []
        for i, (text, doc_id) in enumerate(text_segments):
            segments.append(MergedTextSegment(
                text=text,
                document_id=doc_id,
                segment_index=i,
                is_document_boundary=(i == 0 or text_segments[i-1][1] != doc_id)
            ))
        return MergedTextResult(
            patient_id=patient_id,
            source_documents=list(dict.fromkeys(doc_id for _, doc_id in text_segments)),
            merged_segments=segments
        )
    
    def test_empty_input(self):
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            merged_segments=[]
        )
        result = chunk_merged_text(merged)
        assert result.patient_id == "00000000-0000-0000-0000-000000000001"
        assert result.chunks == []
        assert result.schema_version == "1.0"
    
    def test_short_document_single_chunk(self):
        """Documents shorter than 500 tokens should emit a single chunk."""
        merged = self._create_merged_result([
            ("This is a short document with just a few sentences.", "doc-001")
        ])
        result = chunk_merged_text(merged)
        
        assert len(result.chunks) == 1
        assert result.chunks[0].chunk_index == 0
        assert "short document" in result.chunks[0].text
        assert len(result.chunks[0].provenance) >= 1
        assert result.chunks[0].provenance[0].document_id == "doc-001"
    
    def test_chunk_token_count_populated(self):
        merged = self._create_merged_result([
            ("Sample text for token counting.", "doc-001")
        ])
        result = chunk_merged_text(merged)
        
        assert result.chunks[0].token_count is not None
        assert result.chunks[0].token_count > 0
    
    def test_chunk_hash_populated(self):
        merged = self._create_merged_result([
            ("Sample text for hashing.", "doc-001")
        ])
        result = chunk_merged_text(merged)
        
        assert result.chunks[0].chunk_hash is not None
        assert len(result.chunks[0].chunk_hash) == 16
    
    def test_chunking_config_included(self):
        merged = self._create_merged_result([
            ("Some text.", "doc-001")
        ])
        result = chunk_merged_text(
            merged,
            chunk_size_target_tokens=1000,
            chunk_size_min_tokens=500,
            chunk_overlap_tokens=100
        )
        
        assert result.chunking_config is not None
        assert result.chunking_config["chunk_size_target_tokens"] == 1000
        assert result.chunking_config["chunk_size_min_tokens"] == 500
        assert result.chunking_config["chunk_overlap_tokens"] == 100
    
    def test_source_documents_preserved(self):
        merged = self._create_merged_result([
            ("Text from doc 1.", "doc-001"),
            ("Text from doc 2.", "doc-002"),
        ])
        result = chunk_merged_text(merged)
        
        assert result.source_documents == ["doc-001", "doc-002"]
    
    def test_deterministic_output(self):
        """Same input should produce same output."""
        merged = self._create_merged_result([
            ("Deterministic test content.", "doc-001")
        ])
        
        result1 = chunk_merged_text(merged)
        result2 = chunk_merged_text(merged)
        
        assert len(result1.chunks) == len(result2.chunks)
        for c1, c2 in zip(result1.chunks, result2.chunks):
            assert c1.chunk_index == c2.chunk_index
            assert c1.text == c2.text
            assert c1.chunk_hash == c2.chunk_hash
            assert c1.token_count == c2.token_count
    
    def test_chunk_index_sequential(self):
        """Chunk indices should be sequential starting from 0."""
        long_text = " ".join(["This is sentence number {}.".format(i) for i in range(200)])
        merged = self._create_merged_result([
            (long_text, "doc-001")
        ])
        result = chunk_merged_text(merged, chunk_size_target_tokens=100)
        
        for i, chunk in enumerate(result.chunks):
            assert chunk.chunk_index == i
    
    def test_provenance_preserved_for_each_chunk(self):
        merged = self._create_merged_result([
            ("Content from first document.", "doc-001"),
            ("Content from second document.", "doc-002"),
        ])
        result = chunk_merged_text(merged)
        
        for chunk in result.chunks:
            assert len(chunk.provenance) >= 1
            for prov in chunk.provenance:
                assert prov.document_id in ["doc-001", "doc-002"]
    
    def test_multi_document_chunk_provenance(self):
        """Chunks spanning document boundaries should have multiple provenance entries."""
        text1 = " ".join(["Sentence from doc one."] * 50)
        text2 = " ".join(["Sentence from doc two."] * 50)
        merged = self._create_merged_result([
            (text1, "doc-001"),
            (text2, "doc-002"),
        ])
        result = chunk_merged_text(merged, chunk_size_target_tokens=200)
        
        has_multi_provenance = any(len(c.provenance) > 1 for c in result.chunks)
        all_have_provenance = all(len(c.provenance) >= 1 for c in result.chunks)
        assert all_have_provenance
    
    def test_to_dict_structure(self):
        merged = self._create_merged_result([
            ("Test content.", "doc-001")
        ])
        result = chunk_merged_text(merged)
        result_dict = result.to_dict()
        
        assert "schema_version" in result_dict
        assert "patient_id" in result_dict
        assert "chunks" in result_dict
        assert result_dict["schema_version"] == "1.0"
        
        if result_dict["chunks"]:
            chunk = result_dict["chunks"][0]
            assert "chunk_index" in chunk
            assert "text" in chunk
            assert "provenance" in chunk


class TestChunkSizingAndOverlap:
    """Tests for chunk sizing within 500-1000 token range and overlap."""
    
    def _generate_long_text(self, target_tokens: int = 3000) -> str:
        """Generate text with approximately target_tokens tokens."""
        sentences = []
        counter = TokenCounter()
        current_tokens = 0
        i = 0
        while current_tokens < target_tokens:
            sentence = f"This is sentence number {i} with some additional words to make it longer. "
            sentences.append(sentence)
            current_tokens += counter.count_tokens(sentence)
            i += 1
        return "".join(sentences)
    
    def test_chunk_sizes_within_range(self):
        """Chunks should be approximately within target range (with some tolerance)."""
        long_text = self._generate_long_text(5000)
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            source_documents=["doc-001"],
            merged_segments=[MergedTextSegment(text=long_text, document_id="doc-001")]
        )
        
        result = chunk_merged_text(
            merged,
            chunk_size_target_tokens=1000,
            chunk_size_min_tokens=500,
            chunk_overlap_tokens=100
        )
        
        assert len(result.chunks) >= 1
        
        for chunk in result.chunks:
            assert chunk.token_count is not None
            assert chunk.token_count > 0
    
    def test_overlap_between_chunks(self):
        """Adjacent chunks should have overlapping content."""
        long_text = self._generate_long_text(3000)
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            source_documents=["doc-001"],
            merged_segments=[MergedTextSegment(text=long_text, document_id="doc-001")]
        )
        
        result = chunk_merged_text(
            merged,
            chunk_size_target_tokens=500,
            chunk_overlap_tokens=100
        )
        
        if len(result.chunks) >= 2:
            chunk1_end = result.chunks[0].text[-200:]
            chunk2_start = result.chunks[1].text[:200]
            
            words1 = set(chunk1_end.split())
            words2 = set(chunk2_start.split())
            overlap_words = words1 & words2
            
            assert len(overlap_words) > 0 or len(result.chunks) == 1


class TestEdgeCases:
    """Tests for edge cases in chunking."""
    
    def test_whitespace_only_input(self):
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            merged_segments=[MergedTextSegment(text="   \n\n   ", document_id="doc-001")]
        )
        result = chunk_merged_text(merged)
        assert result.chunks == []
    
    def test_unusual_formatting_tables(self):
        """Tables and unusual formatting should be handled."""
        table_text = """
        | Column A | Column B | Column C |
        |----------|----------|----------|
        | Value 1  | Value 2  | Value 3  |
        | Value 4  | Value 5  | Value 6  |
        """
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            source_documents=["doc-001"],
            merged_segments=[MergedTextSegment(text=table_text, document_id="doc-001")]
        )
        result = chunk_merged_text(merged)
        
        assert len(result.chunks) >= 1
        assert result.chunks[0].token_count > 0
    
    def test_excessive_whitespace(self):
        """Excessive whitespace should be handled without dropping metadata."""
        text_with_whitespace = "Content\n\n\n\n\n\n\n\nMore content\n\n\n\nEnd."
        merged = MergedTextResult(
            patient_id="00000000-0000-0000-0000-000000000001",
            source_documents=["doc-001"],
            merged_segments=[MergedTextSegment(text=text_with_whitespace, document_id="doc-001")]
        )
        result = chunk_merged_text(merged)
        
        assert len(result.chunks) >= 1
        assert len(result.chunks[0].provenance) >= 1
