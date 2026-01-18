-- Create a test patient for document upload testing
INSERT INTO erd_patients (
    "Id",
    "Mrn",
    "Name",
    "Dob",
    "Address",
    "Contact",
    "IsDeleted",
    "DeletedAt",
    "CreatedAt",
    "UpdatedAt"
)
VALUES
(
    gen_random_uuid(),
    'MRN12345',
    'Test Patient',
    '1980-01-01',
    '123 Test Street',
    '555-123-4567',
    false,
    NULL,
    NOW(),
    NOW()
)
ON CONFLICT ("Mrn") DO NOTHING;

-- Get the patient ID to use in tests
SELECT "Id", "Mrn", "Name" FROM erd_patients WHERE "Mrn" = 'MRN12345';
