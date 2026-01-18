"""Data models for extracted text segments aligned to the extracted-text contract."""

from dataclasses import dataclass, field, asdict
from typing import Optional, List
from datetime import datetime, timezone


@dataclass
class DocumentLocation:
    """Positional metadata for a text segment within the source document."""
    
    page: Optional[int] = None
    section: Optional[str] = None
    coordinates: Optional[dict] = None
    
    def to_dict(self) -> dict:
        """Convert to dictionary, excluding None values."""
        result = {}
        if self.page is not None:
            result["page"] = self.page
        if self.section is not None:
            result["section"] = self.section
        if self.coordinates is not None:
            result["coordinates"] = self.coordinates
        return result if result else None


@dataclass
class ExtractedTextSegment:
    """A single extracted text segment with positional metadata."""
    
    text: str
    document_location: Optional[DocumentLocation] = None
    segment_index: Optional[int] = None
    
    def to_dict(self) -> dict:
        """Convert to dictionary for JSON serialization."""
        result = {"text": self.text}
        if self.document_location:
            location_dict = self.document_location.to_dict()
            if location_dict:
                result["document_location"] = location_dict
        if self.segment_index is not None:
            result["segment_index"] = self.segment_index
        return result


@dataclass
class ExtractedTextResult:
    """Complete extraction result aligned to extracted_text.schema.json."""
    
    document_id: str
    segments: List[ExtractedTextSegment] = field(default_factory=list)
    schema_version: str = "1.0"
    extraction_timestamp: Optional[str] = None
    
    def __post_init__(self):
        if self.extraction_timestamp is None:
            self.extraction_timestamp = datetime.now(timezone.utc).isoformat()
    
    def to_dict(self) -> dict:
        """Convert to dictionary matching the extracted_text.schema.json structure."""
        return {
            "schema_version": self.schema_version,
            "document_id": self.document_id,
            "extraction_timestamp": self.extraction_timestamp,
            "segments": [seg.to_dict() for seg in self.segments]
        }
    
    def add_segment(
        self,
        text: str,
        page: Optional[int] = None,
        section: Optional[str] = None,
        coordinates: Optional[dict] = None
    ) -> None:
        """Add a new segment with optional positional metadata."""
        location = None
        if page is not None or section is not None or coordinates is not None:
            location = DocumentLocation(page=page, section=section, coordinates=coordinates)
        
        segment = ExtractedTextSegment(
            text=text,
            document_location=location,
            segment_index=len(self.segments)
        )
        self.segments.append(segment)
