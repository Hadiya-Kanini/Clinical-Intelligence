#!/usr/bin/env python3
"""
Direct database insert test to bypass EF Core issues
"""

import psycopg2
import json

def direct_db_insert():
    """Test direct database insert to verify table works"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Use the ERD patient and linked document
        patient_id = "dca8e532-3276-419c-8be0-025e6c4dd105"
        document_id = "db56280e-7f83-4159-a90e-347ea290e2f3"
        
        print(f"👤 ERD Patient ID: {patient_id}")
        print(f"📄 Document ID: {document_id}")
        
        # Direct insert into extracted_entities
        entities = [
            {
                "category": "document_metadata",
                "name": "document_type",
                "value": "medical_report",
                "display_category": "Document Metadata"
            },
            {
                "category": "patient_demographics", 
                "name": "patient_name",
                "value": "Test Patient",
                "display_category": "Patient Demographics"
            },
            {
                "category": "diagnoses",
                "name": "hypertension",
                "value": "Essential hypertension",
                "display_category": "Diagnoses"
            }
        ]
        
        for entity in entities:
            cursor.execute("""
                INSERT INTO extracted_entities (
                    "Id", "PatientId", "DocumentId", "Category", "DisplayCategory", 
                    "Name", "Value", "IsVerified"
                ) VALUES (
                    gen_random_uuid(), 
                    %s,
                    %s,
                    %s,
                    %s,
                    %s,
                    %s,
                    false
                )
            """, (
                patient_id, 
                document_id,
                entity["category"],
                entity["display_category"],
                entity["name"],
                entity["value"]
            ))
        
        conn.commit()
        print(f"✅ Successfully inserted {len(entities)} entities directly")
        
        # Verify the insert
        cursor.execute("""
            SELECT COUNT(*) 
            FROM extracted_entities 
            WHERE "DocumentId" = %s
        """, (document_id,))
        
        count = cursor.fetchone()[0]
        print(f"📊 Total entities for document: {count}")
        
        conn.close()
        return True
        
    except Exception as e:
        print(f"❌ Direct insert failed: {e}")
        return False

def test_360_view_after_direct_insert():
    """Test 360 view after direct insert"""
    import requests
    
    BASE_URL = "http://localhost:5000"
    document_id = "db56280e-7f83-4159-a90e-347ea290e2f3"
    
    print("\n🔍 Testing 360 view after direct insert...")
    
    # Login
    login_data = {"email": "test@example.com", "password": "Test123456"}
    login_response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    
    if login_response.status_code == 200:
        token = login_response.json().get("access_token")
        headers = {'Authorization': f'Bearer {token}'}
        
        view_response = requests.get(f"{BASE_URL}/api/v1/entities/360-view?documentId={document_id}", 
                                   headers=headers)
        
        if view_response.status_code == 200:
            entities = view_response.json().get("entities", [])
            print(f"✅ 360 view found {len(entities)} entities")
            
            # Show entity categories
            categories = {}
            for entity in entities:
                cat = entity.get("entity_group_name", "unknown")
                if cat not in categories:
                    categories[cat] = 0
                categories[cat] += 1
            
            print("📋 Entity Categories:")
            for cat, count in categories.items():
                print(f"  • {cat}: {count} entities")
                
            print("\n🎉 ENTITY STORAGE ISSUE IDENTIFIED!")
            print("✅ Direct database insert works")
            print("✅ 360 view API works")
            print("❌ EF Core entity writer has issues")
            return True
        else:
            print(f"❌ 360 view failed: {view_response.status_code}")
            return False
    else:
        print("❌ Login failed")
        return False

if __name__ == "__main__":
    if direct_db_insert():
        test_360_view_after_direct_insert()
