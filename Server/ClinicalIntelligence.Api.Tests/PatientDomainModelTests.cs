using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Tests for EF Core model relationships and constraints.
/// Validates patient-centric schema invariants to ensure future schema changes
/// do not accidentally break integration readiness.
/// </summary>
public sealed class PatientDomainModelTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public PatientDomainModelTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region Patient Entity Tests

    [Fact]
    public async Task Patient_Create_ShouldPersistSuccessfully()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN001",
            GivenName = "John",
            FamilyName = "Doe",
            DateOfBirth = new DateOnly(1980, 1, 15),
            Gender = "male",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedPatient = await _dbContext.Patients.FindAsync(patient.Id);
        Assert.NotNull(savedPatient);
        Assert.Equal("MRN001", savedPatient.Mrn);
        Assert.Equal("John", savedPatient.GivenName);
        Assert.Equal("Doe", savedPatient.FamilyName);
    }

    [Fact]
    public async Task Patient_SoftDelete_ShouldFilterFromQueries()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN002",
            GivenName = "Jane",
            FamilyName = "Doe",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Act - Query without IgnoreQueryFilters should not return soft-deleted patient
        var patients = await _dbContext.Patients.ToListAsync();

        // Assert
        Assert.DoesNotContain(patients, p => p.Id == patient.Id);
    }

    [Fact]
    public async Task Patient_SoftDelete_ShouldBeAccessibleWithIgnoreQueryFilters()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN003",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Act
        var allPatients = await _dbContext.Patients
            .IgnoreQueryFilters()
            .ToListAsync();

        // Assert
        Assert.Contains(allPatients, p => p.Id == patient.Id);
    }

    #endregion

    #region Patient-Encounter Relationship Tests

    [Fact]
    public async Task Patient_Encounters_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN004",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var encounter = new Encounter
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "finished",
            Class = "AMB",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Encounters.Add(encounter);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedPatient = await _dbContext.Patients
            .Include(p => p.Encounters)
            .FirstOrDefaultAsync(p => p.Id == patient.Id);

        Assert.NotNull(loadedPatient);
        Assert.Single(loadedPatient.Encounters);
        Assert.Equal(encounter.Id, loadedPatient.Encounters.First().Id);
    }

    [Fact]
    public async Task Patient_Delete_ShouldCascadeToEncounters()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN005",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var encounter = new Encounter
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "finished",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Patients.Add(patient);
        _dbContext.Encounters.Add(encounter);
        await _dbContext.SaveChangesAsync();

        // Act
        _dbContext.Patients.Remove(patient);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedEncounter = await _dbContext.Encounters.FindAsync(encounter.Id);
        Assert.Null(deletedEncounter);
    }

    #endregion

    #region Patient-Observation Relationship Tests

    [Fact]
    public async Task Patient_Observations_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN006",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var observation = new Observation
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "final",
            Code = "8867-4",
            CodeDisplay = "Heart rate",
            Value = "72",
            Unit = "beats/min",
            Category = "vital-signs",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Observations.Add(observation);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedPatient = await _dbContext.Patients
            .Include(p => p.Observations)
            .FirstOrDefaultAsync(p => p.Id == patient.Id);

        Assert.NotNull(loadedPatient);
        Assert.Single(loadedPatient.Observations);
        Assert.Equal("8867-4", loadedPatient.Observations.First().Code);
    }

    [Fact]
    public async Task Observation_EncounterContext_IsOptional()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN007",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var observationWithoutEncounter = new Observation
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            EncounterId = null,
            Status = "final",
            Code = "2339-0",
            CodeDisplay = "Glucose",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Observations.Add(observationWithoutEncounter);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedObservation = await _dbContext.Observations.FindAsync(observationWithoutEncounter.Id);
        Assert.NotNull(savedObservation);
        Assert.Null(savedObservation.EncounterId);
    }

    #endregion

    #region Patient-MedicationStatement Relationship Tests

    [Fact]
    public async Task Patient_MedicationStatements_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN008",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var medication = new MedicationStatement
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "active",
            MedicationName = "Aspirin",
            MedicationCode = "1191",
            Dosage = "81mg",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.MedicationStatements.Add(medication);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedPatient = await _dbContext.Patients
            .Include(p => p.MedicationStatements)
            .FirstOrDefaultAsync(p => p.Id == patient.Id);

        Assert.NotNull(loadedPatient);
        Assert.Single(loadedPatient.MedicationStatements);
        Assert.Equal("Aspirin", loadedPatient.MedicationStatements.First().MedicationName);
    }

    #endregion

    #region Patient-Condition Relationship Tests

    [Fact]
    public async Task Patient_Conditions_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN009",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var condition = new Condition
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            ClinicalStatus = "active",
            VerificationStatus = "confirmed",
            Code = "E11.9",
            CodeDisplay = "Type 2 diabetes mellitus without complications",
            Category = "encounter-diagnosis",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Conditions.Add(condition);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedPatient = await _dbContext.Patients
            .Include(p => p.Conditions)
            .FirstOrDefaultAsync(p => p.Id == patient.Id);

        Assert.NotNull(loadedPatient);
        Assert.Single(loadedPatient.Conditions);
        Assert.Equal("E11.9", loadedPatient.Conditions.First().Code);
    }

    #endregion

    #region Patient-Procedure Relationship Tests

    [Fact]
    public async Task Patient_Procedures_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN010",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var procedure = new Procedure
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "completed",
            Code = "99213",
            CodeDisplay = "Office visit, established patient",
            PerformedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Procedures.Add(procedure);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedPatient = await _dbContext.Patients
            .Include(p => p.Procedures)
            .FirstOrDefaultAsync(p => p.Id == patient.Id);

        Assert.NotNull(loadedPatient);
        Assert.Single(loadedPatient.Procedures);
        Assert.Equal("99213", loadedPatient.Procedures.First().Code);
    }

    #endregion

    #region Patient-DocumentReference Relationship Tests

    [Fact]
    public async Task Patient_DocumentReferences_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN011",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var document = new DocumentReference
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "current",
            OriginalName = "lab_report.pdf",
            ContentType = "application/pdf",
            StoragePath = "/documents/lab_report.pdf",
            SizeBytes = 1024,
            ProcessingStatus = "completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.DocumentReferences.Add(document);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedPatient = await _dbContext.Patients
            .Include(p => p.DocumentReferences)
            .FirstOrDefaultAsync(p => p.Id == patient.Id);

        Assert.NotNull(loadedPatient);
        Assert.Single(loadedPatient.DocumentReferences);
        Assert.Equal("lab_report.pdf", loadedPatient.DocumentReferences.First().OriginalName);
    }

    #endregion

    #region Encounter-Context Relationship Tests

    [Fact]
    public async Task Encounter_Observations_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN012",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var encounter = new Encounter
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "finished",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var observation = new Observation
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            EncounterId = encounter.Id,
            Status = "final",
            Code = "8480-6",
            CodeDisplay = "Systolic blood pressure",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Encounters.Add(encounter);
        _dbContext.Observations.Add(observation);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedEncounter = await _dbContext.Encounters
            .Include(e => e.Observations)
            .FirstOrDefaultAsync(e => e.Id == encounter.Id);

        Assert.NotNull(loadedEncounter);
        Assert.Single(loadedEncounter.Observations);
    }

    [Fact]
    public async Task Encounter_Conditions_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN013",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var encounter = new Encounter
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "finished",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var condition = new Condition
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            EncounterId = encounter.Id,
            CodeDisplay = "Acute bronchitis",
            Category = "encounter-diagnosis",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Encounters.Add(encounter);
        _dbContext.Conditions.Add(condition);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedEncounter = await _dbContext.Encounters
            .Include(e => e.Conditions)
            .FirstOrDefaultAsync(e => e.Id == encounter.Id);

        Assert.NotNull(loadedEncounter);
        Assert.Single(loadedEncounter.Conditions);
    }

    [Fact]
    public async Task Encounter_Procedures_RelationshipExists()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN014",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var encounter = new Encounter
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "finished",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var procedure = new Procedure
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            EncounterId = encounter.Id,
            Status = "completed",
            CodeDisplay = "Blood draw",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Encounters.Add(encounter);
        _dbContext.Procedures.Add(procedure);
        await _dbContext.SaveChangesAsync();

        // Assert
        var loadedEncounter = await _dbContext.Encounters
            .Include(e => e.Procedures)
            .FirstOrDefaultAsync(e => e.Id == encounter.Id);

        Assert.NotNull(loadedEncounter);
        Assert.Single(loadedEncounter.Procedures);
    }

    #endregion

    #region FhirResourceLink Tests

    [Fact]
    public async Task FhirResourceLink_Create_ShouldPersistSuccessfully()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var link = new FhirResourceLink
        {
            Id = Guid.NewGuid(),
            InternalEntityType = "Patient",
            InternalEntityId = patientId,
            FhirResourceType = "Patient",
            FhirResourceId = "external-patient-123",
            FhirVersion = "R4",
            SourceSystem = "ExternalEHR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.FhirResourceLinks.Add(link);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedLink = await _dbContext.FhirResourceLinks.FindAsync(link.Id);
        Assert.NotNull(savedLink);
        Assert.Equal("Patient", savedLink.InternalEntityType);
        Assert.Equal(patientId, savedLink.InternalEntityId);
        Assert.Equal("R4", savedLink.FhirVersion);
    }

    [Fact]
    public async Task FhirResourceLink_MultipleVersions_ShouldCoexist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var r4Link = new FhirResourceLink
        {
            Id = Guid.NewGuid(),
            InternalEntityType = "Patient",
            InternalEntityId = patientId,
            FhirResourceType = "Patient",
            FhirResourceId = "r4-patient-123",
            FhirVersion = "R4",
            SourceSystem = "SystemA",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var r5Link = new FhirResourceLink
        {
            Id = Guid.NewGuid(),
            InternalEntityType = "Patient",
            InternalEntityId = patientId,
            FhirResourceType = "Patient",
            FhirResourceId = "r5-patient-456",
            FhirVersion = "R5",
            SourceSystem = "SystemB",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.FhirResourceLinks.AddRange(r4Link, r5Link);
        await _dbContext.SaveChangesAsync();

        // Assert
        var links = await _dbContext.FhirResourceLinks
            .Where(l => l.InternalEntityId == patientId)
            .ToListAsync();

        Assert.Equal(2, links.Count);
        Assert.Contains(links, l => l.FhirVersion == "R4");
        Assert.Contains(links, l => l.FhirVersion == "R5");
    }

    #endregion

    #region Extension Field Tests

    [Fact]
    public async Task Patient_Extensions_ShouldStoreJsonData()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN015",
            Extensions = "{\"extensions\":[{\"url\":\"http://example.org/custom\",\"valueString\":\"test\"}]}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedPatient = await _dbContext.Patients.FindAsync(patient.Id);
        Assert.NotNull(savedPatient);
        Assert.NotNull(savedPatient.Extensions);
        Assert.Contains("custom", savedPatient.Extensions);
    }

    [Fact]
    public async Task Observation_Extensions_ShouldStoreJsonData()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Mrn = "MRN016",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var observation = new Observation
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Status = "final",
            Code = "test-code",
            Extensions = "{\"unmappedData\":{\"customField\":\"customValue\"}}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        _dbContext.Observations.Add(observation);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedObservation = await _dbContext.Observations.FindAsync(observation.Id);
        Assert.NotNull(savedObservation);
        Assert.NotNull(savedObservation.Extensions);
        Assert.Contains("customField", savedObservation.Extensions);
    }

    #endregion

    #region Phase 1 Standalone Usage Tests

    [Fact]
    public async Task Patient_CanBeStoredWithoutExternalIdentifiers()
    {
        // Arrange - Patient with only internal data, no FHIR links
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            GivenName = "Standalone",
            FamilyName = "Patient",
            DateOfBirth = new DateOnly(1990, 5, 20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedPatient = await _dbContext.Patients.FindAsync(patient.Id);
        Assert.NotNull(savedPatient);
        Assert.Null(savedPatient.Mrn);

        var fhirLinks = await _dbContext.FhirResourceLinks
            .Where(l => l.InternalEntityId == patient.Id)
            .ToListAsync();
        Assert.Empty(fhirLinks);
    }

    #endregion
}
