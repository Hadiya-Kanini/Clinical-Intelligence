"""
Patient demographics extraction from clinical documents.

Extracts patient information like name, MRN, DOB, gender, contact info.
"""

import re
import logging
from typing import Dict, Optional, Any
from datetime import datetime

logger = logging.getLogger(__name__)


class PatientDemographicsExtractor:
    """Extract patient demographics from document text"""
    
    def __init__(self):
        # Patterns for extracting patient information
        self.patterns = {
            'patient_id': [
                r'Patient\s*ID\s*:\s*(\d+)',
                r'PatientID\s*:\s*(\d+)',
                r'MRN\s*:\s*([A-Z0-9\-]+)',
                r'Medical\s*Record\s*Number\s*:\s*([A-Z0-9\-]+)',
            ],
            'name': [
                r'Patient\s*Name\s*:\s*([A-Za-z\s,]+)',
                r'Name\s*:\s*([A-Za-z\s,]+)',
                r'PATIENT\s*INFORMATION\s*:\s*([A-Za-z\s]+)',
            ],
            'dob': [
                r'DOB\s*:\s*(\d{1,2}/\d{1,2}/\d{4})',
                r'Date\s*of\s*Birth\s*:\s*(\d{1,2}/\d{1,2}/\d{4})',
                r'Birth\s*Date\s*:\s*(\d{1,2}/\d{1,2}/\d{4})',
            ],
            'gender': [
                r'Gender\s*:\s*(Male|Female|Other|M|F)',
                r'Sex\s*:\s*(Male|Female|M|F)',
            ],
            'phone': [
                r'Phone\s*:\s*(\+?\d[\d\s\-\(\)]+)',
                r'Contact\s*:\s*(\+?\d[\d\s\-\(\)]+)',
                r'Tel\s*:\s*(\+?\d[\d\s\-\(\)]+)',
            ],
            'age': [
                r'Age\s*:\s*(\d+)',
            ]
        }
    
    def extract_patient_demographics(self, text: str) -> Dict[str, Any]:
        """
        Extract patient demographics from document text.
        
        Args:
            text: Full text extracted from document
            
        Returns:
            Dictionary containing extracted patient information
        """
        demographics = {}
        
        # Extract each field
        demographics['mrn'] = self._extract_field(text, 'patient_id')
        demographics['name'] = self._extract_field(text, 'name')
        demographics['dob'] = self._extract_field(text, 'dob')
        demographics['gender'] = self._extract_field(text, 'gender')
        demographics['phone'] = self._extract_field(text, 'phone')
        demographics['age'] = self._extract_field(text, 'age')
        
        # Clean up extracted data
        demographics = self._clean_demographics(demographics)
        
        # Validate extracted data
        is_valid, validation_errors = self._validate_demographics(demographics)
        demographics['is_valid'] = is_valid
        demographics['validation_errors'] = validation_errors
        
        logger.info(
            "Extracted patient demographics: MRN=%s, Name=%s, DOB=%s, Valid=%s",
            demographics.get('mrn'),
            demographics.get('name'),
            demographics.get('dob'),
            is_valid
        )
        
        return demographics
    
    def _extract_field(self, text: str, field_name: str) -> Optional[str]:
        """Extract a specific field using regex patterns"""
        patterns = self.patterns.get(field_name, [])
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE | re.MULTILINE)
            if match:
                value = match.group(1).strip()
                if value:
                    logger.debug(f"Extracted {field_name}: {value}")
                    return value
        
        logger.debug(f"Could not extract {field_name}")
        return None
    
    def _clean_demographics(self, demographics: Dict[str, Any]) -> Dict[str, Any]:
        """Clean and normalize extracted demographics"""
        cleaned = {}
        
        # Clean MRN
        if demographics.get('mrn'):
            cleaned['mrn'] = demographics['mrn'].strip().upper()
        
        # Clean name - remove extra whitespace, capitalize properly
        if demographics.get('name'):
            name = demographics['name'].strip()
            # Remove common prefixes/suffixes
            name = re.sub(r'\b(Mr|Mrs|Ms|Dr|MD|RN|LPN)\.?\b', '', name, flags=re.IGNORECASE)
            name = ' '.join(name.split())  # Normalize whitespace
            cleaned['name'] = name.title()
        
        # Clean DOB - convert to ISO format
        if demographics.get('dob'):
            cleaned['dob'] = self._parse_date(demographics['dob'])
        
        # Normalize gender
        if demographics.get('gender'):
            gender = demographics['gender'].upper()
            if gender in ['M', 'MALE']:
                cleaned['gender'] = 'Male'
            elif gender in ['F', 'FEMALE']:
                cleaned['gender'] = 'Female'
            else:
                cleaned['gender'] = gender
        
        # Clean phone
        if demographics.get('phone'):
            phone = re.sub(r'[^\d+]', '', demographics['phone'])
            cleaned['phone'] = phone
        
        # Clean age
        if demographics.get('age'):
            try:
                cleaned['age'] = int(demographics['age'])
            except ValueError:
                pass
        
        return cleaned
    
    def _parse_date(self, date_str: str) -> Optional[str]:
        """Parse date string to ISO format (YYYY-MM-DD)"""
        try:
            # Try MM/DD/YYYY format
            dt = datetime.strptime(date_str, '%m/%d/%Y')
            return dt.strftime('%Y-%m-%d')
        except ValueError:
            try:
                # Try DD/MM/YYYY format
                dt = datetime.strptime(date_str, '%d/%m/%Y')
                return dt.strftime('%Y-%m-%d')
            except ValueError:
                logger.warning(f"Could not parse date: {date_str}")
                return date_str
    
    def _validate_demographics(self, demographics: Dict[str, Any]) -> tuple[bool, list]:
        """
        Validate extracted demographics.
        
        Returns:
            Tuple of (is_valid, list of validation errors)
        """
        errors = []
        
        # At minimum, we need either MRN or Name (relaxed validation)
        if not demographics.get('mrn') and not demographics.get('name'):
            errors.append("Missing both MRN and patient name")
        
        # If we have at least one identifier, consider it valid for basic linking
        # This allows documents with only MRN or only name to be linked to patients
        
        # Validate DOB format if present
        if demographics.get('dob'):
            if not re.match(r'\d{4}-\d{2}-\d{2}', demographics['dob']):
                errors.append("Invalid DOB format")
        
        # Validate age if present
        if demographics.get('age'):
            age = demographics.get('age')
            if not isinstance(age, int) or age < 0 or age > 150:
                errors.append("Invalid age value")
        
        is_valid = len(errors) == 0
        return is_valid, errors


def extract_patient_from_text(text: str) -> Dict[str, Any]:
    """
    Convenience function to extract patient demographics from text.
    
    Args:
        text: Full text extracted from document
        
    Returns:
        Dictionary containing extracted patient information
    """
    extractor = PatientDemographicsExtractor()
    return extractor.extract_patient_demographics(text)
