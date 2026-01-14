namespace ClinicalIntelligence.Api.Tests.Mocks;

public static class MockSchemaProvider
{
    public static string ValidJobSchema => @"{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""title"": ""Job"",
  ""description"": ""Schema for a job to be processed by the AI worker."",
  ""type"": ""object"",
  ""properties"": {
    ""schema_version"": {
      ""description"": ""The version of this job schema."",
      ""type"": ""string"",
      ""enum"": [""1.0""]
    },
    ""job_id"": {
      ""description"": ""Unique identifier for the job (UUID)."",
      ""type"": ""string"",
      ""pattern"": ""^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$""
    },
    ""document_id"": {
      ""description"": ""Identifier for the document to be processed."",
      ""type"": ""string"",
      ""minLength"": 1
    },
    ""status"": {
      ""description"": ""The current status of the job."",
      ""type"": ""string"",
      ""enum"": [""pending"", ""processing"", ""completed"", ""failed"", ""validation_failed""]
    },
    ""payload"": {
      ""description"": ""Arbitrary data for the worker to use."",
      ""type"": [""object"", ""null""]
    }
  },
  ""required"": [
    ""schema_version"",
    ""job_id"",
    ""document_id"",
    ""status""
  ]
}";

    public static string MalformedSchema => @"{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""title"": ""Job"",
  ""type"": ""object"",
  ""properties"": {
    ""schema_version"": {
      ""type"": ""string""
    }
  }
  // Missing closing brace
";
}
