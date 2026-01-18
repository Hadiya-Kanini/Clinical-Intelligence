-- Query to get entities from the most recent processed document
-- Perfect for testing the 360° view

WITH latest_document AS (
    SELECT d."Id" 
    FROM documents d
    WHERE d."IsDeleted" = false
    ORDER BY d."UploadedAt" DESC
    LIMIT 1
)

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
WHERE d."Id" = (SELECT "Id" FROM latest_document)
ORDER BY e."Category", e."Name";
