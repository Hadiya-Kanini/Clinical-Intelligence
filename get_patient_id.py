import psycopg2
import uuid

# Database connection
conn = psycopg2.connect(
    host="localhost",
    database="ClinicalIntelligence",
    user="postgres",
    password="admin"
)

try:
    cursor = conn.cursor()
    
    # Get the test patient ID
    cursor.execute('SELECT "Id", "Mrn" FROM erd_patients WHERE "Mrn" = %s', ('MRN12345',))
    result = cursor.fetchone()
    
    if result:
        patient_id, mrn = result
        print(f"Found patient: MRN={mrn}, ID={patient_id}")
        
        # Update the test file with the correct patient ID
        with open('test_upload.py', 'r') as f:
            content = f.read()
        
        # Replace the hardcoded patient ID
        updated_content = content.replace(
            "'patientId': '12345'",
            f"'patientId': '{patient_id}'"
        )
        
        with open('test_upload.py', 'w') as f:
            f.write(updated_content)
            
        print(f"Updated test_upload.py with patient ID: {patient_id}")
    else:
        print("Test patient not found!")
        
finally:
    cursor.close()
    conn.close()
