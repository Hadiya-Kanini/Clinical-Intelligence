#!/usr/bin/env python3
"""
Check all tables in database
"""

import psycopg2

def check_all_tables():
    """Check all tables in the database"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Get all tables
        cursor.execute("""
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public'
            ORDER BY table_name
        """)
        
        tables = cursor.fetchall()
        print(f"📊 Found {len(tables)} tables:")
        for table in tables:
            print(f"  • {table[0]}")
        
        # Look for entity-related tables
        entity_tables = [t[0] for t in tables if 'entity' in t[0].lower()]
        if entity_tables:
            print(f"\n🎯 Entity-related tables: {entity_tables}")
        else:
            print("\n⚠️ No entity-related tables found")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Database error: {e}")

if __name__ == "__main__":
    check_all_tables()
