using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace ClinicalIntelligence.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPgvectorExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "document_chunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Page = table.Column<int>(type: "integer", nullable: true),
                    Section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Coordinates = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TextContent = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(768)", nullable: true),
                    TokenCount = table.Column<int>(type: "integer", nullable: true),
                    ChunkHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_chunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_chunks_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vector_query_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueryText = table.Column<string>(type: "text", nullable: false),
                    ResultCount = table.Column<int>(type: "integer", nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    QueryHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vector_query_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vector_query_logs_erd_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "erd_patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_vector_query_logs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "entity_citations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentChunkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Page = table.Column<int>(type: "integer", nullable: true),
                    Section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Coordinates = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CitedText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_citations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entity_citations_document_chunks_DocumentChunkId",
                        column: x => x.DocumentChunkId,
                        principalTable: "document_chunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entity_citations_extracted_entities_ExtractedEntityId",
                        column: x => x.ExtractedEntityId,
                        principalTable: "extracted_entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_document_chunks_chunk_hash",
                table: "document_chunks",
                column: "ChunkHash");

            migrationBuilder.CreateIndex(
                name: "ix_document_chunks_document_id",
                table: "document_chunks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "ix_entity_citations_document_chunk_id",
                table: "entity_citations",
                column: "DocumentChunkId");

            migrationBuilder.CreateIndex(
                name: "ix_entity_citations_extracted_entity_id",
                table: "entity_citations",
                column: "ExtractedEntityId");

            migrationBuilder.CreateIndex(
                name: "ix_vector_query_logs_patient_id",
                table: "vector_query_logs",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "ix_vector_query_logs_query_hash",
                table: "vector_query_logs",
                column: "QueryHash");

            migrationBuilder.CreateIndex(
                name: "ix_vector_query_logs_timestamp",
                table: "vector_query_logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_vector_query_logs_user_id",
                table: "vector_query_logs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entity_citations");

            migrationBuilder.DropTable(
                name: "vector_query_logs");

            migrationBuilder.DropTable(
                name: "document_chunks");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
