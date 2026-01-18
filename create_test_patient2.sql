-- Create a test patient with specific ID for document upload testing
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
    '00000000-0000-0000-0000-000000000001'::uuid,
    'MRNUPLOAD001',
    'Test Patient For Upload',
    '1980-01-01',
    '123 Test Street',
    '555-123-4567',
    false,
    NULL,
    NOW(),
    NOW()
);
