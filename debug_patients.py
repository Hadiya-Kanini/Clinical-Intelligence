#!/usr/bin/env python3
"""
Debug patients and document linking issue
"""

import psycopg2
import requests

def debug_patients_and_documents():
    """Debug why uploaded documents aren't showing in patients section"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Check ERD patients
        cursor.execute('SELECT COUNT(*) FROM erd_patients WHERE NOT "IsDeleted"')
        patient_count = cursor.fetchone()[0]
        print(f"👤 ERD Patients (not deleted): {patient_count}")
        
        # Check documents
        cursor.execute('SELECT COUNT(*) FROM documents WHERE NOT "IsDeleted"')
        doc_count = cursor.fetchone()[0]
        print(f"📄 Documents (not deleted): {doc_count}")
        
        # Check documents with PatientId
        cursor.execute('SELECT COUNT(*) FROM documents WHERE "PatientId" IS NOT NULL AND NOT "IsDeleted"')
        linked_docs = cursor.fetchone()[0]
        print(f"🔗 Documents linked to patients: {linked_docs}")
        
        # Check documents without PatientId
        cursor.execute('SELECT COUNT(*) FROM documents WHERE "PatientId" IS NULL AND NOT "IsDeleted"')
        unlinked_docs = cursor.fetchone()[0]
        print(f"❌ Documents NOT linked to patients: {unlinked_docs}")
        
        if patient_count > 0:
            # Show sample patients with document counts
            cursor.execute("""
                SELECT p."Id", p."Name", p."Mrn", 
                       COUNT(d."Id") as doc_count,
                       MAX(d."UploadedAt") as last_upload
                FROM erd_patients p
                LEFT JOIN documents d ON d."PatientId" = p."Id" AND NOT d."IsDeleted"
                WHERE NOT p."IsDeleted"
                GROUP BY p."Id", p."Name", p."Mrn"
                ORDER BY doc_count DESC, p."Name"
                LIMIT 5
            """)
            
            patients = cursor.fetchall()
            print(f"\n📊 Sample patients with document counts:")
            for patient in patients:
                print(f"  • {patient[1]} ({patient[2]}) - {patient[3]} docs, last: {patient[4]}")
        
        if unlinked_docs > 0:
            # Show sample unlinked documents
            cursor.execute("""
                SELECT d.Id, d.OriginalName, d.UploadedAt, d.Status
                FROM documents d
                WHERE d.PatientId IS NULL AND NOT d.IsDeleted
                ORDER BY d.UploadedAt DESC
                LIMIT 5
            """)
            
            docs = cursor.fetchall()
            print(f"\n📄 Sample unlinked documents:")
            for doc in docs:
                print(f"  • {doc[1]} - Status: {doc[3]}, Uploaded: {doc[2]}")
        
        conn.close()
        
        # Test the API endpoint
        print(f"\n🔍 Testing patients API endpoint...")
        try:
            response = requests.get("http://localhost:5000/api/v1/patients/dashboard", timeout=5)
            if response.status_code == 200:
                data = response.json()
                api_patients = data.get("patients", [])
                api_count = data.get("totalCount", 0)
                print(f"✅ API returned {api_count} patients")
                
                if api_patients:
                    print(f"📊 API Sample patients:")
                    for patient in api_patients[:3]:
                        print(f"  • {patient.get('name')} ({patient.get('mrn')}) - {patient.get('documentCount')} docs")
            else:
                print(f"❌ API failed: {response.status_code}")
        except Exception as e:
            print(f"❌ API error: {e}")
        
    except Exception as e:
        print(f"❌ Database error: {e}")

if __name__ == "__main__":
    debug_patients_and_documents()
