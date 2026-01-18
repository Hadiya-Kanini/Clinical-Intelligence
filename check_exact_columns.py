#!/usr/bin/env python3
"""
Check exact column names in extracted_entities
"""

import psycopg2

def check_exact_columns():
    """Check the exact column names case-sensitive"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Get exact column names
        cursor.execute("""
            SELECT column_name
            FROM information_schema.columns
            WHERE table_name = 'extracted_entities'
            ORDER BY ordinal_position
        """)
        
        columns = cursor.fetchall()
        print("📋 Exact column names:")
        for col in columns:
            print(f"  '{col[0]}'")
        
        # Try insert with correct case
        print("\n🧪 Testing insert with correct column names...")
        cursor.execute("""
            INSERT INTO extracted_entities (
                "Id", "PatientId", "DocumentId", "Category", "DisplayCategory", 
                "Name", "Value", "IsVerified", "CreatedAt", "UpdatedAt"
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
        cursor.execute("DELETE FROM extracted_entities WHERE \"Category\" = 'test_category'")
        conn.commit()
        print("🧹 Test record cleaned up")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    check_exact_columns()
