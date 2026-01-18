using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalIntelligence.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentChunksHnswIndexAndUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_document_chunks_document_id_chunk_hash_unique",
                table: "document_chunks",
                columns: new[] { "DocumentId", "ChunkHash" },
                unique: true);

            migrationBuilder.Sql(@"
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_document_chunks_embedding_hnsw
                ON document_chunks
                USING hnsw (""Embedding"" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_document_chunks_embedding_hnsw;");

            migrationBuilder.DropIndex(
                name: "ix_document_chunks_document_id_chunk_hash_unique",
                table: "document_chunks");
        }
    }
}
