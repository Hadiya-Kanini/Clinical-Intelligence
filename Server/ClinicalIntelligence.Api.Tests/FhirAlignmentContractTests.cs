using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Tests to ensure FHIR alignment documentation exists and remains consistent.
/// These tests validate that required contract artifacts are present and up to date.
/// </summary>
public sealed class FhirAlignmentContractTests
{
    private static readonly string ProjectRoot = GetProjectRoot();

    private static string GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);
        
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "contracts", "entities", "v1", "fhir_alignment.md")))
        {
            dir = dir.Parent;
        }
        
        return dir?.FullName ?? currentDir;
    }

    #region FHIR Alignment Document Tests

    [Fact]
    public void FhirAlignmentDocument_ShouldExist()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");

        // Act & Assert
        Assert.True(File.Exists(fhirAlignmentPath), 
            $"FHIR alignment document must exist at: {fhirAlignmentPath}");
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainPatientMapping()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - Patient mapping section exists
        Assert.Contains("### Patient", content);
        Assert.Contains("Patient.id", content);
        Assert.Contains("Patient.identifier", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainEncounterMapping()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - Encounter mapping section exists
        Assert.Contains("### Encounter", content);
        Assert.Contains("Encounter.subject", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainObservationMapping()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - Observation mapping section exists
        Assert.Contains("### Observation", content);
        Assert.Contains("Observation.subject", content);
        Assert.Contains("Observation.code", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainMedicationStatementMapping()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - MedicationStatement mapping section exists
        Assert.Contains("### MedicationStatement", content);
        Assert.Contains("MedicationStatement.subject", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainConditionMapping()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - Condition mapping section exists
        Assert.Contains("### Condition", content);
        Assert.Contains("Condition.subject", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainProcedureMapping()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - Procedure mapping section exists
        Assert.Contains("### Procedure", content);
        Assert.Contains("Procedure.subject", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainDocumentReferenceMapping()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - DocumentReference mapping section exists
        Assert.Contains("### DocumentReference", content);
        Assert.Contains("DocumentReference.subject", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainRelationshipMappings()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - Relationship mapping section exists
        Assert.Contains("Relationship Mapping", content);
        Assert.Contains("Reference(Patient)", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainExtensionStrategy()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - Extension strategy section exists
        Assert.Contains("Extension Strategy", content);
        Assert.Contains("unmappedData", content);
    }

    [Fact]
    public void FhirAlignmentDocument_ShouldContainVersionEvolutionStrategy()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - Version evolution section exists
        Assert.Contains("Version Evolution", content);
        Assert.Contains("R4", content);
    }

    #endregion

    #region Domain Model Migration Notes Tests

    [Fact]
    public void DomainModelMigrationNotes_ShouldExist()
    {
        // Arrange
        var migrationNotesPath = Path.Combine(ProjectRoot, "contracts", "migrations", "domain_model_v1.md");

        // Act & Assert
        Assert.True(File.Exists(migrationNotesPath), 
            $"Domain model migration notes must exist at: {migrationNotesPath}");
    }

    [Fact]
    public void DomainModelMigrationNotes_ShouldContainVersionInfo()
    {
        // Arrange
        var migrationNotesPath = Path.Combine(ProjectRoot, "contracts", "migrations", "domain_model_v1.md");
        
        if (!File.Exists(migrationNotesPath))
        {
            Assert.Fail($"Migration notes not found at: {migrationNotesPath}");
            return;
        }

        var content = File.ReadAllText(migrationNotesPath);

        // Assert - Version information exists
        Assert.Contains("1.0.0", content);
        Assert.Contains("Initial Release", content);
    }

    [Fact]
    public void DomainModelMigrationNotes_ShouldDocumentEntities()
    {
        // Arrange
        var migrationNotesPath = Path.Combine(ProjectRoot, "contracts", "migrations", "domain_model_v1.md");
        
        if (!File.Exists(migrationNotesPath))
        {
            Assert.Fail($"Migration notes not found at: {migrationNotesPath}");
            return;
        }

        var content = File.ReadAllText(migrationNotesPath);

        // Assert - All core entities are documented
        Assert.Contains("Patient", content);
        Assert.Contains("Encounter", content);
        Assert.Contains("Observation", content);
        Assert.Contains("MedicationStatement", content);
        Assert.Contains("Condition", content);
        Assert.Contains("Procedure", content);
        Assert.Contains("DocumentReference", content);
    }

    #endregion

    #region Entity README Tests

    [Fact]
    public void EntityReadme_ShouldExist()
    {
        // Arrange
        var readmePath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "README.md");

        // Act & Assert
        Assert.True(File.Exists(readmePath), 
            $"Entity README must exist at: {readmePath}");
    }

    [Fact]
    public void EntityReadme_ShouldReferenceFhirAlignment()
    {
        // Arrange
        var readmePath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "README.md");
        
        if (!File.Exists(readmePath))
        {
            Assert.Fail($"README not found at: {readmePath}");
            return;
        }

        var content = File.ReadAllText(readmePath);

        // Assert - FHIR alignment reference exists
        Assert.Contains("fhir_alignment.md", content);
        Assert.Contains("FHIR", content);
    }

    #endregion

    #region Phase 1 Categories Coverage Tests

    [Fact]
    public void FhirAlignmentDocument_ShouldCoverAllPhase1Categories()
    {
        // Arrange
        var fhirAlignmentPath = Path.Combine(ProjectRoot, "contracts", "entities", "v1", "fhir_alignment.md");
        
        if (!File.Exists(fhirAlignmentPath))
        {
            Assert.Fail($"FHIR alignment document not found at: {fhirAlignmentPath}");
            return;
        }

        var content = File.ReadAllText(fhirAlignmentPath);

        // Assert - All Phase 1 categories are covered
        var phase1Categories = new[]
        {
            "demographics",
            "encounters",
            "observations",
            "medications",
            "diagnoses",
            "procedures"
        };

        foreach (var category in phase1Categories)
        {
            Assert.True(content.ToLowerInvariant().Contains(category), 
                $"Phase 1 category '{category}' must be documented in FHIR alignment");
        }
    }

    #endregion
}
