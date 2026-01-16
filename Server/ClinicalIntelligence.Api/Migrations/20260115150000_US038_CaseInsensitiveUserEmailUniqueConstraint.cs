using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalIntelligence.Api.Migrations
{
    /// <summary>
    /// US_038 TASK_002: Enforces case-insensitive unique email constraint on users table.
    /// 
    /// Strategy: Uses PostgreSQL citext extension for case-insensitive text storage.
    /// This ensures that 'User@Example.com' and 'user@example.com' are treated as duplicates
    /// at the database level, providing defense-in-depth beyond application-level checks.
    /// 
    /// The unique index name 'ix_users_email' is preserved for compatibility with existing tests.
    /// </summary>
    public partial class US038_CaseInsensitiveUserEmailUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable citext extension for case-insensitive text comparison
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS citext;");

            // Drop the existing unique index on Email
            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");

            // Alter the Email column to use citext type for case-insensitive storage
            migrationBuilder.Sql(@"ALTER TABLE users ALTER COLUMN ""Email"" TYPE citext;");

            // Recreate the unique index on Email (now case-insensitive due to citext)
            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the unique index
            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");

            // Revert Email column back to varchar(255)
            migrationBuilder.Sql(@"ALTER TABLE users ALTER COLUMN ""Email"" TYPE character varying(255);");

            // Recreate the original unique index
            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "Email",
                unique: true);

            // Note: We don't drop the citext extension as other tables might use it
        }
    }
}
