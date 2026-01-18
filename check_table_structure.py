#!/usr/bin/env python3
"""
Check the actual structure of extracted_entities table.
"""

import psycopg2
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

def check_table_structure():
    """Check the actual column names in extracted_entities table."""
    
    # Database connection
    db_url = os.getenv("DATABASE_URL", "postgresql://postgres:admin@localhost:5432/ClinicalIntelligence")
    
    try:
        conn = psycopg2.connect(db_url)
        cursor = conn.cursor()
        
        print("🔍 Checking extracted_entities table structure:")
        print("=" * 50)
        
        # Get column information
        cursor.execute("""
            SELECT column_name, data_type, is_nullable, column_default
            FROM information_schema.columns 
            WHERE table_name = 'extracted_entities' 
            AND table_schema = 'public'
            ORDER BY ordinal_position
        """)
        
        columns = cursor.fetchall()
        
        if columns:
            for col in columns:
                nullable = "NULL" if col[2] == "YES" else "NOT NULL"
                default = f" DEFAULT {col[3]}" if col[3] else ""
                print(f"  📋 {col[0]} ({col[1]}) {nullable}{default}")
        
        # Get a sample row
        cursor.execute("SELECT * FROM extracted_entities LIMIT 1")
        sample = cursor.fetchone()
        
        if sample:
            print(f"\n📊 Sample data (first row):")
            for i, value in enumerate(sample):
                col_name = columns[i][0] if i < len(columns) else f"Column{i}"
                print(f"  {col_name}: {value}")
        else:
            print("⚠️ No data in extracted_entities table")
            
    except Exception as e:
        print(f"❌ Error: {e}")
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    check_table_structure()
