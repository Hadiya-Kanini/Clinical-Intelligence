#!/usr/bin/env python3
"""
Check extracted_entities table structure
"""

import psycopg2

def check_extracted_entities_table():
    """Check the structure of the extracted_entities table"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Get table structure
        cursor.execute("""
            SELECT column_name, data_type, is_nullable, column_default
            FROM information_schema.columns
            WHERE table_name = 'extracted_entities'
            ORDER BY ordinal_position
        """)
        
        columns = cursor.fetchall()
        print("📋 extracted_entities table structure:")
        for col in columns:
            print(f"  • {col[0]}: {col[1]} (nullable: {col[2]}, default: {col[3]})")
        
        # Try to insert a test record
        print("\n🧪 Testing insert...")
        cursor.execute("""
            INSERT INTO extracted_entities (
                id, patient_id, document_id, category, display_category, 
                name, value, is_verified, created_at, updated_at
            ) VALUES (
                gen_random_uuid(), 
                '00000000-0000-0000-0000-000000012345',
                '00000000-0000-0000-0000-000000000001',
                'test_category',
                'Test Category',
                'test_name',
                'test_value',
                false,
                NOW(),
                NOW()
            )
        """)
        
        conn.commit()
        print("✅ Test insert successful")
        
        # Clean up
        cursor.execute("DELETE FROM extracted_entities WHERE category = 'test_category'")
        conn.commit()
        print("🧹 Test record cleaned up")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    check_extracted_entities_table()
