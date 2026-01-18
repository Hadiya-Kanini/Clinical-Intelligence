#!/usr/bin/env python3
"""
Check patients and documents table structure
"""

import psycopg2

def check_table_structures():
    """Check patients and documents table structures"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Check patients table
        cursor.execute("""
            SELECT column_name
            FROM information_schema.columns
            WHERE table_name = 'patients'
            ORDER BY ordinal_position
        """)
        
        patient_cols = cursor.fetchall()
        print("👤 patients table columns:")
        for col in patient_cols:
            print(f"  • {col[0]}")
        
        # Check documents table
        cursor.execute("""
            SELECT column_name
            FROM information_schema.columns
            WHERE table_name = 'documents'
            ORDER BY ordinal_position
        """)
        
        doc_cols = cursor.fetchall()
        print("\n📄 documents table columns:")
        for col in doc_cols:
            print(f"  • {col[0]}")
        
        # Get real data
        cursor.execute("SELECT \"Id\" FROM patients LIMIT 1")
        patient = cursor.fetchone()
        
        cursor.execute("SELECT \"Id\" FROM documents LIMIT 1")
        document = cursor.fetchone()
        
        if patient and document:
            print(f"\n✅ Found patient: {patient[0]}, document: {document[0]}")
            
            # Test insert
            print("\n🧪 Testing insert...")
            cursor.execute("""
                INSERT INTO extracted_entities (
                    "Id", "PatientId", "DocumentId", "Category", "DisplayCategory", 
                    "Name", "Value", "IsVerified"
                ) VALUES (
                    gen_random_uuid(), 
                    %s,
                    %s,
                    'test_category',
                    'Test Category',
                    'test_name',
                    'test_value',
                    false
                )
            """, (patient[0], document[0]))
            
            conn.commit()
            print("✅ Test insert successful")
            
            # Clean up
            cursor.execute("DELETE FROM extracted_entities WHERE \"Category\" = 'test_category'")
            conn.commit()
            print("🧹 Test record cleaned up")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    check_table_structures()
