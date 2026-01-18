#!/usr/bin/env python3
"""
Working 360° view query with correct column names.
"""

import psycopg2
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

def get_entities_360_view():
    """Get entities formatted for 360° view display."""
    
    # Database connection
    db_url = os.getenv("DATABASE_URL", "postgresql://postgres:admin@localhost:5432/ClinicalIntelligence")
    
    try:
        conn = psycopg2.connect(db_url)
        cursor = conn.cursor()
        
        print("🎯 360° View - Extracted Entities")
        print("=" * 50)
        
        # Query with correct column names
        query = """
        SELECT 
            e."Category" as category,
            e."Name" as name, 
            e."Value" as value,
            e."DisplayCategory" as display_category,
            e."ConfidenceScore" as confidence,
            e."Units" as units,
            e."IsVerified" as verified,
            e."EffectiveAt" as effective_at,
            p."GivenName" || ' ' || p."FamilyName" as patient_name,
            p."Mrn" as patient_mrn,
            d."OriginalName" as document_name,
            d."UploadedAt" as document_date
        FROM extracted_entities e
        JOIN documents d ON e."DocumentId" = d."Id"
        JOIN patients p ON e."PatientId" = p."Id"
        WHERE d."IsDeleted" = false 
        AND p."IsDeleted" = false
        ORDER BY e."Category", e."Name"
        """
        
        cursor.execute(query)
        entities = cursor.fetchall()
        
        if entities:
            # Group entities by category
            categories = {}
            for entity in entities:
                cat = entity[0] or 'Unknown'
                if cat not in categories:
                    categories[cat] = []
                categories[cat].append({
                    'name': entity[1],
                    'value': entity[2],
                    'display_category': entity[3],
                    'confidence': entity[4],
                    'units': entity[5],
                    'verified': entity[6],
                    'effective_at': entity[7],
                    'patient_name': entity[8],
                    'patient_mrn': entity[9],
                    'document_name': entity[10],
                    'document_date': entity[11]
                })
            
            # Display entities by category
            for category, items in categories.items():
                print(f"\n📁 {category} ({len(items)} items)")
                print("-" * 40)
                
                for item in items:
                    status = "✅" if item['verified'] else "⏳"
                    confidence = f"{item['confidence']:.2f}" if item['confidence'] else "N/A"
                    units = f" {item['units']}" if item['units'] else ""
                    
                    print(f"  {status} {item['name']}: {item['value']}{units}")
                    print(f"     🎯 Confidence: {confidence}")
                    if item['display_category']:
                        print(f"     📂 Display: {item['display_category']}")
            
            print(f"\n👤 Patient: {entities[0][8]} ({entities[0][9]})")
            print(f"📄 Document: {entities[0][10]}")
            print(f"📅 Date: {entities[0][11]}")
            print(f"\n🎉 Total entities: {len(entities)}")
            
        else:
            print("❌ No entities found")
            
    except Exception as e:
        print(f"❌ Error: {e}")
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    get_entities_360_view()
