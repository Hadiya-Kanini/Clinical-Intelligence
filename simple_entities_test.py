#!/usr/bin/env python3
"""
Simple entity query without joins to test data.
"""

import psycopg2
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

def test_simple_entities():
    """Test simple entity query without joins."""
    
    # Database connection
    db_url = os.getenv("DATABASE_URL", "postgresql://postgres:admin@localhost:5432/ClinicalIntelligence")
    
    try:
        conn = psycopg2.connect(db_url)
        cursor = conn.cursor()
        
        print("🔍 Simple Entity Query")
        print("=" * 30)
        
        # Simple query without joins
        cursor.execute("""
            SELECT 
                "Category", "Name", "Value", "DisplayCategory", 
                "ConfidenceScore", "Units", "IsVerified", "PatientId", "DocumentId"
            FROM extracted_entities 
            ORDER BY "Category", "Name"
            LIMIT 10
        """)
        
        entities = cursor.fetchall()
        
        if entities:
            print(f"📊 Found {len(entities)} entities:")
            for entity in entities:
                print(f"  🏷️  {entity[0]}: {entity[1]} = {entity[2]}")
                print(f"     📋 Patient: {entity[7]}")
                print(f"     📄 Document: {entity[8]}")
                print(f"     📂 Display: {entity[3]}")
                print()
        else:
            print("❌ No entities found")
            
    except Exception as e:
        print(f"❌ Error: {e}")
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    test_simple_entities()
