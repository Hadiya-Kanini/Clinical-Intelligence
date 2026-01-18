#!/usr/bin/env python3
"""
Test insert with real document ID
"""

import psycopg2

def test_with_real_document():
    """Test insert with a real document ID"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Get a real document ID
        cursor.execute("""
            SELECT "Id", "OriginalName" 
            FROM documents 
            LIMIT 1
        """)
        
        doc = cursor.fetchone()
        if not doc:
            print("❌ No documents found")
            return
        
        document_id = doc[0]
        print(f"📄 Using document: {document_id} ({doc[1]})")
        
        # Get a real patient ID
        cursor.execute("""
            SELECT "Id", "Name" 
            FROM patients 
            LIMIT 1
        """)
        
        patient = cursor.fetchone()
        if not patient:
            print("❌ No patients found")
            return
        
        patient_id = patient[0]
        print(f"👤 Using patient: {patient_id} ({patient[1]})")
        
        # Test insert with real IDs
        print("\n🧪 Testing insert with real IDs...")
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
        """, (patient_id, document_id))
        
        conn.commit()
        print("✅ Test insert successful")
        
        # Verify the record
        cursor.execute("""
            SELECT COUNT(*) 
            FROM extracted_entities 
            WHERE "Category" = 'test_category'
        """)
        
        count = cursor.fetchone()[0]
        print(f"📊 Found {count} test records")
        
        # Clean up
        cursor.execute("DELETE FROM extracted_entities WHERE \"Category\" = 'test_category'")
        conn.commit()
        print("🧹 Test record cleaned up")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    test_with_real_document()
