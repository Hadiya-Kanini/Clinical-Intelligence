#!/usr/bin/env python3
"""
Check timestamp columns and test proper insert
"""

import psycopg2

def check_timestamp_columns():
    """Check timestamp columns and test proper insert"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Get timestamp columns
        cursor.execute("""
            SELECT column_name, data_type
            FROM information_schema.columns
            WHERE table_name = 'extracted_entities' 
            AND data_type LIKE '%timestamp%'
            ORDER BY ordinal_position
        """)
        
        timestamp_cols = cursor.fetchall()
        print("🕒 Timestamp columns:")
        for col in timestamp_cols:
            print(f"  • {col[0]}: {col[1]}")
        
        # Try insert without CreatedAt/UpdatedAt
        print("\n🧪 Testing insert without CreatedAt/UpdatedAt...")
        cursor.execute("""
            INSERT INTO extracted_entities (
                "Id", "PatientId", "DocumentId", "Category", "DisplayCategory", 
                "Name", "Value", "IsVerified"
            ) VALUES (
                gen_random_uuid(), 
                '00000000-0000-0000-0000-000000012345',
                '00000000-0000-0000-0000-000000000001',
                'test_category',
                'Test Category',
                'test_name',
                'test_value',
                false
            )
        """)
        
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
    check_timestamp_columns()
