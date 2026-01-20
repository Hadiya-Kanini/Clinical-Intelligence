using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalIntelligence.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateExtractedEntitiesV2Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "extracted_entities_v2",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityData = table.Column<string>(type: "jsonb", nullable: false),
                    SourceReference = table.Column<string>(type: "text", nullable: true),
                    ExtractedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extracted_entities_v2", x => x.Id);
                    table.ForeignKey(
                        name: "fk_extracted_entities_v2_documents_document_id",
                        column: x => x.DocumentId,
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_extracted_entities_v2_patients_patient_id",
                        column: x => x.PatientId,
                        principalTable: "erd_patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_extracted_entities_v2_users_verified_by_user_id",
                        column: x => x.VerifiedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_extracted_entities_v2_document_id",
                table: "extracted_entities_v2",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "ix_extracted_entities_v2_extracted_at",
                table: "extracted_entities_v2",
                column: "ExtractedAt");

            migrationBuilder.CreateIndex(
                name: "ix_extracted_entities_v2_patient_id",
                table: "extracted_entities_v2",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "ix_extracted_entities_v2_patient_section",
                table: "extracted_entities_v2",
                columns: new[] { "PatientId", "SectionName" });

            migrationBuilder.CreateIndex(
                name: "ix_extracted_entities_v2_section_name",
                table: "extracted_entities_v2",
                column: "SectionName");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_entities_v2_VerifiedByUserId",
                table: "extracted_entities_v2",
                column: "VerifiedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "extracted_entities_v2");
        }
    }
}
