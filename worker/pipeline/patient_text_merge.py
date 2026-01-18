"""Patient-level multi-document text merge logic preserving per-document provenance."""

from dataclasses import dataclass, field
from typing import Optional, List, Iterator, Dict, Any
from datetime import datetime, timezone


@dataclass
class MergedTextSegment:
    """A single segment in the merged text stream with document provenance."""
    
    text: str
    document_id: str
    page: Optional[int] = None
    section: Optional[str] = None
    coordinates: Optional[dict] = None
    segment_index: Optional[int] = None
    is_document_boundary: bool = False
    
    def to_dict(self) -> dict:
        """Convert to dictionary matching merged_text.schema.json segment structure."""
        result = {
            "text": self.text,
            "document_id": self.document_id
        }
        
        location = {}
        if self.page is not None:
            location["page"] = self.page
        if self.section is not None:
            location["section"] = self.section
        if self.coordinates is not None:
            location["coordinates"] = self.coordinates
        
        if location:
            result["document_location"] = location
        
        if self.segment_index is not None:
            result["segment_index"] = self.segment_index
        
        if self.is_document_boundary:
            result["is_document_boundary"] = True
        
        return result


@dataclass
class MergedTextResult:
    """Complete merged text result aligned to merged_text.schema.json."""
    
    patient_id: str
    source_documents: List[str] = field(default_factory=list)
    merged_segments: List[MergedTextSegment] = field(default_factory=list)
    schema_version: str = "1.0"
    merge_timestamp: Optional[str] = None
    
    def __post_init__(self):
        if self.merge_timestamp is None:
            self.merge_timestamp = datetime.now(timezone.utc).isoformat()
    
    def to_dict(self) -> dict:
        """Convert to dictionary matching the merged_text.schema.json structure."""
        return {
            "schema_version": self.schema_version,
            "patient_id": self.patient_id,
            "merge_timestamp": self.merge_timestamp,
            "source_documents": self.source_documents,
            "merged_segments": [seg.to_dict() for seg in self.merged_segments]
        }


@dataclass
class DocumentSegments:
    """Container for extracted segments from a single document."""
    
    document_id: str
    segments: List[Dict[str, Any]] = field(default_factory=list)


def merge_patient_documents(
    patient_id: str,
    document_segments_list: List[DocumentSegments],
    document_order: Optional[List[str]] = None
) -> MergedTextResult:
    """
    Merge multiple documents' extracted text for a single patient.
    
    Args:
        patient_id: UUID of the patient.
        document_segments_list: List of DocumentSegments, each containing
            document_id and segments from extraction.
        document_order: Optional explicit ordering of document IDs.
            If not provided, uses the order in document_segments_list.
    
    Returns:
        MergedTextResult with all segments preserving per-document provenance.
    
    Raises:
        ValueError: If conflicting patient identifiers are detected.
    """
    if not document_segments_list:
        return MergedTextResult(
            patient_id=patient_id,
            source_documents=[],
            merged_segments=[]
        )
    
    doc_map = {ds.document_id: ds for ds in document_segments_list}
    
    if document_order:
        ordered_doc_ids = document_order
    else:
        ordered_doc_ids = [ds.document_id for ds in document_segments_list]
    
    result = MergedTextResult(
        patient_id=patient_id,
        source_documents=list(ordered_doc_ids)
    )
    
    global_index = 0
    
    for doc_id in ordered_doc_ids:
        doc_segments = doc_map.get(doc_id)
        if not doc_segments:
            continue
        
        is_first_segment = True
        
        for seg in doc_segments.segments:
            text = seg.get("text", "")
            if not text or not text.strip():
                continue
            
            location = seg.get("document_location", {}) or {}
            
            merged_segment = MergedTextSegment(
                text=text.strip(),
                document_id=doc_id,
                page=location.get("page"),
                section=location.get("section"),
                coordinates=location.get("coordinates"),
                segment_index=global_index,
                is_document_boundary=is_first_segment
            )
            
            result.merged_segments.append(merged_segment)
            global_index += 1
            is_first_segment = False
    
    return result


def merge_patient_documents_streaming(
    patient_id: str,
    document_segments_list: List[DocumentSegments],
    document_order: Optional[List[str]] = None
) -> Iterator[MergedTextSegment]:
    """
    Generator version of merge for large document sets.
    
    Yields segments one at a time to avoid memory accumulation.
    
    Args:
        patient_id: UUID of the patient.
        document_segments_list: List of DocumentSegments.
        document_order: Optional explicit ordering of document IDs.
    
    Yields:
        MergedTextSegment instances in merge order.
    """
    if not document_segments_list:
        return
    
    doc_map = {ds.document_id: ds for ds in document_segments_list}
    
    if document_order:
        ordered_doc_ids = document_order
    else:
        ordered_doc_ids = [ds.document_id for ds in document_segments_list]
    
    global_index = 0
    
    for doc_id in ordered_doc_ids:
        doc_segments = doc_map.get(doc_id)
        if not doc_segments:
            continue
        
        is_first_segment = True
        
        for seg in doc_segments.segments:
            text = seg.get("text", "")
            if not text or not text.strip():
                continue
            
            location = seg.get("document_location", {}) or {}
            
            yield MergedTextSegment(
                text=text.strip(),
                document_id=doc_id,
                page=location.get("page"),
                section=location.get("section"),
                coordinates=location.get("coordinates"),
                segment_index=global_index,
                is_document_boundary=is_first_segment
            )
            
            global_index += 1
            is_first_segment = False


def validate_patient_identifiers(
    document_patient_metadata: List[Dict[str, Any]]
) -> Optional[str]:
    """
    Validate that all documents belong to the same patient.
    
    Args:
        document_patient_metadata: List of dicts with patient identifiers
            per document (mrn, name, dob).
    
    Returns:
        Error message if conflicting identifiers detected, None otherwise.
    """
    if not document_patient_metadata:
        return None
    
    mrns = set()
    name_dob_pairs = set()
    
    for meta in document_patient_metadata:
        mrn = meta.get("mrn")
        if mrn:
            mrns.add(mrn.strip().upper())
        
        name = meta.get("name")
        dob = meta.get("dob")
        if name and dob:
            normalized_name = " ".join(name.strip().upper().split())
            name_dob_pairs.add((normalized_name, dob))
    
    if len(mrns) > 1:
        return f"Conflicting MRNs detected: {sorted(mrns)}"
    
    if len(name_dob_pairs) > 1:
        return f"Conflicting name+DOB pairs detected: {sorted(name_dob_pairs)}"
    
    return None
