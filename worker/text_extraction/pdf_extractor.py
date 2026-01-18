"""PDF text extraction using LangChain PyPDFLoader with positional metadata."""

import os
from typing import Optional

from .models import ExtractedTextResult, DocumentLocation, ExtractedTextSegment


def extract_pdf_text(storage_path: str, document_id: Optional[str] = None) -> ExtractedTextResult:
    """
    Extract text from a PDF file using PyPDFLoader.
    
    Args:
        storage_path: Path to the PDF file.
        document_id: Optional document identifier. Defaults to filename if not provided.
    
    Returns:
        ExtractedTextResult with segments containing page metadata.
    
    Raises:
        FileNotFoundError: If the PDF file does not exist.
        ValueError: If the file is not a valid PDF.
    """
    if not os.path.exists(storage_path):
        raise FileNotFoundError(f"PDF file not found: {storage_path}")
    
    if document_id is None:
        document_id = os.path.basename(storage_path)
    
    result = ExtractedTextResult(document_id=document_id)
    
    try:
        from langchain_community.document_loaders import PyPDFLoader
        
        loader = PyPDFLoader(storage_path)
        documents = loader.load()
        
        for doc in documents:
            text = doc.page_content
            if not text or not text.strip():
                continue
            
            metadata = doc.metadata or {}
            page_num = metadata.get("page")
            if page_num is not None:
                page_num = page_num + 1
            
            source = metadata.get("source")
            section = None
            
            coordinates = None
            if "bbox" in metadata:
                bbox = metadata["bbox"]
                if isinstance(bbox, (list, tuple)) and len(bbox) >= 4:
                    coordinates = {
                        "x0": bbox[0],
                        "y0": bbox[1],
                        "x1": bbox[2],
                        "y1": bbox[3]
                    }
            
            result.add_segment(
                text=text.strip(),
                page=page_num,
                section=section,
                coordinates=coordinates
            )
    
    except ImportError as e:
        raise ImportError(
            "LangChain PDF dependencies not installed. "
            "Run: pip install langchain-community pypdf"
        ) from e
    except Exception as e:
        raise ValueError(f"Failed to extract text from PDF: {e}") from e
    
    return result
