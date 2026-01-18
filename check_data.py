#!/usr/bin/env python3
"""
Check if we have any patients or documents
"""

import psycopg2

def check_data():
    """Check if we have patients and documents"""
    try:
        conn = psycopg2.connect(
            host="localhost",
            database="ClinicalIntelligence",
            user="postgres",
            password="admin"
        )
        
        cursor = conn.cursor()
        
        # Check patients
        cursor.execute("SELECT COUNT(*) FROM patients")
        patient_count = cursor.fetchone()[0]
        
        # Check documents
        cursor.execute("SELECT COUNT(*) FROM documents")
        doc_count = cursor.fetchone()[0]
        
        print(f"👤 Patients: {patient_count}")
        print(f"📄 Documents: {doc_count}")
        
        if patient_count > 0:
            cursor.execute("SELECT \"Id\", \"GivenName\", \"FamilyName\" FROM patients LIMIT 1")
            patient = cursor.fetchone()
            print(f"  Sample patient: {patient[0]} - {patient[1]} {patient[2]}")
        
        if doc_count > 0:
            cursor.execute("SELECT \"Id\", \"OriginalName\" FROM documents LIMIT 1")
            doc = cursor.fetchone()
            print(f"  Sample document: {doc[0]} - {doc[1]}")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    check_data()
