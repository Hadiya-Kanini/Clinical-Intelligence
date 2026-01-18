-- Insert test patient
INSERT INTO patients ("Id", "Mrn", "Name", "DateOfBirth", "CreatedAt", "UpdatedAt")
VALUES ('00000000-0000-0000-0000-000000012345', 'TEST123', 'Test Patient', '1990-01-01', NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Insert test document
INSERT INTO documents ("Id", "OriginalName", "MimeType", "Status", "PatientId", "CreatedAt", "UpdatedAt", "IsDeleted")
VALUES ('5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'test.pdf', 'application/pdf', 'completed', '00000000-0000-0000-0000-000000012345', NOW(), NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Insert test entities
INSERT INTO "ExtractedEntities" ("Id", "PatientId", "DocumentId", "Category", "Name", "Value", "Confidence", "SourceText", "CreatedAt", "UpdatedAt")
VALUES 
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'patient_demographics', 'name', 'Test Patient', 0.95, 'Patient Name: Test Patient', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'patient_demographics', 'date_of_birth', '1990-01-01', 0.90, 'DOB: 1990-01-01', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'diagnoses', 'hypertension', 'Essential hypertension', 0.85, 'Diagnosis: Essential hypertension', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'medications', 'lisinopril', 'Lisinopril 10mg', 0.88, 'Medication: Lisinopril 10mg daily', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'lab_results', 'blood_pressure', '140/90 mmHg', 0.92, 'BP: 140/90 mmHg', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'allergies', 'penicillin', 'Penicillin allergy', 0.94, 'Allergy: Penicillin', NOW(), NOW()),
    (gen_random_uuid(), '00000000-0000-0000-0000-000000012345', '5cf84765-f8bf-41b9-8a95-cc8b790fa495', 'vital_signs', 'heart_rate', '72 bpm', 0.89, 'Heart Rate: 72 bpm', NOW(), NOW());
