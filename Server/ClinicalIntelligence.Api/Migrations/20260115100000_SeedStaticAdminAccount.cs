using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;
using ClinicalIntelligence.Api.Services.Auth;
using ClinicalIntelligence.Api.Validation;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicalIntelligence.Api.Migrations
{
    /// <summary>
    /// Migration to seed a static admin account using environment variables.
    /// Validates email format and password complexity per FR-009c.
    /// Uses bcrypt (12 rounds) for password hashing and idempotent INSERT.
    /// </summary>
    public partial class SeedStaticAdminAccount : Migration
    {
        private const int BcryptWorkFactor = 12;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add IsStaticAdmin column to users table
            migrationBuilder.AddColumn<bool>(
                name: "IsStaticAdmin",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Add index for IsStaticAdmin
            migrationBuilder.CreateIndex(
                name: "ix_users_is_static_admin",
                table: "users",
                column: "IsStaticAdmin");

            // Read environment variables
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            // Validate required environment variables
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Required environment variables ADMIN_EMAIL and ADMIN_PASSWORD must be set");
            }

            // Normalize email
            adminEmail = adminEmail.Trim().ToLowerInvariant();

            // Validate email format using centralized RFC 5322 validator
            if (!EmailValidation.IsValid(adminEmail))
            {
                throw new InvalidOperationException(
                    "ADMIN_EMAIL must be a valid email address format");
            }

            // Validate password complexity per FR-009c using centralized policy
            PasswordPolicy.ValidateOrThrow(adminPassword, "ADMIN_PASSWORD");

            // Hash password using bcrypt with 12 rounds
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, BcryptWorkFactor);

            // Generate a stable GUID for the static admin
            var adminId = Guid.NewGuid();

            // Insert static admin idempotently using ON CONFLICT DO NOTHING
            migrationBuilder.Sql($@"
                INSERT INTO users (
                    ""Id"",
                    ""Email"",
                    ""PasswordHash"",
                    ""Name"",
                    ""Role"",
                    ""Status"",
                    ""IsStaticAdmin"",
                    ""FailedLoginAttempts"",
                    ""IsDeleted"",
                    ""CreatedAt"",
                    ""UpdatedAt""
                )
                VALUES (
                    '{adminId}',
                    '{adminEmail.Replace("'", "''")}',
                    '{passwordHash.Replace("'", "''")}',
                    'Static Admin',
                    'Admin',
                    'Active',
                    true,
                    0,
                    false,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                )
                ON CONFLICT (""Email"") DO NOTHING;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove static admin user
            migrationBuilder.Sql(@"
                DELETE FROM users WHERE ""IsStaticAdmin"" = true;
            ");

            // Remove index
            migrationBuilder.DropIndex(
                name: "ix_users_is_static_admin",
                table: "users");

            // Remove IsStaticAdmin column
            migrationBuilder.DropColumn(
                name: "IsStaticAdmin",
                table: "users");
        }

    }
}
