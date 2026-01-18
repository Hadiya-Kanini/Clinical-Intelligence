"""
Unit tests for entity extraction prompt builder.

Tests prompt content, category coverage, and JSON output constraints.
"""

import pytest

from worker.entity_extraction.models import ChunkWithProvenance, ENTITY_CATEGORIES
from worker.entity_extraction.prompt_builder import (
    build_entity_extraction_prompt,
    get_system_instruction,
    get_entity_categories,
    validate_prompt_content,
    _format_chunks,
)


class TestBuildEntityExtractionPrompt:
    """Tests for build_entity_extraction_prompt function."""

    def test_build_prompt_requires_document_id(self):
        """Test that document_id is required."""
        chunks = [ChunkWithProvenance(text="test", document_id="doc-1")]
        
        with pytest.raises(ValueError, match="document_id is required"):
            build_entity_extraction_prompt("", chunks)
        
        with pytest.raises(ValueError, match="document_id is required"):
            build_entity_extraction_prompt(None, chunks)

    def test_build_prompt_requires_chunks(self):
        """Test that at least one chunk is required."""
        with pytest.raises(ValueError, match="At least one chunk is required"):
            build_entity_extraction_prompt("doc-123", [])
        
        with pytest.raises(ValueError, match="At least one chunk is required"):
            build_entity_extraction_prompt("doc-123", None)

    def test_build_prompt_includes_document_id(self):
        """Test that prompt includes the document ID."""
        chunks = [ChunkWithProvenance(text="Patient info", document_id="doc-123")]
        
        prompt = build_entity_extraction_prompt("doc-123", chunks)
        
        assert "doc-123" in prompt

    def test_build_prompt_includes_all_categories(self):
        """Test that prompt includes all 10 entity categories."""
        chunks = [ChunkWithProvenance(text="Patient info", document_id="doc-123")]
        
        prompt = build_entity_extraction_prompt("doc-123", chunks)
        
        for category in ENTITY_CATEGORIES:
            assert category in prompt, f"Missing category: {category}"

    def test_build_prompt_includes_json_constraints(self):
        """Test that prompt includes JSON output constraints."""
        chunks = [ChunkWithProvenance(text="Patient info", document_id="doc-123")]
        
        prompt = build_entity_extraction_prompt("doc-123", chunks)
        
        assert "JSON" in prompt
        assert "schema_version" in prompt
        assert "extracted_entities" in prompt

    def test_build_prompt_includes_conflict_instructions(self):
        """Test that prompt includes conflict detection instructions."""
        chunks = [ChunkWithProvenance(text="Patient info", document_id="doc-123")]
        
        prompt = build_entity_extraction_prompt("doc-123", chunks)
        
        assert "conflicts" in prompt.lower()
        assert "conflicting" in prompt.lower()

    def test_build_prompt_includes_grounding_requirements(self):
        """Test that prompt includes grounding requirements."""
        chunks = [ChunkWithProvenance(text="Patient info", document_id="doc-123")]
        
        prompt = build_entity_extraction_prompt("doc-123", chunks)
        
        assert "source_text" in prompt
        assert "document_location" in prompt

    def test_build_prompt_includes_chunk_text(self):
        """Test that prompt includes the chunk text."""
        chunks = [
            ChunkWithProvenance(text="Patient name is John Doe", document_id="doc-123"),
            ChunkWithProvenance(text="DOB: 1990-01-15", document_id="doc-123"),
        ]
        
        prompt = build_entity_extraction_prompt("doc-123", chunks)
        
        assert "Patient name is John Doe" in prompt
        assert "DOB: 1990-01-15" in prompt


class TestFormatChunks:
    """Tests for _format_chunks function."""

    def test_format_single_chunk(self):
        """Test formatting a single chunk."""
        chunks = [ChunkWithProvenance(text="Sample text", document_id="doc-123")]
        
        result = _format_chunks(chunks)
        
        assert "[CHUNK 1]" in result
        assert "Sample text" in result

    def test_format_multiple_chunks(self):
        """Test formatting multiple chunks."""
        chunks = [
            ChunkWithProvenance(text="First chunk", document_id="doc-123"),
            ChunkWithProvenance(text="Second chunk", document_id="doc-123"),
        ]
        
        result = _format_chunks(chunks)
        
        assert "[CHUNK 1]" in result
        assert "[CHUNK 2]" in result
        assert "First chunk" in result
        assert "Second chunk" in result

    def test_format_chunk_with_metadata(self):
        """Test formatting chunk with metadata."""
        chunks = [
            ChunkWithProvenance(
                text="Sample text",
                document_id="doc-123",
                page=5,
                section="History",
                rank=1
            )
        ]
        
        result = _format_chunks(chunks)
        
        assert "Page: 5" in result
        assert "Section: History" in result
        assert "Relevance Rank: 1" in result


class TestGetSystemInstruction:
    """Tests for get_system_instruction function."""

    def test_system_instruction_not_empty(self):
        """Test that system instruction is not empty."""
        instruction = get_system_instruction()
        
        assert instruction
        assert len(instruction) > 0

    def test_system_instruction_includes_json_requirement(self):
        """Test that system instruction requires JSON output."""
        instruction = get_system_instruction()
        
        assert "JSON" in instruction

    def test_system_instruction_includes_grounding_requirement(self):
        """Test that system instruction requires grounding."""
        instruction = get_system_instruction()
        
        assert "source" in instruction.lower() or "ground" in instruction.lower()


class TestGetEntityCategories:
    """Tests for get_entity_categories function."""

    def test_returns_10_categories(self):
        """Test that exactly 10 categories are returned."""
        categories = get_entity_categories()
        
        assert len(categories) == 10

    def test_returns_copy(self):
        """Test that a copy is returned, not the original."""
        categories1 = get_entity_categories()
        categories2 = get_entity_categories()
        
        categories1.append("new_category")
        
        assert len(categories2) == 10

    def test_includes_expected_categories(self):
        """Test that expected categories are included."""
        categories = get_entity_categories()
        
        expected = [
            "patient_demographics",
            "allergies",
            "medications",
            "diagnoses",
            "procedures",
            "lab_results",
            "vital_signs",
            "social_history",
            "clinical_notes",
            "document_metadata",
        ]
        
        for cat in expected:
            assert cat in categories


class TestValidatePromptContent:
    """Tests for validate_prompt_content function."""

    def test_valid_prompt_passes(self):
        """Test that a valid prompt passes validation."""
        chunks = [ChunkWithProvenance(text="Patient info", document_id="doc-123")]
        prompt = build_entity_extraction_prompt("doc-123", chunks)
        
        assert validate_prompt_content(prompt) is True

    def test_incomplete_prompt_fails(self):
        """Test that an incomplete prompt fails validation."""
        incomplete_prompt = "Just some text without required elements"
        
        assert validate_prompt_content(incomplete_prompt) is False
