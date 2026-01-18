"""
Pydantic models for entity extraction schema version 1.0.

Aligned to contracts/entities/v1/entity.schema.json.
"""

from typing import Any, Dict, List, Literal, Optional

from pydantic import BaseModel, Field, field_validator


class CoordinatesV1(BaseModel):
    """Coordinates within a document page."""
    
    x: float = Field(..., description="X coordinate")
    y: float = Field(..., description="Y coordinate")
    width: float = Field(..., description="Width of the region")
    height: float = Field(..., description="Height of the region")


class DocumentLocationV1(BaseModel):
    """Location information within a document."""
    
    page: Optional[int] = Field(None, ge=1, description="Page number (1-indexed)")
    section: Optional[str] = Field(None, description="Section name")
    coordinates: Optional[CoordinatesV1] = Field(None, description="Bounding box coordinates")


class ConflictV1(BaseModel):
    """Conflicting value from another source."""
    
    conflicting_value: str = Field(..., description="The conflicting value found")
    source_document: Optional[str] = Field(None, description="Source document identifier")
    document_location: Optional[DocumentLocationV1] = Field(
        None, description="Location in the source document"
    )


class ExtractedEntityV1(BaseModel):
    """A single extracted entity with provenance."""
    
    entity_group_name: str = Field(
        ..., 
        description="Category/group of the entity (e.g., patient_demographics, medications)"
    )
    entity_name: str = Field(
        ..., 
        description="Specific name/label of the entity (e.g., name, dob, dosage)"
    )
    entity_value: str = Field(
        ..., 
        description="The extracted value"
    )
    rationale: Optional[str] = Field(
        None, 
        description="Explanation of why this value was extracted"
    )
    source_text: Optional[str] = Field(
        None, 
        description="Exact text from source document"
    )
    document_location: Optional[DocumentLocationV1] = Field(
        None, 
        description="Location in the source document"
    )
    conflicts: Optional[List[ConflictV1]] = Field(
        default_factory=list, 
        description="Conflicting values from other sources"
    )

    @field_validator("entity_group_name", "entity_name", "entity_value")
    @classmethod
    def validate_non_empty_string(cls, v: str, info) -> str:
        """Ensure required string fields are non-empty."""
        if not v or not v.strip():
            raise ValueError(f"{info.field_name} must be a non-empty string")
        return v


class EntityExtractionResultV1(BaseModel):
    """
    Top-level entity extraction result for schema version 1.0.
    
    Aligned to contracts/entities/v1/entity.schema.json.
    """
    
    schema_version: Literal["1.0"] = Field(
        ..., 
        description="Schema version identifier"
    )
    document_id: str = Field(
        ..., 
        description="Identifier for the document processed"
    )
    extracted_entities: List[ExtractedEntityV1] = Field(
        ..., 
        description="List of extracted entities"
    )
    additional_entities: Optional[Dict[str, Any]] = Field(
        None, 
        description="Extension point for additional extracted data"
    )

    @field_validator("document_id")
    @classmethod
    def validate_document_id(cls, v: str) -> str:
        """Ensure document_id is non-empty."""
        if not v or not v.strip():
            raise ValueError("document_id must be a non-empty string")
        return v

    @field_validator("extracted_entities")
    @classmethod
    def validate_extracted_entities(cls, v: List[ExtractedEntityV1]) -> List[ExtractedEntityV1]:
        """Validate extracted_entities is a list (can be empty)."""
        if v is None:
            raise ValueError("extracted_entities is required")
        return v
