#!/usr/bin/env python3
"""
Check ERD patients structure and test entity storage
"""

import psycopg2

def check_erd_patients():
    """Check ERD patients table structure"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Get ERD patients structure
        cursor.execute("""
            SELECT column_name
            FROM information_schema.columns
            WHERE table_name = 'erd_patients'
            ORDER BY ordinal_position
        """)
        
        columns = cursor.fetchall()
        print("👤 ERD patients table columns:")
        for col in columns:
            print(f"  • {col[0]}")
        
        # Get a sample patient
        cursor.execute("SELECT * FROM erd_patients LIMIT 1")
        patient = cursor.fetchone()
        
        if patient:
            print(f"\n📊 Sample ERD patient: {patient}")
            patient_id = patient[0]  # Assuming first column is Id
            
            # Get a document and link it
            cursor.execute("SELECT \"Id\" FROM documents LIMIT 1")
            document = cursor.fetchone()
            
            if document:
                doc_id = document[0]
                print(f"📄 Using document: {doc_id}")
                
                # Link document to ERD patient
                cursor.execute("""
                    UPDATE documents 
                    SET "PatientId" = %s 
                    WHERE "Id" = %s
                """, (patient_id, doc_id))
                
                conn.commit()
                print(f"🔗 Linked document to ERD patient")
                
                conn.close()
                return patient_id, doc_id
        
        conn.close()
        return None, None
        
    except Exception as e:
        print(f"❌ Error: {e}")
        return None, None

if __name__ == "__main__":
    patient_id, doc_id = check_erd_patients()
    
    if patient_id and doc_id:
        print(f"\n✅ Ready to test with ERD patient: {patient_id}, document: {doc_id}")
    else:
        print("❌ Could not get ERD patient or document")
