"""
Entity extraction prompt builder for single-call Gemini extraction.

Builds a structured prompt that requests all 10 core entity categories
in a single API call with strict JSON output requirements.
"""

from typing import List, Optional
from .models import (
    ChunkWithProvenance,
    ENTITY_CATEGORIES,
    CoreEntityCategories,
    RecommendedEntityNames,
)


SYSTEM_INSTRUCTION = """You are a clinical entity extraction system. Your task is to extract structured medical entities from clinical document text.

CRITICAL REQUIREMENTS:
1. Output ONLY valid JSON - no markdown, no explanations, no additional text
2. Follow the exact schema provided
3. Ground every entity in the source text - include source_text and document_location when available
4. If conflicting values appear across different chunks, include them in the conflicts array
5. Do NOT hallucinate or invent entities not present in the text
6. Omit entity categories that have no relevant data rather than including empty arrays"""


EXTRACTION_PROMPT_TEMPLATE = """You are a clinical data extraction expert. Extract SPECIFIC, MEANINGFUL clinical entities from the following medical document.

DOCUMENT ID: {document_id}

ENTITY CATEGORIES TO EXTRACT (use exact entity_group_name values):
1. patient_demographics - Extract: name (full name), dob (YYYY-MM-DD format), mrn, gender, contact (phone), address
2. allergies - Extract: allergen (e.g., "Penicillin", "Latex"), reaction (e.g., "Anaphylaxis", "Rash"), severity
3. medications - Extract: medication_name (e.g., "Lisinopril", "Metformin"), dosage (e.g., "10mg"), frequency (e.g., "daily"), route (e.g., "oral")
4. diagnoses - Extract: condition (e.g., "Essential hypertension", "Type 2 diabetes"), date (YYYY-MM-DD), status (e.g., "active", "resolved")
5. procedures - Extract: procedure_name (e.g., "Cardiac catheterization"), date (YYYY-MM-DD), provider (doctor name)
6. lab_results - Extract: test_name (e.g., "CBC", "Lipid panel"), value (e.g., "140/90", "5.1"), unit (e.g., "mmHg", "mg/dL"), reference_range
7. vital_signs - Extract: bp (e.g., "128/78"), hr (e.g., "72"), temp (e.g., "98.6"), spo2 (e.g., "98%"), weight, height
8. social_history - Extract: smoking (e.g., "Former smoker", "Never"), alcohol (e.g., "Social", "None"), occupation
9. clinical_notes - Extract: assessment (clinical findings), plan (treatment plan), provider_notes (doctor observations)
10. document_metadata - Extract: document_type (e.g., "Discharge summary", "Progress note"), date, provider, facility

CRITICAL EXTRACTION RULES:
- Extract ACTUAL clinical values, NOT generic placeholders
- For diagnoses: Use specific medical conditions (e.g., "Essential hypertension" NOT "diagnosis_mentioned")
- For medications: Use specific drug names with dosages (e.g., "Lisinopril 10mg" NOT "medication_present")
- For lab results: Extract actual measured values with units (e.g., "BP: 128/78 mmHg" NOT "lab_result_present")
- For demographics: Extract real patient data (e.g., "Olivia Phone" NOT "patient_identified")
- OMIT categories with no meaningful data - do NOT include placeholder entities
- Only include entities that have specific, extractable values from the text

EXAMPLES OF GOOD EXTRACTIONS:
- GOOD: "entity_group_name": "diagnoses", "entity_name": "Essential hypertension", "entity_value": "Essential hypertension"
- BAD: "entity_group_name": "diagnoses", "entity_name": "diagnosis_mentioned", "entity_value": "diagnosis_information_present"
- GOOD: "entity_group_name": "medications", "entity_name": "Lisinopril", "entity_value": "Lisinopril 10mg daily"
- BAD: "entity_group_name": "medications", "entity_name": "medication_present", "entity_value": "medication_information_present"

SOURCE CHUNKS:
{formatted_chunks}

OUTPUT SCHEMA (strict JSON):
{{
  "schema_version": "1.0",
  "document_id": "{document_id}",
  "extracted_entities": [
    {{
      "entity_group_name": "<category from list above>",
      "entity_name": "<specific clinical entity name>",
      "entity_value": "<actual extracted clinical value>",
      "rationale": "<brief explanation of what was extracted and why>",
      "source_text": "<exact text from source containing the entity>",
      "document_location": {{
        "page": <page number if known>,
        "section": "<section name if known>"
      }},
      "conflicts": []
    }}
  ],
  "additional_entities": {{}}
}}

Respond with ONLY the JSON object, no other text."""


def build_entity_extraction_prompt(
    document_id: str,
    chunks: List[ChunkWithProvenance],
    patient_id: Optional[str] = None
) -> str:
    """
    Build a single-call entity extraction prompt for Gemini.
    
    Args:
        document_id: The document ID being processed.
        chunks: List of text chunks with provenance information.
        patient_id: Optional patient ID for context.
    
    Returns:
        A formatted prompt string suitable for Gemini API.
    
    Raises:
        ValueError: If document_id is empty or chunks list is empty.
    """
    if not document_id:
        raise ValueError("document_id is required")
    if not chunks:
        raise ValueError("At least one chunk is required for extraction")
    
    formatted_chunks = _format_chunks(chunks)
    
    prompt = EXTRACTION_PROMPT_TEMPLATE.format(
        document_id=document_id,
        formatted_chunks=formatted_chunks
    )
    
    return prompt


def get_system_instruction() -> str:
    """Get the system instruction for entity extraction."""
    return SYSTEM_INSTRUCTION


def get_entity_categories() -> List[str]:
    """Get the list of entity categories to extract."""
    return ENTITY_CATEGORIES.copy()


def _format_chunks(chunks: List[ChunkWithProvenance]) -> str:
    """
    Format chunks with their provenance for inclusion in the prompt.
    
    Args:
        chunks: List of chunks with provenance.
    
    Returns:
        Formatted string representation of all chunks.
    """
    formatted_parts = []
    
    for i, chunk in enumerate(chunks, start=1):
        header_parts = [f"[CHUNK {i}]"]
        
        if chunk.document_id:
            header_parts.append(f"Document: {chunk.document_id}")
        if chunk.page is not None:
            header_parts.append(f"Page: {chunk.page}")
        if chunk.section:
            header_parts.append(f"Section: {chunk.section}")
        if chunk.rank is not None:
            header_parts.append(f"Relevance Rank: {chunk.rank}")
        
        header = " | ".join(header_parts)
        
        chunk_text = f"{header}\n{chunk.text}"
        formatted_parts.append(chunk_text)
    
    return "\n\n---\n\n".join(formatted_parts)


def validate_prompt_content(prompt: str) -> bool:
    """
    Validate that the prompt contains required elements.
    
    Args:
        prompt: The generated prompt string.
    
    Returns:
        True if prompt contains all required elements.
    """
    required_elements = [
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
        "schema_version",
        "extracted_entities",
        "conflicts",
        "source_text",
        "document_location",
        "JSON",
        "OMIT",
    ]
    
    return all(element in prompt for element in required_elements)


def get_category_taxonomy() -> dict:
    """
    Get the canonical category taxonomy with recommended entity names.
    
    Returns:
        Dictionary mapping category IDs to their recommended entity names.
    """
    return {
        category: RecommendedEntityNames.get_names_for_category(category)
        for category in CoreEntityCategories.all_categories()
    }
