#!/usr/bin/env python3
"""
Create test entities for 360 view testing
"""

import requests
import json
import uuid
from datetime import datetime

BASE_URL = "http://localhost:5000"

def create_test_entities():
    """Create some test entities directly via API if possible, or show manual SQL"""
    
    # Login
    login_data = {
        "email": "test@example.com",
        "password": "Test123456"
    }
    
    response = requests.post(f"{BASE_URL}/api/v1/auth/login", json=login_data)
    if response.status_code != 200:
        print("❌ Login failed")
        return
    
    token = response.json().get("access_token")
    headers = {"Authorization": f"Bearer {token}"}
    
    print("🔍 Checking for entity creation endpoints...")
    
    # Try to find entity creation endpoints
    endpoints_to_try = [
        "/api/v1/entities/create",
        "/api/v1/entities/add",
        "/api/v1/entities"
    ]
    
    for endpoint in endpoints_to_try:
        try:
            response = requests.post(f"{BASE_URL}{endpoint}", headers=headers, json={})
            print(f"  POST {endpoint}: {response.status_code}")
        except Exception as e:
            print(f"  POST {endpoint}: Error - {e}")
    
    print("\n📝 Manual SQL to create test entities:")
    print("""
-- Insert test patient
INSERT INTO patients ("Id", "Mrn", "Name", "DateOfBirth", "CreatedAt", "UpdatedAt")
VALUES ('00000000-0000-0000-0000-000000012345', 'TEST123', 'Test Patient', '1990-01-01', NOW(), NOW());

-- Insert test document
INSERT INTO documents ("Id", "OriginalName", "MimeType", "Status", "PatientId", "CreatedAt", "UpdatedAt")
VALUES ('5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'test.pdf', 'application/pdf', 'completed', '00000000-0000-0000-0000-000000012345', NOW(), NOW());

-- Insert test entities
INSERT INTO "ExtractedEntities" ("Id", "PatientId", "DocumentId", "Category", "Name", "Value", "Confidence", "SourceText", "CreatedAt", "UpdatedAt")
VALUES 
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'patient_demographics', 'name', 'Test Patient', 0.95, 'Patient Name: Test Patient', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'patient_demographics', 'date_of_birth', '1990-01-01', 0.90, 'DOB: 1990-01-01', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'diagnoses', 'hypertension', 'Essential hypertension', 0.85, 'Diagnosis: Essential hypertension', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'medications', 'lisinopril', 'Lisinopril 10mg', 0.88, 'Medication: Lisinopril 10mg daily', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'lab_results', 'blood_pressure', '140/90 mmHg', 0.92, 'BP: 140/90 mmHg', NOW(), NOW());
""")

if __name__ == "__main__":
    create_test_entities()
