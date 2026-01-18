-- Query to retrieve stored entities for 360° view
-- This gets all entities for a specific document with patient info

SELECT 
    e."Id",
    e."EntityName",
    e."EntityValue", 
    e."EntityGroupName",
    e."Rationale",
    e."SourceText",
    e."DocumentLocation",
    e."DisplayCategory",
    e."Confidence",
    e."CreatedAt",
    d."OriginalName" as "DocumentName",
    d."UploadedAt" as "DocumentDate",
    p."Name" as "PatientName",
    p."Mrn" as "PatientMRN"
FROM extracted_entities e
JOIN documents d ON e."DocumentId" = d."Id"
JOIN patients p ON e."PatientId" = p."Id"
WHERE d."IsDeleted" = false 
AND p."IsDeleted" = false
ORDER BY e."EntityGroupName", e."EntityName";
