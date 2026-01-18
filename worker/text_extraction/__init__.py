"""Text extraction module for processing PDF and DOCX documents."""

from .models import ExtractedTextSegment, DocumentLocation, ExtractedTextResult
from .pdf_extractor import extract_pdf_text
from .docx_extractor import extract_docx_text

__all__ = [
    "ExtractedTextSegment",
    "DocumentLocation",
    "ExtractedTextResult",
    "extract_pdf_text",
    "extract_docx_text",
]
