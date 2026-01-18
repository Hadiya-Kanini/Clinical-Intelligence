#!/usr/bin/env python3
"""
Test script to verify entities are stored in database and can be retrieved.
"""

import psycopg2
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

def test_entity_storage():
    """Test that entities are properly stored and can be retrieved."""
    
    # Database connection
    db_url = os.getenv("DATABASE_URL", "postgresql://postgres:admin@localhost:5432/ClinicalIntelligence")
    
    try:
        conn = psycopg2.connect(db_url)
        cursor = conn.cursor()
        
        print("🔍 Testing entity storage...")
        
        # Check if any entities exist
        cursor.execute("SELECT COUNT(*) FROM extracted_entities")
        entity_count = cursor.fetchone()[0]
        print(f"📊 Total entities in database: {entity_count}")
        
        if entity_count > 0:
            # Get the most recent entities
            query = """
            SELECT 
                e."Category" as "EntityGroupName",
                e."Name" as "EntityName", 
                e."Value" as "EntityValue",
                e."DisplayCategory",
                e."ConfidenceScore" as "Confidence",
                e."Units",
                e."IsVerified",
                e."EffectiveAt",
                p."GivenName" || ' ' || p."FamilyName" as "PatientName",
                p."Mrn" as "PatientMRN",
                d."OriginalName" as "DocumentName",
                d."UploadedAt" as "DocumentDate"
            FROM extracted_entities e
            JOIN documents d ON e."DocumentId" = d."Id"
            JOIN patients p ON e."PatientId" = p."Id"
            WHERE d."IsDeleted" = false 
            AND p."IsDeleted" = false
            ORDER BY d."UploadedAt" DESC, e."Category", e."Name"
            LIMIT 10
            """
            
            cursor.execute(query)
            entities = cursor.fetchall()
            
            print(f"\n📋 Latest {len(entities)} entities:")
            print("-" * 80)
            
            for entity in entities:
                print(f"🏷️  {entity[0]}: {entity[1]} = {entity[2]}")
                if entity[3]:
                    print(f"   📂 Display: {entity[3]}")
                if entity[4]:
                    print(f"   🎯 Confidence: {entity[4]:.2f}")
                if entity[6]:
                    print(f"   ✅ Verified: {entity[6]}")
                print(f"   👤 Patient: {entity[7]} ({entity[8]})")
                print(f"   📄 Document: {entity[9]}")
                print()
            
            print("✅ Entity storage test PASSED!")
            return True
        else:
            print("⚠️ No entities found in database")
            return False
            
    except Exception as e:
        print(f"❌ Database error: {e}")
        return False
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    test_entity_storage()
