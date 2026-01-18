"""
Patient database management for worker service.

Handles creating/updating patient records and linking documents.
"""

import logging
import psycopg2
from typing import Dict, Any, Optional
from datetime import datetime
import uuid

logger = logging.getLogger(__name__)


class PatientManager:
    """Manage patient records in the database"""
    
    def __init__(self, db_connection_string: str):
        self.connection_string = db_connection_string
    
    def find_or_create_patient(self, demographics: Dict[str, Any]) -> Optional[str]:
        """
        Find existing patient by MRN or create new patient record.
        
        Args:
            demographics: Dictionary containing patient information
            
        Returns:
            Patient ID (GUID) or None if failed
        """
        try:
            conn = psycopg2.connect(self.connection_string)
            cursor = conn.cursor()
            
            mrn = demographics.get('mrn')
            name = demographics.get('name')
            
            if not mrn and not name:
                logger.error("Cannot create patient without MRN or name")
                return None
            
            # Try to find existing patient by MRN
            if mrn:
                cursor.execute(
                    'SELECT "Id" FROM erd_patients WHERE "Mrn" = %s AND "IsDeleted" = false',
                    (mrn,)
                )
                result = cursor.fetchone()
                
                if result:
                    patient_id = str(result[0])
                    logger.info(f"Found existing patient with MRN {mrn}: {patient_id}")
                    
                    # Update patient information if needed
                    self._update_patient(cursor, patient_id, demographics)
                    conn.commit()
                    cursor.close()
                    conn.close()
                    return patient_id
            
            # Create new patient
            patient_id = str(uuid.uuid4())
            
            # Parse DOB
            dob = None
            if demographics.get('dob'):
                try:
                    dob = datetime.strptime(demographics['dob'], '%Y-%m-%d').date()
                except ValueError:
                    logger.warning(f"Invalid DOB format: {demographics['dob']}")
            
            # Generate MRN if not provided
            if not mrn:
                mrn = self._generate_mrn(cursor)
            
            # Insert new patient - match ErdPatient schema: Name, Dob, Contact
            cursor.execute('''
                INSERT INTO erd_patients (
                    "Id", "Mrn", "Name", "Dob", 
                    "Contact", "CreatedAt", "UpdatedAt", "IsDeleted"
                ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s)
            ''', (
                patient_id,
                mrn,
                name,  # Full name in single column
                dob,
                demographics.get('phone'),  # Phone goes in Contact column
                datetime.now(),
                datetime.now(),
                False
            ))
            
            conn.commit()
            cursor.close()
            conn.close()
            
            logger.info(f"Created new patient: {patient_id} (MRN: {mrn})")
            return patient_id
            
        except Exception as e:
            logger.error(f"Error creating/finding patient: {e}")
            return None
    
    def link_document_to_patient(self, document_id: str, patient_id: str) -> bool:
        """
        Link a document to a patient.
        
        Args:
            document_id: Document GUID
            patient_id: Patient GUID
            
        Returns:
            True if successful, False otherwise
        """
        try:
            conn = psycopg2.connect(self.connection_string)
            cursor = conn.cursor()
            
            # Document table only has PatientId, no UpdatedAt column
            cursor.execute('''
                UPDATE documents 
                SET "PatientId" = %s
                WHERE "Id" = %s
            ''', (patient_id, document_id))
            
            conn.commit()
            cursor.close()
            conn.close()
            
            logger.info(f"Linked document {document_id} to patient {patient_id}")
            return True
            
        except Exception as e:
            logger.error(f"Error linking document to patient: {e}")
            return False
    
    def update_document_status(self, document_id: str, status: str) -> bool:
        """
        Update document processing status.
        
        Args:
            document_id: Document GUID
            status: New status (Pending, Processing, Completed, Failed)
            
        Returns:
            True if successful, False otherwise
        """
        try:
            conn = psycopg2.connect(self.connection_string)
            cursor = conn.cursor()
            
            cursor.execute('''
                UPDATE documents 
                SET "Status" = %s
                WHERE "Id" = %s
            ''', (status, document_id))
            
            conn.commit()
            cursor.close()
            conn.close()
            
            logger.info(f"Updated document {document_id} status to {status}")
            return True
            
        except Exception as e:
            logger.error(f"Error updating document status: {e}")
            return False
    
    def _update_patient(self, cursor, patient_id: str, demographics: Dict[str, Any]):
        """Update existing patient with new information - match ErdPatient schema"""
        updates = []
        params = []
        
        if demographics.get('name'):
            updates.append('"Name" = %s')
            params.append(demographics['name'])
        
        if demographics.get('dob'):
            try:
                dob = datetime.strptime(demographics['dob'], '%Y-%m-%d').date()
                updates.append('"Dob" = %s')
                params.append(dob)
            except ValueError:
                pass
        
        if demographics.get('phone'):
            updates.append('"Contact" = %s')
            params.append(demographics['phone'])
        
        if updates:
            updates.append('"UpdatedAt" = %s')
            params.append(datetime.utcnow())
            params.append(patient_id)
            
            query = f'UPDATE erd_patients SET {", ".join(updates)} WHERE "Id" = %s'
            cursor.execute(query, params)
            logger.info(f"Updated patient {patient_id} with new information")
    
    def _generate_mrn(self, cursor) -> str:
        """Generate a unique MRN"""
        # Get current year
        year = datetime.now().year
        
        # Find the highest MRN for this year
        cursor.execute('''
            SELECT "Mrn" FROM erd_patients 
            WHERE "Mrn" LIKE %s 
            ORDER BY "Mrn" DESC 
            LIMIT 1
        ''', (f'MRN-{year}-%',))
        
        result = cursor.fetchone()
        
        if result:
            # Extract sequence number and increment
            last_mrn = result[0]
            try:
                seq = int(last_mrn.split('-')[-1]) + 1
            except ValueError:
                seq = 1
        else:
            seq = 1
        
        return f'MRN-{year}-{seq:04d}'
    
    def _extract_first_name(self, full_name: Optional[str]) -> Optional[str]:
        """Extract first name from full name"""
        if not full_name:
            return None
        parts = full_name.split()
        return parts[0] if parts else None
    
    def _extract_last_name(self, full_name: Optional[str]) -> Optional[str]:
        """Extract last name from full name"""
        if not full_name:
            return None
        parts = full_name.split()
        return ' '.join(parts[1:]) if len(parts) > 1 else None
