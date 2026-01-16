using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalIntelligence.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadLetterJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dead_letter_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessingJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalMessage = table.Column<string>(type: "jsonb", nullable: false),
                    MessageSchemaVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ErrorDetails = table.Column<string>(type: "jsonb", nullable: true),
                    RetryHistory = table.Column<string>(type: "jsonb", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    DeadLetterReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DeadLetteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActionByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReplayAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastReplayError = table.Column<string>(type: "text", nullable: true),
                    ReplayedJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dead_letter_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dead_letter_jobs_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dead_letter_jobs_processing_jobs_ProcessingJobId",
                        column: x => x.ProcessingJobId,
                        principalTable: "processing_jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_dead_letter_jobs_users_LastActionByUserId",
                        column: x => x.LastActionByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dead_letter_jobs_dead_lettered_at",
                table: "dead_letter_jobs",
                column: "DeadLetteredAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_dead_letter_jobs_document_id",
                table: "dead_letter_jobs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_dead_letter_jobs_LastActionByUserId",
                table: "dead_letter_jobs",
                column: "LastActionByUserId");

            migrationBuilder.CreateIndex(
                name: "ix_dead_letter_jobs_processing_job_id",
                table: "dead_letter_jobs",
                column: "ProcessingJobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dead_letter_jobs_status",
                table: "dead_letter_jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "ix_dead_letter_jobs_status_dead_lettered_at",
                table: "dead_letter_jobs",
                columns: new[] { "Status", "DeadLetteredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dead_letter_jobs");
        }
    }
}
