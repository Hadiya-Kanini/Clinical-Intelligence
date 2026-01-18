#!/usr/bin/env python3
"""
Debug script to check entity storage without joins.
"""

import psycopg2
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

def debug_entity_storage():
    """Debug entity storage without complex joins."""
    
    # Database connection
    db_url = os.getenv("DATABASE_URL", "postgresql://postgres:admin@localhost:5432/ClinicalIntelligence")
    
    try:
        conn = psycopg2.connect(db_url)
        cursor = conn.cursor()
        
        print("🔍 Debug: Raw entity storage")
        print("=" * 40)
        
        # Get raw entities first
        cursor.execute("""
            SELECT 
                "Id", "PatientId", "DocumentId", "Category", "Name", "Value", 
                "DisplayCategory", "ConfidenceScore", "Units", "IsVerified"
            FROM extracted_entities 
            ORDER BY "CreatedAt" DESC 
            LIMIT 5
        """)
        
        entities = cursor.fetchall()
        
        if entities:
            print(f"📊 Found {len(entities)} entities:")
            for entity in entities:
                print(f"  🏷️  {entity[3]}: {entity[4]} = {entity[5]}")
                print(f"     📋 Patient: {entity[1]}")
                print(f"     📄 Document: {entity[2]}")
                print(f"     📂 Display: {entity[6]}")
                print()
        else:
            print("❌ No entities found in extracted_entities table")
            
        # Check documents table
        cursor.execute('SELECT COUNT(*) FROM documents WHERE "IsDeleted" = false')
        doc_count = cursor.fetchone()[0]
        print(f"📄 Documents (not deleted): {doc_count}")
        
        # Check patients table
        cursor.execute('SELECT COUNT(*) FROM patients WHERE "IsDeleted" = false')
        patient_count = cursor.fetchone()[0]
        print(f"👥 Patients (not deleted): {patient_count}")
            
    except Exception as e:
        print(f"❌ Error: {e}")
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    debug_entity_storage()
