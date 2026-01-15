namespace ClinicalIntelligence.Api.Tests.TestData;

public static class JobPayloads
{
    public static string ValidJobPayload => @"{
  ""schema_version"": ""1.0"",
  ""job_id"": ""00000000-0000-0000-0000-000000000000"",
  ""document_id"": ""doc-123"",
  ""status"": ""pending"",
  ""payload"": {}
}";

    public static string ValidJobPayloadWithNullPayload => @"{
  ""schema_version"": ""1.0"",
  ""job_id"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""document_id"": ""doc-456"",
  ""status"": ""processing"",
  ""payload"": null
}";

    public static string MissingSchemaVersion => @"{
  ""job_id"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""document_id"": ""doc-123"",
  ""status"": ""pending""
}";

    public static string InvalidSchemaVersion => @"{
  ""schema_version"": ""2.0"",
  ""job_id"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""document_id"": ""doc-123"",
  ""status"": ""pending""
}";

    public static string MissingDocumentId => @"{
  ""schema_version"": ""1.0"",
  ""job_id"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""status"": ""pending""
}";

    public static string EmptyDocumentId => @"{
  ""schema_version"": ""1.0"",
  ""job_id"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""document_id"": """",
  ""status"": ""pending""
}";

    public static string InvalidStatus => @"{
  ""schema_version"": ""1.0"",
  ""job_id"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""document_id"": ""doc-123"",
  ""status"": ""invalid_status""
}";

    public static string MalformedJobId => @"{
  ""schema_version"": ""1.0"",
  ""job_id"": ""not-a-uuid"",
  ""document_id"": ""doc-123"",
  ""status"": ""pending""
}";

    public static string MissingJobId => @"{
  ""schema_version"": ""1.0"",
  ""document_id"": ""doc-123"",
  ""status"": ""pending""
}";

    public static string ValidJobPayloadCompleted => @"{
  ""schema_version"": ""1.0"",
  ""job_id"": ""a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"",
  ""document_id"": ""doc-789"",
  ""status"": ""completed"",
  ""payload"": {
    ""result"": ""success"",
    ""entities_extracted"": 42
  }
}";

    public static string ValidJobPayloadFailed => @"{
  ""schema_version"": ""1.0"",
  ""job_id"": ""b2c3d4e5-f6a7-4b5c-9d0e-1f2a3b4c5d6e"",
  ""document_id"": ""doc-999"",
  ""status"": ""failed"",
  ""payload"": {
    ""error"": ""Processing timeout""
  }
}";
}
