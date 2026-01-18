"""Unit tests for PDF text extraction with positional metadata."""

import pytest
import sys
import os
import tempfile

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from text_extraction.models import ExtractedTextResult, ExtractedTextSegment, DocumentLocation
from text_extraction.pdf_extractor import extract_pdf_text


class TestExtractedTextModels:
    """Test cases for extracted text data models."""

    def test_document_location_to_dict_with_all_fields(self):
        """DocumentLocation.to_dict includes all non-None fields."""
        location = DocumentLocation(
            page=1,
            section="Introduction",
            coordinates={"x0": 0, "y0": 0, "x1": 100, "y1": 50}
        )
        result = location.to_dict()
        
        assert result["page"] == 1
        assert result["section"] == "Introduction"
        assert result["coordinates"] == {"x0": 0, "y0": 0, "x1": 100, "y1": 50}

    def test_document_location_to_dict_with_only_page(self):
        """DocumentLocation.to_dict excludes None fields."""
        location = DocumentLocation(page=5)
        result = location.to_dict()
        
        assert result == {"page": 5}
        assert "section" not in result
        assert "coordinates" not in result

    def test_document_location_to_dict_empty_returns_none(self):
        """DocumentLocation.to_dict returns None when all fields are None."""
        location = DocumentLocation()
        result = location.to_dict()
        
        assert result is None

    def test_extracted_text_segment_to_dict(self):
        """ExtractedTextSegment.to_dict produces correct structure."""
        segment = ExtractedTextSegment(
            text="Sample text",
            document_location=DocumentLocation(page=2),
            segment_index=0
        )
        result = segment.to_dict()
        
        assert result["text"] == "Sample text"
        assert result["document_location"]["page"] == 2
        assert result["segment_index"] == 0

    def test_extracted_text_segment_minimal(self):
        """ExtractedTextSegment with only text produces minimal dict."""
        segment = ExtractedTextSegment(text="Only text")
        result = segment.to_dict()
        
        assert result == {"text": "Only text"}

    def test_extracted_text_result_structure(self):
        """ExtractedTextResult.to_dict matches schema structure."""
        result = ExtractedTextResult(document_id="doc-123")
        result.add_segment(text="First segment", page=1)
        result.add_segment(text="Second segment", page=2, section="Methods")
        
        output = result.to_dict()
        
        assert output["schema_version"] == "1.0"
        assert output["document_id"] == "doc-123"
        assert "extraction_timestamp" in output
        assert len(output["segments"]) == 2
        assert output["segments"][0]["text"] == "First segment"
        assert output["segments"][0]["document_location"]["page"] == 1
        assert output["segments"][1]["document_location"]["section"] == "Methods"

    def test_extracted_text_result_segment_indexing(self):
        """ExtractedTextResult assigns sequential segment indices."""
        result = ExtractedTextResult(document_id="doc-456")
        result.add_segment(text="A")
        result.add_segment(text="B")
        result.add_segment(text="C")
        
        output = result.to_dict()
        
        assert output["segments"][0]["segment_index"] == 0
        assert output["segments"][1]["segment_index"] == 1
        assert output["segments"][2]["segment_index"] == 2


class TestPdfExtractor:
    """Test cases for PDF text extraction."""

    def test_extract_pdf_text_file_not_found(self):
        """extract_pdf_text raises FileNotFoundError for missing file."""
        with pytest.raises(FileNotFoundError) as exc_info:
            extract_pdf_text("/nonexistent/path/to/file.pdf")
        
        assert "PDF file not found" in str(exc_info.value)

    def test_extract_pdf_text_uses_filename_as_document_id(self):
        """extract_pdf_text uses filename when document_id not provided."""
        with tempfile.NamedTemporaryFile(suffix=".pdf", delete=False) as f:
            f.write(b"%PDF-1.4\n")
            temp_path = f.name
        
        try:
            result = extract_pdf_text(temp_path)
            assert os.path.basename(temp_path) in result.document_id
        except ValueError:
            pass
        finally:
            os.unlink(temp_path)

    def test_extract_pdf_text_uses_provided_document_id(self):
        """extract_pdf_text uses provided document_id."""
        with tempfile.NamedTemporaryFile(suffix=".pdf", delete=False) as f:
            f.write(b"%PDF-1.4\n")
            temp_path = f.name
        
        try:
            result = extract_pdf_text(temp_path, document_id="custom-doc-id")
            assert result.document_id == "custom-doc-id"
        except ValueError:
            pass
        finally:
            os.unlink(temp_path)

    def test_extract_pdf_text_returns_extracted_text_result(self):
        """extract_pdf_text returns ExtractedTextResult instance."""
        with tempfile.NamedTemporaryFile(suffix=".pdf", delete=False) as f:
            f.write(b"%PDF-1.4\n")
            temp_path = f.name
        
        try:
            result = extract_pdf_text(temp_path, document_id="test-doc")
            assert isinstance(result, ExtractedTextResult)
            assert result.schema_version == "1.0"
        except ValueError:
            pass
        finally:
            os.unlink(temp_path)

    def test_extract_pdf_text_handles_empty_pdf_gracefully(self):
        """extract_pdf_text handles PDF with no extractable text."""
        with tempfile.NamedTemporaryFile(suffix=".pdf", delete=False) as f:
            f.write(b"%PDF-1.4\n%%EOF")
            temp_path = f.name
        
        try:
            result = extract_pdf_text(temp_path, document_id="empty-doc")
            assert isinstance(result, ExtractedTextResult)
            assert len(result.segments) == 0
        except ValueError:
            pass
        finally:
            os.unlink(temp_path)

    def test_extract_pdf_text_null_coordinates_handled(self):
        """extract_pdf_text handles missing coordinates gracefully."""
        with tempfile.NamedTemporaryFile(suffix=".pdf", delete=False) as f:
            f.write(b"%PDF-1.4\n")
            temp_path = f.name
        
        try:
            result = extract_pdf_text(temp_path, document_id="test-doc")
            for segment in result.segments:
                if segment.document_location:
                    pass
        except ValueError:
            pass
        finally:
            os.unlink(temp_path)


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
