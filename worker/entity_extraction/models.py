"""
Data models for entity extraction.

Defines typed structures for retrieval chunks with provenance and extraction inputs.
"""

from dataclasses import dataclass, field
from typing import List, Optional


@dataclass
class DocumentLocation:
    """Location information within a document."""
    page: Optional[int] = None
    section: Optional[str] = None
    coordinates: Optional[str] = None

    def to_dict(self) -> dict:
        """Convert to dictionary representation."""
        result = {}
        if self.page is not None:
            result["page"] = self.page
        if self.section is not None:
            result["section"] = self.section
        if self.coordinates is not None:
            result["coordinates"] = self.coordinates
        return result if result else None


@dataclass
class ChunkWithProvenance:
    """
    A text chunk with its source provenance information.
    
    Used as input to the entity extraction prompt builder.
    """
    text: str
    document_id: str
    chunk_id: Optional[str] = None
    page: Optional[int] = None
    section: Optional[str] = None
    coordinates: Optional[str] = None
    rank: Optional[int] = None
    score: Optional[float] = None

    @property
    def document_location(self) -> Optional[DocumentLocation]:
        """Get document location if any location fields are set."""
        if self.page is not None or self.section is not None or self.coordinates is not None:
            return DocumentLocation(
                page=self.page,
                section=self.section,
                coordinates=self.coordinates
            )
        return None


@dataclass
class ExtractionInput:
    """
    Input for entity extraction containing document context and chunks.
    """
    document_id: str
    chunks: List[ChunkWithProvenance] = field(default_factory=list)
    patient_id: Optional[str] = None

    def get_combined_text(self) -> str:
        """Get all chunk texts combined."""
        return "\n\n".join(chunk.text for chunk in self.chunks)

    def get_chunk_count(self) -> int:
        """Get the number of chunks."""
        return len(self.chunks)


class CoreEntityCategories:
    """
    Canonical taxonomy for the 10 core clinical entity categories.
    
    Aligned to FR-039 through FR-048 requirements.
    These are the authoritative entity_group_name values used throughout
    the extraction, validation, and persistence pipeline.
    """
    
    PATIENT_DEMOGRAPHICS = "patient_demographics"
    ALLERGIES = "allergies"
    MEDICATIONS = "medications"
    DIAGNOSES = "diagnoses"
    PROCEDURES = "procedures"
    LAB_RESULTS = "lab_results"
    VITAL_SIGNS = "vital_signs"
    SOCIAL_HISTORY = "social_history"
    CLINICAL_NOTES = "clinical_notes"
    DOCUMENT_METADATA = "document_metadata"
    
    @classmethod
    def all_categories(cls) -> list:
        """Get all core category IDs in canonical order."""
        return [
            cls.PATIENT_DEMOGRAPHICS,
            cls.ALLERGIES,
            cls.MEDICATIONS,
            cls.DIAGNOSES,
            cls.PROCEDURES,
            cls.LAB_RESULTS,
            cls.VITAL_SIGNS,
            cls.SOCIAL_HISTORY,
            cls.CLINICAL_NOTES,
            cls.DOCUMENT_METADATA,
        ]
    
    @classmethod
    def is_valid_category(cls, category: str) -> bool:
        """Check if a category is a valid core category."""
        return category in cls.all_categories()


class RecommendedEntityNames:
    """
    Recommended entity_name keys per category aligned to FR-039..FR-048.
    
    These are non-exhaustive but provide deterministic defaults for
    common entity types within each category.
    """
    
    PATIENT_DEMOGRAPHICS = ["name", "dob", "address", "contact", "mrn", "gender"]
    ALLERGIES = ["allergen", "reaction", "severity"]
    MEDICATIONS = ["medication_name", "dosage", "frequency", "route", "start_date", "end_date"]
    DIAGNOSES = ["condition", "date", "icd_code", "status"]
    PROCEDURES = ["procedure_name", "date", "cpt_code", "provider"]
    LAB_RESULTS = ["test_name", "value", "unit", "reference_range", "date"]
    VITAL_SIGNS = ["bp", "hr", "temp", "spo2", "weight", "height", "bmi"]
    SOCIAL_HISTORY = ["smoking", "alcohol", "occupation", "living_situation"]
    CLINICAL_NOTES = ["provider_notes", "assessment", "plan", "recommendations"]
    DOCUMENT_METADATA = ["type", "date", "provider", "facility"]
    
    @classmethod
    def get_names_for_category(cls, category: str) -> list:
        """Get recommended entity names for a category."""
        mapping = {
            CoreEntityCategories.PATIENT_DEMOGRAPHICS: cls.PATIENT_DEMOGRAPHICS,
            CoreEntityCategories.ALLERGIES: cls.ALLERGIES,
            CoreEntityCategories.MEDICATIONS: cls.MEDICATIONS,
            CoreEntityCategories.DIAGNOSES: cls.DIAGNOSES,
            CoreEntityCategories.PROCEDURES: cls.PROCEDURES,
            CoreEntityCategories.LAB_RESULTS: cls.LAB_RESULTS,
            CoreEntityCategories.VITAL_SIGNS: cls.VITAL_SIGNS,
            CoreEntityCategories.SOCIAL_HISTORY: cls.SOCIAL_HISTORY,
            CoreEntityCategories.CLINICAL_NOTES: cls.CLINICAL_NOTES,
            CoreEntityCategories.DOCUMENT_METADATA: cls.DOCUMENT_METADATA,
        }
        return mapping.get(category, [])


ENTITY_CATEGORIES = CoreEntityCategories.all_categories()
