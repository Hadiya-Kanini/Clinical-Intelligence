"""DOCX text extraction using LangChain Docx2txtLoader with positional metadata."""

import os
from typing import Optional

from .models import ExtractedTextResult, DocumentLocation, ExtractedTextSegment


def extract_docx_text(storage_path: str, document_id: Optional[str] = None) -> ExtractedTextResult:
    """
    Extract text from a DOCX file using Docx2txtLoader.
    
    Args:
        storage_path: Path to the DOCX file.
        document_id: Optional document identifier. Defaults to filename if not provided.
    
    Returns:
        ExtractedTextResult with segments. Note: DOCX typically does not provide
        page numbers or coordinates, so these will be null.
    
    Raises:
        FileNotFoundError: If the DOCX file does not exist.
        ValueError: If the file is not a valid DOCX.
    """
    if not os.path.exists(storage_path):
        raise FileNotFoundError(f"DOCX file not found: {storage_path}")
    
    if document_id is None:
        document_id = os.path.basename(storage_path)
    
    result = ExtractedTextResult(document_id=document_id)
    
    try:
        from langchain_community.document_loaders import Docx2txtLoader
        
        loader = Docx2txtLoader(storage_path)
        documents = loader.load()
        
        for doc in documents:
            text = doc.page_content
            if not text or not text.strip():
                continue
            
            paragraphs = _split_into_paragraphs(text)
            
            for para_text in paragraphs:
                if not para_text.strip():
                    continue
                
                section = _detect_section_heading(para_text)
                
                result.add_segment(
                    text=para_text.strip(),
                    page=None,
                    section=section,
                    coordinates=None
                )
    
    except ImportError as e:
        raise ImportError(
            "LangChain DOCX dependencies not installed. "
            "Run: pip install langchain-community docx2txt"
        ) from e
    except Exception as e:
        raise ValueError(f"Failed to extract text from DOCX: {e}") from e
    
    return result


def _split_into_paragraphs(text: str) -> list:
    """Split text into paragraphs based on double newlines or significant breaks."""
    paragraphs = []
    current = []
    
    for line in text.split('\n'):
        stripped = line.strip()
        if stripped:
            current.append(stripped)
        elif current:
            paragraphs.append(' '.join(current))
            current = []
    
    if current:
        paragraphs.append(' '.join(current))
    
    return paragraphs


def _detect_section_heading(text: str) -> Optional[str]:
    """
    Best-effort detection of section headings.
    Returns the heading text if detected, None otherwise.
    """
    heading_indicators = [
        "SECTION",
        "CHAPTER",
        "PART",
        "APPENDIX",
        "INTRODUCTION",
        "CONCLUSION",
        "SUMMARY",
        "ABSTRACT",
        "BACKGROUND",
        "METHODS",
        "RESULTS",
        "DISCUSSION",
        "REFERENCES",
        "DIAGNOSIS",
        "TREATMENT",
        "HISTORY",
        "EXAMINATION",
        "ASSESSMENT",
        "PLAN",
        "MEDICATIONS",
        "ALLERGIES",
        "VITAL SIGNS",
        "CHIEF COMPLAINT",
        "PRESENT ILLNESS",
    ]
    
    upper_text = text.upper().strip()
    
    if len(text) < 100 and text.strip().endswith(':'):
        return text.strip().rstrip(':')
    
    for indicator in heading_indicators:
        if upper_text.startswith(indicator):
            return text.strip()
    
    if len(text) < 50 and text.isupper():
        return text.strip()
    
    return None
