using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalIntelligence.Api.Migrations
{
    /// <inheritdoc />
    public partial class Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fhir_resource_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InternalEntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InternalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    FhirResourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FhirResourceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FhirVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FhirResourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fhir_resource_links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Mrn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GivenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FamilyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Extensions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Extensions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_references_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "encounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Class = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Extensions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_encounters_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medication_statements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MedicationCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MedicationName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Dosage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Route = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Extensions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medication_statements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medication_statements_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicalStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CodeDisplay = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OnsetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AbatementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Severity = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Extensions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conditions_encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_conditions_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "observations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodeDisplay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Interpretation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ReferenceRangeLow = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReferenceRangeHigh = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Extensions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_observations_encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_observations_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "procedures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CodeDisplay = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PerformedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PerformedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BodySite = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Extensions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_procedures_encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_procedures_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_conditions_code",
                table: "conditions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "ix_conditions_encounter_id",
                table: "conditions",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "ix_conditions_patient_id",
                table: "conditions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "ix_document_references_is_deleted",
                table: "document_references",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "ix_document_references_patient_id",
                table: "document_references",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "ix_document_references_processing_status",
                table: "document_references",
                column: "ProcessingStatus");

            migrationBuilder.CreateIndex(
                name: "ix_encounters_patient_id",
                table: "encounters",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "ix_encounters_start_date",
                table: "encounters",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resource_links_fhir_resource",
                table: "fhir_resource_links",
                columns: new[] { "FhirResourceType", "FhirResourceId", "FhirVersion" });

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resource_links_internal_entity",
                table: "fhir_resource_links",
                columns: new[] { "InternalEntityType", "InternalEntityId" });

            migrationBuilder.CreateIndex(
                name: "ix_fhir_resource_links_source_system",
                table: "fhir_resource_links",
                column: "SourceSystem");

            migrationBuilder.CreateIndex(
                name: "ix_medication_statements_medication_code",
                table: "medication_statements",
                column: "MedicationCode");

            migrationBuilder.CreateIndex(
                name: "ix_medication_statements_patient_id",
                table: "medication_statements",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "ix_observations_code",
                table: "observations",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "ix_observations_effective_date",
                table: "observations",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "ix_observations_encounter_id",
                table: "observations",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "ix_observations_patient_id",
                table: "observations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "ix_patients_is_deleted",
                table: "patients",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "ix_patients_mrn",
                table: "patients",
                column: "Mrn",
                unique: true,
                filter: "\"Mrn\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_patients_name_dob",
                table: "patients",
                columns: new[] { "FamilyName", "GivenName", "DateOfBirth" });

            migrationBuilder.CreateIndex(
                name: "ix_procedures_code",
                table: "procedures",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "ix_procedures_encounter_id",
                table: "procedures",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "ix_procedures_patient_id",
                table: "procedures",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conditions");

            migrationBuilder.DropTable(
                name: "document_references");

            migrationBuilder.DropTable(
                name: "fhir_resource_links");

            migrationBuilder.DropTable(
                name: "medication_statements");

            migrationBuilder.DropTable(
                name: "observations");

            migrationBuilder.DropTable(
                name: "procedures");

            migrationBuilder.DropTable(
                name: "encounters");

            migrationBuilder.DropTable(
                name: "patients");
        }
    }
}
