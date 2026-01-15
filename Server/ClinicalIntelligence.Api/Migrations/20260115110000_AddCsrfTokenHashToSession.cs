using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalIntelligence.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCsrfTokenHashToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CsrfTokenHash",
                table: "sessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CsrfTokenHash",
                table: "sessions");
        }
    }
}
