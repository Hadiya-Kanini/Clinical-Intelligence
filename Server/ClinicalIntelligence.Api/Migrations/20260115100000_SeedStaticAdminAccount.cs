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
        private const int MinimumBcryptWorkFactor = 12;
        private const int MaximumBcryptWorkFactor = 31;

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

            // Get configurable bcrypt work factor from environment (default 12, minimum 12)
            var workFactor = GetConfiguredWorkFactor();

            // Hash password using bcrypt with configured work factor (>=12)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor);

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

        /// <summary>
        /// Gets the bcrypt work factor from environment configuration.
        /// Enforces minimum of 12 per OWASP recommendations.
        /// </summary>
        private static int GetConfiguredWorkFactor()
        {
            var workFactorStr = Environment.GetEnvironmentVariable("BCRYPT_WORK_FACTOR");

            if (string.IsNullOrWhiteSpace(workFactorStr))
            {
                return MinimumBcryptWorkFactor;
            }

            if (!int.TryParse(workFactorStr, out var workFactor))
            {
                throw new InvalidOperationException(
                    $"BCRYPT_WORK_FACTOR must be a valid integer. Current value: '{workFactorStr}'");
            }

            if (workFactor < MinimumBcryptWorkFactor)
            {
                throw new InvalidOperationException(
                    $"BCRYPT_WORK_FACTOR must be at least {MinimumBcryptWorkFactor} for secure password hashing. Current value: {workFactor}");
            }

            if (workFactor > MaximumBcryptWorkFactor)
            {
                throw new InvalidOperationException(
                    $"BCRYPT_WORK_FACTOR must not exceed {MaximumBcryptWorkFactor}. Current value: {workFactor}");
            }

            return workFactor;
        }
    }
}
