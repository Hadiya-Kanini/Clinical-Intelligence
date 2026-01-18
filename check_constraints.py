#!/usr/bin/env python3
"""
Check foreign key constraints and create proper patient
"""

import psycopg2

def check_constraints():
    """Check foreign key constraints"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Check constraints on documents table
        cursor.execute("""
            SELECT conname, conrelid, confrelid, pg_get_constraintdef(oid)
            FROM pg_constraint 
            WHERE conrelid = 'documents'::regclass 
            AND contype = 'f'
        """)
        
        constraints = cursor.fetchall()
        print("🔗 Foreign key constraints on documents table:")
        for constraint in constraints:
            print(f"  • {constraint[0]}: {constraint[3]}")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Error: {e}")

def create_patient_properly():
    """Create patient using proper method"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Check if we need to use ERD patients table
        cursor.execute("""
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_name LIKE '%patient%'
            ORDER BY table_name
        """)
        
        patient_tables = cursor.fetchall()
        print("👤 Patient-related tables:")
        for table in patient_tables:
            print(f"  • {table[0]}")
        
        # Check ERD patients table
        cursor.execute("SELECT COUNT(*) FROM erd_patients")
        erd_count = cursor.fetchone()[0]
        print(f"\n📊 ERD patients count: {erd_count}")
        
        if erd_count > 0:
            cursor.execute("SELECT \"Id\", \"GivenName\", \"FamilyName\" FROM erd_patients LIMIT 1")
            patient = cursor.fetchone()
            print(f"  Sample ERD patient: {patient[0]} - {patient[1]} {patient[2]}")
            
            # Use this patient ID for testing
            patient_id = patient[0]
            
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
    check_constraints()
    patient_id, doc_id = create_patient_properly()
    
    if patient_id and doc_id:
        print(f"\n✅ Ready to test with patient: {patient_id}, document: {doc_id}")
