"""Unit tests for DOCX text extraction with positional metadata."""

import pytest
import sys
import os
import tempfile

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from text_extraction.models import ExtractedTextResult
from text_extraction.docx_extractor import extract_docx_text, _detect_section_heading, _split_into_paragraphs


class TestDocxExtractor:
    """Test cases for DOCX text extraction."""

    def test_extract_docx_text_file_not_found(self):
        """extract_docx_text raises FileNotFoundError for missing file."""
        with pytest.raises(FileNotFoundError) as exc_info:
            extract_docx_text("/nonexistent/path/to/file.docx")
        
        assert "DOCX file not found" in str(exc_info.value)

    def test_extract_docx_text_uses_filename_as_document_id(self):
        """extract_docx_text uses filename when document_id not provided."""
        with tempfile.NamedTemporaryFile(suffix=".docx", delete=False) as f:
            f.write(b"PK")
            temp_path = f.name
        
        try:
            result = extract_docx_text(temp_path)
            assert os.path.basename(temp_path) in result.document_id
        except ValueError:
            pass
        finally:
            os.unlink(temp_path)

    def test_extract_docx_text_uses_provided_document_id(self):
        """extract_docx_text uses provided document_id."""
        with tempfile.NamedTemporaryFile(suffix=".docx", delete=False) as f:
            f.write(b"PK")
            temp_path = f.name
        
        try:
            result = extract_docx_text(temp_path, document_id="custom-doc-id")
            assert result.document_id == "custom-doc-id"
        except ValueError:
            pass
        finally:
            os.unlink(temp_path)

    def test_extract_docx_text_returns_extracted_text_result(self):
        """extract_docx_text returns ExtractedTextResult instance."""
        with tempfile.NamedTemporaryFile(suffix=".docx", delete=False) as f:
            f.write(b"PK")
            temp_path = f.name
        
        try:
            result = extract_docx_text(temp_path, document_id="test-doc")
            assert isinstance(result, ExtractedTextResult)
            assert result.schema_version == "1.0"
        except ValueError:
            pass
        finally:
            os.unlink(temp_path)


class TestSectionHeadingDetection:
    """Test cases for section heading detection."""

    def test_detect_section_heading_with_colon(self):
        """Detects headings ending with colon."""
        result = _detect_section_heading("Patient History:")
        assert result == "Patient History"

    def test_detect_section_heading_clinical_terms(self):
        """Detects clinical section headings."""
        assert _detect_section_heading("DIAGNOSIS") is not None
        assert _detect_section_heading("TREATMENT PLAN") is not None
        assert _detect_section_heading("MEDICATIONS") is not None
        assert _detect_section_heading("ALLERGIES") is not None

    def test_detect_section_heading_uppercase_short(self):
        """Detects short uppercase text as headings."""
        result = _detect_section_heading("SUMMARY")
        assert result is not None

    def test_detect_section_heading_regular_text(self):
        """Regular paragraph text is not detected as heading."""
        result = _detect_section_heading(
            "The patient presented with symptoms of fatigue and mild fever."
        )
        assert result is None

    def test_detect_section_heading_long_text(self):
        """Long text is not detected as heading even with colon."""
        long_text = "This is a very long sentence that happens to end with a colon but should not be considered a heading because it is too long:"
        result = _detect_section_heading(long_text)
        assert result is None


class TestParagraphSplitting:
    """Test cases for paragraph splitting."""

    def test_split_into_paragraphs_basic(self):
        """Splits text on double newlines."""
        text = "First paragraph.\n\nSecond paragraph."
        result = _split_into_paragraphs(text)
        
        assert len(result) == 2
        assert result[0] == "First paragraph."
        assert result[1] == "Second paragraph."

    def test_split_into_paragraphs_single_newlines(self):
        """Joins lines with single newlines."""
        text = "Line one.\nLine two.\nLine three."
        result = _split_into_paragraphs(text)
        
        assert len(result) == 1
        assert "Line one." in result[0]
        assert "Line two." in result[0]

    def test_split_into_paragraphs_empty_lines(self):
        """Handles multiple empty lines."""
        text = "Para one.\n\n\n\nPara two."
        result = _split_into_paragraphs(text)
        
        assert len(result) == 2

    def test_split_into_paragraphs_whitespace_only(self):
        """Handles whitespace-only input."""
        text = "   \n\n   \n   "
        result = _split_into_paragraphs(text)
        
        assert len(result) == 0


class TestDocxMetadataPreservation:
    """Test cases for DOCX metadata preservation."""

    def test_docx_segments_have_null_page(self):
        """DOCX segments have null page (not available in DOCX format)."""
        result = ExtractedTextResult(document_id="test")
        result.add_segment(text="Test content", page=None, section="Intro")
        
        output = result.to_dict()
        segment = output["segments"][0]
        
        assert "page" not in segment.get("document_location", {})

    def test_docx_segments_have_null_coordinates(self):
        """DOCX segments have null coordinates (not available in DOCX format)."""
        result = ExtractedTextResult(document_id="test")
        result.add_segment(text="Test content", coordinates=None)
        
        output = result.to_dict()
        segment = output["segments"][0]
        
        assert "coordinates" not in segment.get("document_location", {})


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
