#!/usr/bin/env python3
"""
Check database connection and tables
"""

import psycopg2
import os

def check_database():
    """Check database connection and table structure"""
    try:
        # Connect to database
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Check if ExtractedEntities table exists
        cursor.execute("""
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'ExtractedEntities'
        """)
        
        result = cursor.fetchone()
        if result:
            print("✅ ExtractedEntities table exists")
            
            # Check table structure
            cursor.execute("""
                SELECT column_name, data_type, is_nullable
                FROM information_schema.columns
                WHERE table_name = 'ExtractedEntities'
                ORDER BY ordinal_position
            """)
            
            columns = cursor.fetchall()
            print("📋 Table structure:")
            for col in columns:
                print(f"  • {col[0]}: {col[1]} (nullable: {col[2]})")
        else:
            print("❌ ExtractedEntities table does not exist")
        
        # Check if patients and documents exist
        cursor.execute("""
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name IN ('Patients', 'Documents')
        """)
        
        tables = cursor.fetchall()
        print(f"📊 Found {len(tables)} related tables: {[t[0] for t in tables]}")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Database error: {e}")

if __name__ == "__main__":
    check_database()
