"""Unit tests for patient-level multi-document text merge."""

import pytest
import sys
import os

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from pipeline.patient_text_merge import (
    merge_patient_documents,
    merge_patient_documents_streaming,
    validate_patient_identifiers,
    MergedTextResult,
    MergedTextSegment,
    DocumentSegments
)


class TestMergedTextSegment:
    """Test cases for MergedTextSegment data model."""

    def test_to_dict_minimal(self):
        """Minimal segment produces required fields only."""
        segment = MergedTextSegment(text="Content", document_id="doc-1")
        result = segment.to_dict()
        
        assert result == {"text": "Content", "document_id": "doc-1"}

    def test_to_dict_with_location(self):
        """Segment with location includes document_location."""
        segment = MergedTextSegment(
            text="Content",
            document_id="doc-1",
            page=5,
            section="Methods"
        )
        result = segment.to_dict()
        
        assert result["document_location"]["page"] == 5
        assert result["document_location"]["section"] == "Methods"

    def test_to_dict_with_boundary_flag(self):
        """Boundary segment includes is_document_boundary."""
        segment = MergedTextSegment(
            text="First",
            document_id="doc-1",
            is_document_boundary=True
        )
        result = segment.to_dict()
        
        assert result["is_document_boundary"] is True

    def test_to_dict_without_boundary_flag(self):
        """Non-boundary segment omits is_document_boundary."""
        segment = MergedTextSegment(
            text="Middle",
            document_id="doc-1",
            is_document_boundary=False
        )
        result = segment.to_dict()
        
        assert "is_document_boundary" not in result


class TestMergedTextResult:
    """Test cases for MergedTextResult data model."""

    def test_to_dict_structure(self):
        """Result produces schema-compliant structure."""
        result = MergedTextResult(
            patient_id="patient-123",
            source_documents=["doc-1", "doc-2"]
        )
        output = result.to_dict()
        
        assert output["schema_version"] == "1.0"
        assert output["patient_id"] == "patient-123"
        assert output["source_documents"] == ["doc-1", "doc-2"]
        assert "merge_timestamp" in output
        assert output["merged_segments"] == []

    def test_timestamp_auto_generated(self):
        """Merge timestamp is auto-generated if not provided."""
        result = MergedTextResult(patient_id="p-1")
        assert result.merge_timestamp is not None


class TestMergePatientDocuments:
    """Test cases for merge_patient_documents function."""

    def test_empty_input_returns_empty_result(self):
        """Empty document list returns empty merged result."""
        result = merge_patient_documents("patient-1", [])
        
        assert result.patient_id == "patient-1"
        assert result.source_documents == []
        assert result.merged_segments == []

    def test_single_document_merge(self):
        """Single document merge preserves all segments."""
        doc_segments = DocumentSegments(
            document_id="doc-1",
            segments=[
                {"text": "First paragraph"},
                {"text": "Second paragraph"}
            ]
        )
        
        result = merge_patient_documents("patient-1", [doc_segments])
        
        assert result.source_documents == ["doc-1"]
        assert len(result.merged_segments) == 2
        assert result.merged_segments[0].text == "First paragraph"
        assert result.merged_segments[0].document_id == "doc-1"

    def test_multi_document_merge_ordering(self):
        """Multiple documents merge in provided order."""
        doc1 = DocumentSegments(
            document_id="doc-1",
            segments=[{"text": "Doc1 content"}]
        )
        doc2 = DocumentSegments(
            document_id="doc-2",
            segments=[{"text": "Doc2 content"}]
        )
        
        result = merge_patient_documents("patient-1", [doc1, doc2])
        
        assert result.source_documents == ["doc-1", "doc-2"]
        assert result.merged_segments[0].document_id == "doc-1"
        assert result.merged_segments[1].document_id == "doc-2"

    def test_explicit_document_order(self):
        """Explicit document_order overrides input order."""
        doc1 = DocumentSegments(document_id="doc-1", segments=[{"text": "A"}])
        doc2 = DocumentSegments(document_id="doc-2", segments=[{"text": "B"}])
        
        result = merge_patient_documents(
            "patient-1",
            [doc1, doc2],
            document_order=["doc-2", "doc-1"]
        )
        
        assert result.source_documents == ["doc-2", "doc-1"]
        assert result.merged_segments[0].document_id == "doc-2"
        assert result.merged_segments[1].document_id == "doc-1"

    def test_document_boundary_markers(self):
        """First segment of each document has is_document_boundary=True."""
        doc1 = DocumentSegments(
            document_id="doc-1",
            segments=[{"text": "A"}, {"text": "B"}]
        )
        doc2 = DocumentSegments(
            document_id="doc-2",
            segments=[{"text": "C"}, {"text": "D"}]
        )
        
        result = merge_patient_documents("patient-1", [doc1, doc2])
        
        assert result.merged_segments[0].is_document_boundary is True
        assert result.merged_segments[1].is_document_boundary is False
        assert result.merged_segments[2].is_document_boundary is True
        assert result.merged_segments[3].is_document_boundary is False

    def test_segment_index_sequential(self):
        """Segment indices are sequential across all documents."""
        doc1 = DocumentSegments(document_id="doc-1", segments=[{"text": "A"}, {"text": "B"}])
        doc2 = DocumentSegments(document_id="doc-2", segments=[{"text": "C"}])
        
        result = merge_patient_documents("patient-1", [doc1, doc2])
        
        assert result.merged_segments[0].segment_index == 0
        assert result.merged_segments[1].segment_index == 1
        assert result.merged_segments[2].segment_index == 2

    def test_preserves_location_metadata(self):
        """Location metadata is preserved through merge."""
        doc = DocumentSegments(
            document_id="doc-1",
            segments=[{
                "text": "Content",
                "document_location": {
                    "page": 3,
                    "section": "Results",
                    "coordinates": {"x0": 10, "y0": 20, "x1": 100, "y1": 50}
                }
            }]
        )
        
        result = merge_patient_documents("patient-1", [doc])
        
        seg = result.merged_segments[0]
        assert seg.page == 3
        assert seg.section == "Results"
        assert seg.coordinates == {"x0": 10, "y0": 20, "x1": 100, "y1": 50}

    def test_skips_empty_text_segments(self):
        """Empty or whitespace-only segments are skipped."""
        doc = DocumentSegments(
            document_id="doc-1",
            segments=[
                {"text": "Valid"},
                {"text": ""},
                {"text": "   "},
                {"text": "Also valid"}
            ]
        )
        
        result = merge_patient_documents("patient-1", [doc])
        
        assert len(result.merged_segments) == 2
        assert result.merged_segments[0].text == "Valid"
        assert result.merged_segments[1].text == "Also valid"

    def test_handles_missing_document_in_order(self):
        """Missing document in order is gracefully skipped."""
        doc1 = DocumentSegments(document_id="doc-1", segments=[{"text": "A"}])
        
        result = merge_patient_documents(
            "patient-1",
            [doc1],
            document_order=["doc-missing", "doc-1"]
        )
        
        assert result.source_documents == ["doc-missing", "doc-1"]
        assert len(result.merged_segments) == 1
        assert result.merged_segments[0].document_id == "doc-1"


