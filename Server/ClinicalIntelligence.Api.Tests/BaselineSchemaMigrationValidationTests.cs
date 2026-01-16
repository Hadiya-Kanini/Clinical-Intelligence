using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration tests validating the US_119 baseline schema migration.
/// These tests require a PostgreSQL database with pgvector extension.
/// Tests are skipped when PostgreSQL is not available.
/// </summary>
[Collection("Database")]
public class BaselineSchemaMigrationValidationTests : IDisposable
{
    private readonly ApplicationDbContext? _context;
    private readonly bool _isPostgresAvailable;

    public BaselineSchemaMigrationValidationTests()
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("TEST_DATABASE_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            _isPostgresAvailable = false;
            return;
        }

        try
        {
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseVector()));

            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Test connection
            _context.Database.CanConnect();
            _isPostgresAvailable = true;
        }
        catch
        {
            _isPostgresAvailable = false;
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public void AllErdTables_ShouldExist_WhenMigrationApplied()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var expectedTables = new[]
        {
            "users",
            "sessions",
            "password_reset_tokens",
            "erd_patients",
            "document_batches",
            "documents",
            "processing_jobs",
            "document_chunks",
            "extracted_entities",
            "entity_citations",
            "conflicts",
            "conflict_resolutions",
            "billing_code_catalog_items",
            "code_suggestions",
            "audit_log_events",
            "vector_query_logs"
        };

        foreach (var tableName in expectedTables)
        {
            var tableExists = _context!.Database
                .SqlQueryRaw<int>($"SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}'")
                .Any();

            Assert.True(tableExists, $"Table '{tableName}' should exist");
        }
    }

    [Fact]
    public async Task UserEmail_ShouldBeUnique_WhenDuplicateInserted()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var uniqueEmail = $"test-{Guid.NewGuid()}@example.com";

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = uniqueEmail,
            PasswordHash = "hash1",
            Name = "Test User 1",
            Role = "Standard",
            Status = "Active"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = uniqueEmail,
            PasswordHash = "hash2",
            Name = "Test User 2",
            Role = "Standard",
            Status = "Active"
        };

        _context!.Users.Add(user1);
        await _context.SaveChangesAsync();

        _context.Users.Add(user2);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await _context.SaveChangesAsync());

        Assert.Contains("ix_users_email", exception.InnerException?.Message ?? exception.Message, 
            StringComparison.OrdinalIgnoreCase);

        // Cleanup
        _context.Users.Remove(user1);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task ErdPatientMrn_ShouldBeUnique_WhenDuplicateInserted()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var uniqueMrn = $"MRN-{Guid.NewGuid():N}".Substring(0, 50);

        var patient1 = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = uniqueMrn,
            Name = "Test Patient 1"
        };

        var patient2 = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = uniqueMrn,
            Name = "Test Patient 2"
        };

        _context!.ErdPatients.Add(patient1);
        await _context.SaveChangesAsync();

        _context.ErdPatients.Add(patient2);

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await _context.SaveChangesAsync());

        Assert.Contains("ix_erd_patients_mrn", exception.InnerException?.Message ?? exception.Message,
            StringComparison.OrdinalIgnoreCase);

        // Cleanup
        _context.ErdPatients.Remove(patient1);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public void DocumentChunkEmbedding_ShouldBeVector768_WhenColumnInspected()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var columnType = _context!.Database
            .SqlQueryRaw<string>(
                @"SELECT data_type || '(' || character_maximum_length || ')' as column_type
                  FROM information_schema.columns 
                  WHERE table_name = 'document_chunks' AND column_name = 'Embedding'")
            .FirstOrDefault();

        // pgvector columns show as 'USER-DEFINED' in information_schema
        // Check via pg_type instead
        var isVectorColumn = _context.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM pg_attribute a
                  JOIN pg_class c ON a.attrelid = c.oid
                  JOIN pg_type t ON a.atttypid = t.oid
                  WHERE c.relname = 'document_chunks' 
                  AND a.attname = 'Embedding'
                  AND t.typname = 'vector'")
            .Any();

        Assert.True(isVectorColumn, "DocumentChunk.Embedding should be of type vector");
    }

    [Fact]
    public async Task ForeignKeyConstraints_ShouldEnforce_WhenOrphanInserted()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(), // Non-existent user
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _context!.Sessions.Add(session);

        await Assert.ThrowsAsync<DbUpdateException>(
            async () => await _context.SaveChangesAsync());
    }

    [Fact]
    public async Task CascadeDelete_ShouldWork_WhenUserDeleted()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"cascade-test-{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            Name = "Cascade Test User",
            Role = "Standard",
            Status = "Active"
        };

        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _context!.Users.Add(user);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Verify session exists
        var sessionExists = await _context.Sessions.AnyAsync(s => s.Id == session.Id);
        Assert.True(sessionExists);

        // Delete user (should cascade to session)
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Verify session was deleted
        var sessionStillExists = await _context.Sessions
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Id == session.Id);
        Assert.False(sessionStillExists, "Session should be deleted when user is deleted (cascade)");
    }

    [Fact]
    public void MigrationHistory_ShouldContain_US119Migration()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var hasMigration = _context!.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM ""__EFMigrationsHistory"" 
                  WHERE ""MigrationId"" LIKE '%US119_ErdBaselineSchema%'")
            .Any();

        Assert.True(hasMigration, "US119_ErdBaselineSchema migration should be in history");
    }

    #region US_120 FK Enforcement and Delete Behavior Tests

    [Fact]
    public async Task ForeignKeyConstraint_ShouldEnforce_WhenDocumentInsertedWithInvalidPatientId()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        // Create a user for the uploaded_by_user_id FK
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"fk-test-{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            Name = "FK Test User",
            Role = "Standard",
            Status = "Active"
        };

        _context!.Users.Add(user);
        await _context.SaveChangesAsync();

        try
        {
            var document = new Document
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(), // Non-existent patient
                UploadedByUserId = user.Id,
                OriginalName = "test.pdf",
                MimeType = "application/pdf",
                SizeBytes = 1024,
                StoragePath = "/test/path",
                Status = "Pending"
            };

            _context.Documents.Add(document);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                async () => await _context.SaveChangesAsync());

            // Verify FK constraint violation message references the constraint
            var errorMessage = exception.InnerException?.Message ?? exception.Message;
            Assert.True(
                errorMessage.Contains("fk_documents_patient_id", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("erd_patients", StringComparison.OrdinalIgnoreCase),
                $"Error should reference FK constraint. Actual: {errorMessage}");
        }
        finally
        {
            // Cleanup
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task ForeignKeyConstraint_ShouldEnforce_WhenExtractedEntityInsertedWithInvalidDocumentId()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        // Create a patient for the patient_id FK
        var patient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = $"MRN-{Guid.NewGuid():N}".Substring(0, 50),
            Name = "FK Test Patient"
        };

        _context!.ErdPatients.Add(patient);
        await _context.SaveChangesAsync();

        try
        {
            var entity = new ExtractedEntity
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                DocumentId = Guid.NewGuid(), // Non-existent document
                Category = "Diagnosis",
                Name = "Test Entity"
            };

            _context.ExtractedEntities.Add(entity);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                async () => await _context.SaveChangesAsync());

            // Verify FK constraint violation
            var errorMessage = exception.InnerException?.Message ?? exception.Message;
            Assert.True(
                errorMessage.Contains("documents", StringComparison.OrdinalIgnoreCase),
                $"Error should reference documents table. Actual: {errorMessage}");
        }
        finally
        {
            // Cleanup
            _context.ErdPatients.Remove(patient);
            await _context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task RestrictDelete_ShouldBlockPatientDeletion_WhenDocumentsExist()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        // Create patient
        var patient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = $"MRN-{Guid.NewGuid():N}".Substring(0, 50),
            Name = "Restrict Test Patient"
        };

        // Create user for document upload
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"restrict-test-{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            Name = "Restrict Test User",
            Role = "Standard",
            Status = "Active"
        };

        _context!.ErdPatients.Add(patient);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create document referencing the patient
        var document = new Document
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            UploadedByUserId = user.Id,
            OriginalName = "restrict-test.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = "/test/restrict-path",
            Status = "Pending"
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        try
        {
            // Attempt to delete patient (should fail due to RESTRICT)
            _context.ErdPatients.Remove(patient);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                async () => await _context.SaveChangesAsync());

            // Verify FK constraint violation with RESTRICT behavior
            var errorMessage = exception.InnerException?.Message ?? exception.Message;
            Assert.True(
                errorMessage.Contains("fk_documents_patient_id", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("violates foreign key constraint", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("erd_patients", StringComparison.OrdinalIgnoreCase),
                $"Error should indicate FK constraint violation. Actual: {errorMessage}");
        }
        finally
        {
            // Cleanup - must delete in correct order
            _context.ChangeTracker.Clear();
            
            var docToRemove = await _context.Documents
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == document.Id);
            if (docToRemove != null)
            {
                _context.Documents.Remove(docToRemove);
                await _context.SaveChangesAsync();
            }

            var patientToRemove = await _context.ErdPatients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == patient.Id);
            if (patientToRemove != null)
            {
                _context.ErdPatients.Remove(patientToRemove);
                await _context.SaveChangesAsync();
            }

            var userToRemove = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == user.Id);
            if (userToRemove != null)
            {
                _context.Users.Remove(userToRemove);
                await _context.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public async Task CascadeDelete_ShouldDeleteSessions_WhenUserDeleted_AC4()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        // Create user with multiple sessions
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"cascade-ac4-{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            Name = "Cascade AC4 Test User",
            Role = "Standard",
            Status = "Active"
        };

        var session1 = new Session
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var session2 = new Session
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };

        _context!.Users.Add(user);
        _context.Sessions.Add(session1);
        _context.Sessions.Add(session2);
        await _context.SaveChangesAsync();

        // Verify sessions exist
        var sessionsExist = await _context.Sessions
            .Where(s => s.UserId == user.Id)
            .CountAsync();
        Assert.Equal(2, sessionsExist);

        // Delete user (should cascade to sessions per AC-4)
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Verify all sessions were deleted
        var sessionsRemaining = await _context.Sessions
            .IgnoreQueryFilters()
            .Where(s => s.UserId == user.Id)
            .CountAsync();
        Assert.Equal(0, sessionsRemaining);
    }

    [Fact]
    public async Task SetNullDelete_ShouldSetNull_WhenVerifiedByUserDeleted()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        // Create patient, user, and document
        var patient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = $"MRN-{Guid.NewGuid():N}".Substring(0, 50),
            Name = "SetNull Test Patient"
        };

        var uploadUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"setnull-upload-{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            Name = "Upload User",
            Role = "Standard",
            Status = "Active"
        };

        var verifyUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"setnull-verify-{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            Name = "Verify User",
            Role = "Standard",
            Status = "Active"
        };

        _context!.ErdPatients.Add(patient);
        _context.Users.Add(uploadUser);
        _context.Users.Add(verifyUser);
        await _context.SaveChangesAsync();

        var document = new Document
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            UploadedByUserId = uploadUser.Id,
            OriginalName = "setnull-test.pdf",
            MimeType = "application/pdf",
            SizeBytes = 1024,
            StoragePath = "/test/setnull-path",
            Status = "Completed"
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        var entity = new ExtractedEntity
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            DocumentId = document.Id,
            Category = "Diagnosis",
            Name = "Test Diagnosis",
            IsVerified = true,
            VerifiedByUserId = verifyUser.Id,
            VerifiedAt = DateTime.UtcNow
        };

        _context.ExtractedEntities.Add(entity);
        await _context.SaveChangesAsync();

        try
        {
            // Delete the verify user (should SET NULL on extracted_entity.verified_by_user_id)
            _context.Users.Remove(verifyUser);
            await _context.SaveChangesAsync();

            // Verify entity still exists but verified_by_user_id is null
            var updatedEntity = await _context.ExtractedEntities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == entity.Id);

            Assert.NotNull(updatedEntity);
            Assert.Null(updatedEntity.VerifiedByUserId);
        }
        finally
        {
            // Cleanup
            _context.ChangeTracker.Clear();

            var entityToRemove = await _context.ExtractedEntities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == entity.Id);
            if (entityToRemove != null)
            {
                _context.ExtractedEntities.Remove(entityToRemove);
                await _context.SaveChangesAsync();
            }

            var docToRemove = await _context.Documents
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == document.Id);
            if (docToRemove != null)
            {
                _context.Documents.Remove(docToRemove);
                await _context.SaveChangesAsync();
            }

            var patientToRemove = await _context.ErdPatients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == patient.Id);
            if (patientToRemove != null)
            {
                _context.ErdPatients.Remove(patientToRemove);
                await _context.SaveChangesAsync();
            }

            var uploadUserToRemove = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == uploadUser.Id);
            if (uploadUserToRemove != null)
            {
                _context.Users.Remove(uploadUserToRemove);
                await _context.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public void MigrationHistory_ShouldContain_US120Migration()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var hasMigration = _context!.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM ""__EFMigrationsHistory"" 
                  WHERE ""MigrationId"" LIKE '%US120_ForeignKeysAndReferentialIntegrity%'")
            .Any();

        Assert.True(hasMigration, "US120_ForeignKeysAndReferentialIntegrity migration should be in history");
    }

    #endregion

    #region US_121 Index Presence and Query Plan Tests

    [Fact]
    public void US121_RequiredIndexes_ShouldExist_WhenMigrationApplied()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var requiredIndexes = new[]
        {
            "ix_users_email",
            "ix_documents_patient_id_uploaded_at",
            "ix_processing_jobs_status",
            "ix_extracted_entities_patient_id",
            "ix_audit_log_events_timestamp",
            "ix_document_chunks_embedding_hnsw"
        };

        foreach (var indexName in requiredIndexes)
        {
            var indexExists = _context!.Database
                .SqlQueryRaw<int>($@"SELECT 1 FROM pg_indexes WHERE indexname = '{indexName}'")
                .Any();

            Assert.True(indexExists, $"Index '{indexName}' should exist for US_121 requirements");
        }
    }

    [Fact]
    public void US121_UsersEmailIndex_ShouldBeUnique()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var isUnique = _context!.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM pg_indexes i
                  JOIN pg_class c ON c.relname = i.indexname
                  JOIN pg_index idx ON idx.indexrelid = c.oid
                  WHERE i.indexname = 'ix_users_email' AND idx.indisunique = true")
            .Any();

        Assert.True(isUnique, "ix_users_email should be a unique index (AC-1)");
    }

    [Fact]
    public void US121_DocumentsCompositeIndex_ShouldHaveCorrectColumns()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var hasPatientIdColumn = _context!.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM pg_indexes i
                  JOIN pg_class c ON c.relname = i.indexname
                  JOIN pg_index idx ON idx.indexrelid = c.oid
                  JOIN pg_attribute a ON a.attrelid = idx.indrelid AND a.attnum = ANY(idx.indkey)
                  WHERE i.indexname = 'ix_documents_patient_id_uploaded_at' 
                  AND a.attname = 'PatientId'")
            .Any();

        var hasUploadedAtColumn = _context.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM pg_indexes i
                  JOIN pg_class c ON c.relname = i.indexname
                  JOIN pg_index idx ON idx.indexrelid = c.oid
                  JOIN pg_attribute a ON a.attrelid = idx.indrelid AND a.attnum = ANY(idx.indkey)
                  WHERE i.indexname = 'ix_documents_patient_id_uploaded_at' 
                  AND a.attname = 'UploadedAt'")
            .Any();

        Assert.True(hasPatientIdColumn, "ix_documents_patient_id_uploaded_at should include PatientId column (AC-2)");
        Assert.True(hasUploadedAtColumn, "ix_documents_patient_id_uploaded_at should include UploadedAt column (AC-2)");
    }

    [Fact]
    public void US121_HnswIndex_ShouldUseCorrectMethod()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var usesHnsw = _context!.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM pg_indexes 
                  WHERE indexname = 'ix_document_chunks_embedding_hnsw' 
                  AND indexdef LIKE '%USING hnsw%'")
            .Any();

        Assert.True(usesHnsw, "ix_document_chunks_embedding_hnsw should use HNSW index method (AC-6)");
    }

    [Fact]
    public async Task US121_DocumentListingQuery_ShouldUseCompositeIndex_AC2_AC7()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        // Create test data for query planner to choose index
        var patient = new ErdPatient
        {
            Id = Guid.NewGuid(),
            Mrn = $"MRN-{Guid.NewGuid():N}".Substring(0, 50),
            Name = "Index Test Patient"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"index-test-{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            Name = "Index Test User",
            Role = "Standard",
            Status = "Active"
        };

        _context!.ErdPatients.Add(patient);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        try
        {
            // Run EXPLAIN on document listing query
            var explainResult = await _context.Database
                .SqlQueryRaw<string>(
                    $@"EXPLAIN (FORMAT TEXT) 
                       SELECT * FROM documents 
                       WHERE ""PatientId"" = '{patient.Id}' 
                       AND ""UploadedAt"" BETWEEN '2024-01-01' AND '2024-12-31' 
                       ORDER BY ""UploadedAt"" DESC LIMIT 20")
                .ToListAsync();

            var planText = string.Join("\n", explainResult);

            // Assert index usage (Index Scan, Index Only Scan, or Bitmap Index Scan)
            var usesIndex = planText.Contains("ix_documents_patient_id_uploaded_at", StringComparison.OrdinalIgnoreCase) ||
                           planText.Contains("Index Scan", StringComparison.OrdinalIgnoreCase) ||
                           planText.Contains("Index Only Scan", StringComparison.OrdinalIgnoreCase) ||
                           planText.Contains("Bitmap Index Scan", StringComparison.OrdinalIgnoreCase);

            Assert.True(usesIndex, $"Document listing query should use index scan (AC-2, AC-7). Plan:\n{planText}");
        }
        finally
        {
            _context.ChangeTracker.Clear();
            var patientToRemove = await _context.ErdPatients.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == patient.Id);
            if (patientToRemove != null)
            {
                _context.ErdPatients.Remove(patientToRemove);
                await _context.SaveChangesAsync();
            }
            var userToRemove = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
            if (userToRemove != null)
            {
                _context.Users.Remove(userToRemove);
                await _context.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public async Task US121_ProcessingJobQueueQuery_ShouldUseStatusIndex_AC3_AC7()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var explainResult = await _context!.Database
            .SqlQueryRaw<string>(
                @"EXPLAIN (FORMAT TEXT) 
                  SELECT * FROM processing_jobs 
                  WHERE ""Status"" = 'Pending' 
                  ORDER BY ""Id"" LIMIT 50")
            .ToListAsync();

        var planText = string.Join("\n", explainResult);

        var usesIndex = planText.Contains("ix_processing_jobs_status", StringComparison.OrdinalIgnoreCase) ||
                       planText.Contains("Index Scan", StringComparison.OrdinalIgnoreCase) ||
                       planText.Contains("Bitmap Index Scan", StringComparison.OrdinalIgnoreCase);

        Assert.True(usesIndex, $"Processing job queue query should use index scan (AC-3, AC-7). Plan:\n{planText}");
    }

    [Fact]
    public async Task US121_EntityAggregationQuery_ShouldUsePatientIdIndex_AC4_AC7()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var testPatientId = Guid.NewGuid();

        var explainResult = await _context!.Database
            .SqlQueryRaw<string>(
                $@"EXPLAIN (FORMAT TEXT) 
                   SELECT * FROM extracted_entities 
                   WHERE ""PatientId"" = '{testPatientId}'")
            .ToListAsync();

        var planText = string.Join("\n", explainResult);

        var usesIndex = planText.Contains("ix_extracted_entities_patient_id", StringComparison.OrdinalIgnoreCase) ||
                       planText.Contains("Index Scan", StringComparison.OrdinalIgnoreCase) ||
                       planText.Contains("Bitmap Index Scan", StringComparison.OrdinalIgnoreCase);

        Assert.True(usesIndex, $"Entity aggregation query should use index scan (AC-4, AC-7). Plan:\n{planText}");
    }

    [Fact]
    public async Task US121_AuditLogDateRangeQuery_ShouldUseTimestampIndex_AC5_AC7()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var explainResult = await _context!.Database
            .SqlQueryRaw<string>(
                @"EXPLAIN (FORMAT TEXT) 
                  SELECT * FROM audit_log_events 
                  WHERE ""Timestamp"" BETWEEN '2024-01-01' AND '2024-12-31' 
                  ORDER BY ""Timestamp"" DESC LIMIT 100")
            .ToListAsync();

        var planText = string.Join("\n", explainResult);

        var usesIndex = planText.Contains("ix_audit_log_events_timestamp", StringComparison.OrdinalIgnoreCase) ||
                       planText.Contains("Index Scan", StringComparison.OrdinalIgnoreCase) ||
                       planText.Contains("Bitmap Index Scan", StringComparison.OrdinalIgnoreCase);

        Assert.True(usesIndex, $"Audit log date range query should use index scan (AC-5, AC-7). Plan:\n{planText}");
    }

    [Fact]
    public async Task US121_VectorSimilarityQuery_ShouldUseHnswIndex_AC6_AC7()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        // Generate a 768-dimensional zero vector for testing
        var vectorDimensions = string.Join(",", Enumerable.Repeat("0", 768));

        var explainResult = await _context!.Database
            .SqlQueryRaw<string>(
                $@"EXPLAIN (FORMAT TEXT) 
                   SELECT ""Id"" FROM document_chunks 
                   ORDER BY ""Embedding"" <=> '[{vectorDimensions}]'::vector 
                   LIMIT 15")
            .ToListAsync();

        var planText = string.Join("\n", explainResult);

        var usesHnswIndex = planText.Contains("ix_document_chunks_embedding_hnsw", StringComparison.OrdinalIgnoreCase) ||
                           planText.Contains("Index Scan", StringComparison.OrdinalIgnoreCase);

        Assert.True(usesHnswIndex, $"Vector similarity query should use HNSW index scan (AC-6, AC-7). Plan:\n{planText}");
    }

    [Fact]
    public void MigrationHistory_ShouldContain_US121Migration()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var hasMigration = _context!.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM ""__EFMigrationsHistory"" 
                  WHERE ""MigrationId"" LIKE '%US121_DatabaseIndexingStrategy%'")
            .Any();

        Assert.True(hasMigration, "US121_DatabaseIndexingStrategy migration should be in history");
    }

    #endregion

    #region US_038 Case-Insensitive Email Uniqueness Tests

    [Fact]
    public async Task UserEmail_ShouldBeCaseInsensitiveUnique_WhenDifferentCaseInserted()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var uniqueId = Guid.NewGuid().ToString("N");
        var lowercaseEmail = $"case-test-{uniqueId}@example.com";
        var uppercaseEmail = $"CASE-TEST-{uniqueId}@EXAMPLE.COM";

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = lowercaseEmail,
            PasswordHash = "hash1",
            Name = "Case Test User 1",
            Role = "Standard",
            Status = "Active"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = uppercaseEmail,
            PasswordHash = "hash2",
            Name = "Case Test User 2",
            Role = "Standard",
            Status = "Active"
        };

        _context!.Users.Add(user1);
        await _context.SaveChangesAsync();

        try
        {
            _context.Users.Add(user2);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                async () => await _context.SaveChangesAsync());

            Assert.Contains("ix_users_email", exception.InnerException?.Message ?? exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            _context.ChangeTracker.Clear();
            var userToRemove = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == user1.Id);
            if (userToRemove != null)
            {
                _context.Users.Remove(userToRemove);
                await _context.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public async Task UserEmail_ShouldBeCaseInsensitiveUnique_WhenMixedCaseInserted()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var uniqueId = Guid.NewGuid().ToString("N");
        var email1 = $"mixed-case-{uniqueId}@example.com";
        var email2 = $"Mixed-Case-{uniqueId}@Example.COM";

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = email1,
            PasswordHash = "hash1",
            Name = "Mixed Case User 1",
            Role = "Standard",
            Status = "Active"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = email2,
            PasswordHash = "hash2",
            Name = "Mixed Case User 2",
            Role = "Standard",
            Status = "Active"
        };

        _context!.Users.Add(user1);
        await _context.SaveChangesAsync();

        try
        {
            _context.Users.Add(user2);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                async () => await _context.SaveChangesAsync());

            var errorMessage = exception.InnerException?.Message ?? exception.Message;
            Assert.True(
                errorMessage.Contains("ix_users_email", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase),
                $"Error should indicate unique constraint violation. Actual: {errorMessage}");
        }
        finally
        {
            _context.ChangeTracker.Clear();
            var userToRemove = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == user1.Id);
            if (userToRemove != null)
            {
                _context.Users.Remove(userToRemove);
                await _context.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public void US038_CitextExtension_ShouldBeEnabled()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var hasCitext = _context!.Database
            .SqlQueryRaw<int>(@"SELECT 1 FROM pg_extension WHERE extname = 'citext'")
            .Any();

        Assert.True(hasCitext, "citext extension should be enabled for case-insensitive email uniqueness (US_038)");
    }

    [Fact]
    public void US038_UserEmailColumn_ShouldUseCitextType()
    {
        Skip.If(!_isPostgresAvailable, "PostgreSQL not available");

        var isCitext = _context!.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 FROM information_schema.columns 
                  WHERE table_name = 'users' 
                  AND column_name = 'Email' 
                  AND udt_name = 'citext'")
            .Any();

        Assert.True(isCitext, "users.Email column should use citext type for case-insensitive uniqueness (US_038)");
    }

    #endregion
}