class TestMergePatientDocumentsStreaming:
    """Test cases for streaming merge generator."""

    def test_streaming_yields_segments(self):
        """Streaming merge yields segments one at a time."""
        doc = DocumentSegments(
            document_id="doc-1",
            segments=[{"text": "A"}, {"text": "B"}]
        )
        
        segments = list(merge_patient_documents_streaming("patient-1", [doc]))
        
        assert len(segments) == 2
        assert segments[0].text == "A"
        assert segments[1].text == "B"

    def test_streaming_empty_input(self):
        """Streaming with empty input yields nothing."""
        segments = list(merge_patient_documents_streaming("patient-1", []))
        assert segments == []

    def test_streaming_preserves_order(self):
        """Streaming preserves document order."""
        doc1 = DocumentSegments(document_id="doc-1", segments=[{"text": "A"}])
        doc2 = DocumentSegments(document_id="doc-2", segments=[{"text": "B"}])
        
        segments = list(merge_patient_documents_streaming(
            "patient-1",
            [doc1, doc2],
            document_order=["doc-2", "doc-1"]
        ))
        
        assert segments[0].document_id == "doc-2"
        assert segments[1].document_id == "doc-1"


class TestValidatePatientIdentifiers:
    """Test cases for patient identifier validation."""

    def test_empty_input_returns_none(self):
        """Empty input returns no error."""
        result = validate_patient_identifiers([])
        assert result is None

    def test_single_mrn_returns_none(self):
        """Single MRN returns no error."""
        result = validate_patient_identifiers([{"mrn": "MRN-001"}])
        assert result is None

    def test_matching_mrns_returns_none(self):
        """Matching MRNs return no error."""
        result = validate_patient_identifiers([
            {"mrn": "MRN-001"},
            {"mrn": "mrn-001"}
        ])
        assert result is None

    def test_conflicting_mrns_returns_error(self):
        """Conflicting MRNs return error message."""
        result = validate_patient_identifiers([
            {"mrn": "MRN-001"},
            {"mrn": "MRN-002"}
        ])
        assert result is not None
        assert "Conflicting MRNs" in result

    def test_matching_name_dob_returns_none(self):
        """Matching name+DOB returns no error."""
        result = validate_patient_identifiers([
            {"name": "John Doe", "dob": "1980-01-15"},
            {"name": "JOHN DOE", "dob": "1980-01-15"}
        ])
        assert result is None

    def test_conflicting_name_dob_returns_error(self):
        """Conflicting name+DOB returns error message."""
        result = validate_patient_identifiers([
            {"name": "John Doe", "dob": "1980-01-15"},
            {"name": "Jane Doe", "dob": "1980-01-15"}
        ])
        assert result is not None
        assert "Conflicting name+DOB" in result

    def test_partial_identifiers_ignored(self):
        """Partial identifiers (name without DOB) are ignored."""
        result = validate_patient_identifiers([
            {"name": "John Doe"},
            {"name": "Jane Doe"}
        ])
        assert result is None


class TestMergePerformance:
    """Performance-focused tests for large input handling."""

    def test_large_segment_count_linear_time(self):
        """Merge scales linearly with segment count."""
        segment_count = 1000
        doc = DocumentSegments(
            document_id="doc-1",
            segments=[{"text": f"Segment {i}"} for i in range(segment_count)]
        )
        
        result = merge_patient_documents("patient-1", [doc])
        
        assert len(result.merged_segments) == segment_count

    def test_many_documents_linear_time(self):
        """Merge scales linearly with document count."""
        doc_count = 100
        docs = [
            DocumentSegments(
                document_id=f"doc-{i}",
                segments=[{"text": f"Content from doc {i}"}]
            )
            for i in range(doc_count)
        ]
        
        result = merge_patient_documents("patient-1", docs)
        
        assert len(result.source_documents) == doc_count
        assert len(result.merged_segments) == doc_count


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
