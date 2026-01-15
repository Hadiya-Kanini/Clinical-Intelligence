using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
// TEMPORARY: Commented out for vector DB installation
// using Pgvector.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Existing FHIR-aligned entities
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Encounter> Encounters => Set<Encounter>();
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<MedicationStatement> MedicationStatements => Set<MedicationStatement>();
    public DbSet<Condition> Conditions => Set<Condition>();
    public DbSet<Procedure> Procedures => Set<Procedure>();
    public DbSet<DocumentReference> DocumentReferences => Set<DocumentReference>();
    public DbSet<FhirResourceLink> FhirResourceLinks => Set<FhirResourceLink>();

    // ERD entities for US_119 baseline schema
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<ErdPatient> ErdPatients => Set<ErdPatient>();
    public DbSet<DocumentBatch> DocumentBatches => Set<DocumentBatch>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();
    // TEMPORARY: Commented out for vector DB installation
    // public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<ExtractedEntity> ExtractedEntities => Set<ExtractedEntity>();
    // TEMPORARY: Commented out for vector DB installation
    // public DbSet<EntityCitation> EntityCitations => Set<EntityCitation>();
    public DbSet<ErdConflict> ErdConflicts => Set<ErdConflict>();
    public DbSet<ConflictResolution> ConflictResolutions => Set<ConflictResolution>();
    public DbSet<BillingCodeCatalogItem> BillingCodeCatalogItems => Set<BillingCodeCatalogItem>();
    public DbSet<CodeSuggestion> CodeSuggestions => Set<CodeSuggestion>();
    public DbSet<AuditLogEvent> AuditLogEvents => Set<AuditLogEvent>();
    // TEMPORARY: Commented out for vector DB installation
    // public DbSet<VectorQueryLog> VectorQueryLogs => Set<VectorQueryLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TEMPORARY: Commented out for vector DB installation
        // Enable pgvector extension
        // modelBuilder.HasPostgresExtension("vector");

        // Existing FHIR-aligned entity configurations
        ConfigurePatient(modelBuilder);
        ConfigureEncounter(modelBuilder);
        ConfigureObservation(modelBuilder);
        ConfigureMedicationStatement(modelBuilder);
        ConfigureCondition(modelBuilder);
        ConfigureProcedure(modelBuilder);
        ConfigureDocumentReference(modelBuilder);
        ConfigureFhirResourceLink(modelBuilder);

        // ERD entity configurations for US_119
        ConfigureUser(modelBuilder);
        ConfigureSession(modelBuilder);
        ConfigurePasswordResetToken(modelBuilder);
        ConfigureErdPatient(modelBuilder);
        ConfigureDocumentBatch(modelBuilder);
        ConfigureDocument(modelBuilder);
        ConfigureProcessingJob(modelBuilder);
        // TEMPORARY: Commented out for vector DB installation
        // ConfigureDocumentChunk(modelBuilder);
        ConfigureExtractedEntity(modelBuilder);
        // TEMPORARY: Commented out for vector DB installation
        // ConfigureEntityCitation(modelBuilder);
        ConfigureErdConflict(modelBuilder);
        ConfigureConflictResolution(modelBuilder);
        ConfigureBillingCodeCatalogItem(modelBuilder);
        ConfigureCodeSuggestion(modelBuilder);
        ConfigureAuditLogEvent(modelBuilder);
        // TEMPORARY: Commented out for vector DB installation
        // ConfigureVectorQueryLog(modelBuilder);
    }

    private static void ConfigurePatient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Mrn)
                .IsUnique()
                .HasFilter("\"Mrn\" IS NOT NULL")
                .HasDatabaseName("ix_patients_mrn");

            entity.HasIndex(e => new { e.FamilyName, e.GivenName, e.DateOfBirth })
                .HasDatabaseName("ix_patients_name_dob");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("ix_patients_is_deleted");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureEncounter(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.ToTable("encounters");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_encounters_patient_id");

            entity.HasIndex(e => e.StartDate)
                .HasDatabaseName("ix_encounters_start_date");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Encounters)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureObservation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Observation>(entity =>
        {
            entity.ToTable("observations");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_observations_patient_id");

            entity.HasIndex(e => e.EncounterId)
                .HasDatabaseName("ix_observations_encounter_id");

            entity.HasIndex(e => e.Code)
                .HasDatabaseName("ix_observations_code");

            entity.HasIndex(e => e.EffectiveDate)
                .HasDatabaseName("ix_observations_effective_date");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Observations)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Encounter)
                .WithMany(enc => enc.Observations)
                .HasForeignKey(e => e.EncounterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureMedicationStatement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicationStatement>(entity =>
        {
            entity.ToTable("medication_statements");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_medication_statements_patient_id");

            entity.HasIndex(e => e.MedicationCode)
                .HasDatabaseName("ix_medication_statements_medication_code");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.MedicationStatements)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureCondition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Condition>(entity =>
        {
            entity.ToTable("conditions");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_conditions_patient_id");

            entity.HasIndex(e => e.EncounterId)
                .HasDatabaseName("ix_conditions_encounter_id");

            entity.HasIndex(e => e.Code)
                .HasDatabaseName("ix_conditions_code");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Conditions)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Encounter)
                .WithMany(enc => enc.Conditions)
                .HasForeignKey(e => e.EncounterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureProcedure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Procedure>(entity =>
        {
            entity.ToTable("procedures");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_procedures_patient_id");

            entity.HasIndex(e => e.EncounterId)
                .HasDatabaseName("ix_procedures_encounter_id");

            entity.HasIndex(e => e.Code)
                .HasDatabaseName("ix_procedures_code");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Procedures)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Encounter)
                .WithMany(enc => enc.Procedures)
                .HasForeignKey(e => e.EncounterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureDocumentReference(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentReference>(entity =>
        {
            entity.ToTable("document_references");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_document_references_patient_id");

            entity.HasIndex(e => e.ProcessingStatus)
                .HasDatabaseName("ix_document_references_processing_status");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("ix_document_references_is_deleted");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.DocumentReferences)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureFhirResourceLink(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FhirResourceLink>(entity =>
        {
            entity.ToTable("fhir_resource_links");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.InternalEntityType, e.InternalEntityId })
                .HasDatabaseName("ix_fhir_resource_links_internal_entity");

            entity.HasIndex(e => new { e.FhirResourceType, e.FhirResourceId, e.FhirVersion })
                .HasDatabaseName("ix_fhir_resource_links_fhir_resource");

            entity.HasIndex(e => e.SourceSystem)
                .HasDatabaseName("ix_fhir_resource_links_source_system");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    #region ERD Entity Configurations (US_119)

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("ix_users_email");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_users_status");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("ix_users_is_deleted");

            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

            entity.HasIndex(e => e.IsStaticAdmin)
                .HasDatabaseName("ix_users_is_static_admin");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_sessions_user_id");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("ix_sessions_expires_at");

            entity.HasIndex(e => e.IsRevoked)
                .HasDatabaseName("ix_sessions_is_revoked");

            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(45);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePasswordResetToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_password_reset_tokens_user_id");

            entity.HasIndex(e => e.TokenHash)
                .HasDatabaseName("ix_password_reset_tokens_token_hash");

            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(255);

            entity.HasOne(e => e.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureErdPatient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErdPatient>(entity =>
        {
            entity.ToTable("erd_patients");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Mrn)
                .IsUnique()
                .HasDatabaseName("ix_erd_patients_mrn");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("ix_erd_patients_is_deleted");

            entity.Property(e => e.Mrn).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Contact).HasMaxLength(100);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureDocumentBatch(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentBatch>(entity =>
        {
            entity.ToTable("document_batches");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_document_batches_patient_id");

            entity.HasIndex(e => e.UploadedByUserId)
                .HasDatabaseName("ix_document_batches_uploaded_by_user_id");

            entity.HasIndex(e => e.UploadedAt)
                .HasDatabaseName("ix_document_batches_uploaded_at");

            entity.Property(e => e.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.DocumentBatches)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_document_batches_patient_id");

            entity.HasOne(e => e.UploadedByUser)
                .WithMany(u => u.UploadedBatches)
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_documents_patient_id");

            entity.HasIndex(e => e.DocumentBatchId)
                .HasDatabaseName("ix_documents_document_batch_id");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_documents_status");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("ix_documents_is_deleted");

            entity.HasIndex(e => new { e.PatientId, e.UploadedAt })
                .HasDatabaseName("ix_documents_patient_id_uploaded_at");

            entity.Property(e => e.OriginalName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);

            entity.Property(e => e.UploadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Documents)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_documents_patient_id");

            entity.HasOne(e => e.DocumentBatch)
                .WithMany(b => b.Documents)
                .HasForeignKey(e => e.DocumentBatchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UploadedByUser)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureProcessingJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessingJob>(entity =>
        {
            entity.ToTable("processing_jobs");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.DocumentId)
                .HasDatabaseName("ix_processing_jobs_document_id");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_processing_jobs_status");

            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ErrorDetails).HasColumnType("jsonb");

            entity.HasOne(e => e.Document)
                .WithMany(d => d.ProcessingJobs)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // TEMPORARY: Commented out for vector DB installation
    /*
    private static void ConfigureDocumentChunk(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.ToTable("document_chunks");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.DocumentId)
                .HasDatabaseName("ix_document_chunks_document_id");

            entity.HasIndex(e => e.ChunkHash)
                .HasDatabaseName("ix_document_chunks_chunk_hash");

            entity.Property(e => e.Section).HasMaxLength(100);
            entity.Property(e => e.Coordinates).HasMaxLength(100);
            entity.Property(e => e.TextContent).IsRequired();
            entity.Property(e => e.ChunkHash).HasMaxLength(64);

            entity.Property(e => e.Embedding)
                .HasColumnType("vector(768)");

            entity.HasOne(e => e.Document)
                .WithMany(d => d.DocumentChunks)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    */

    private static void ConfigureExtractedEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExtractedEntity>(entity =>
        {
            entity.ToTable("extracted_entities");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_extracted_entities_patient_id");

            entity.HasIndex(e => e.DocumentId)
                .HasDatabaseName("ix_extracted_entities_document_id");

            entity.HasIndex(e => e.Category)
                .HasDatabaseName("ix_extracted_entities_category");

            entity.HasIndex(e => e.IsVerified)
                .HasDatabaseName("ix_extracted_entities_is_verified");

            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).HasMaxLength(500);
            entity.Property(e => e.Units).HasMaxLength(50);

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.ExtractedEntities)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_extracted_entities_patient_id");

            entity.HasOne(e => e.Document)
                .WithMany(d => d.ExtractedEntities)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.VerifiedByUser)
                .WithMany(u => u.VerifiedEntities)
                .HasForeignKey(e => e.VerifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    // TEMPORARY: Commented out for vector DB installation
    /*
    private static void ConfigureEntityCitation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityCitation>(entity =>
        {
            entity.ToTable("entity_citations");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ExtractedEntityId)
                .HasDatabaseName("ix_entity_citations_extracted_entity_id");

            entity.HasIndex(e => e.DocumentChunkId)
                .HasDatabaseName("ix_entity_citations_document_chunk_id");

            entity.Property(e => e.Section).HasMaxLength(100);
            entity.Property(e => e.Coordinates).HasMaxLength(100);

            entity.HasOne(e => e.ExtractedEntity)
                .WithMany(ee => ee.EntityCitations)
                .HasForeignKey(e => e.ExtractedEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.DocumentChunk)
                .WithMany(dc => dc.EntityCitations)
                .HasForeignKey(e => e.DocumentChunkId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    */

    private static void ConfigureErdConflict(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErdConflict>(entity =>
        {
            entity.ToTable("conflicts");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_conflicts_patient_id");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_conflicts_status");

            entity.HasIndex(e => e.Severity)
                .HasDatabaseName("ix_conflicts_severity");

            entity.Property(e => e.Field).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityCategory).HasMaxLength(50);
            entity.Property(e => e.ConflictingValues).HasColumnType("jsonb");
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

            entity.Property(e => e.DetectedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Conflicts)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_conflicts_patient_id");
        });
    }

    private static void ConfigureConflictResolution(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConflictResolution>(entity =>
        {
            entity.ToTable("conflict_resolutions");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ConflictId)
                .IsUnique()
                .HasDatabaseName("ix_conflict_resolutions_conflict_id");

            entity.HasIndex(e => e.ResolvedByUserId)
                .HasDatabaseName("ix_conflict_resolutions_resolved_by_user_id");

            entity.Property(e => e.ResolvedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Conflict)
                .WithOne(c => c.Resolution)
                .HasForeignKey<ConflictResolution>(e => e.ConflictId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ResolvedByUser)
                .WithMany(u => u.ConflictResolutions)
                .HasForeignKey(e => e.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBillingCodeCatalogItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingCodeCatalogItem>(entity =>
        {
            entity.ToTable("billing_code_catalog_items");
            entity.HasKey(e => e.Code);

            entity.HasIndex(e => e.CodeType)
                .HasDatabaseName("ix_billing_code_catalog_items_code_type");

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CodeType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
        });
    }

    private static void ConfigureCodeSuggestion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CodeSuggestion>(entity =>
        {
            entity.ToTable("code_suggestions");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_code_suggestions_patient_id");

            entity.HasIndex(e => e.ExtractedEntityId)
                .HasDatabaseName("ix_code_suggestions_extracted_entity_id");

            entity.HasIndex(e => e.Code)
                .HasDatabaseName("ix_code_suggestions_code");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("ix_code_suggestions_status");

            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CodeType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

            entity.Property(e => e.SuggestedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.CodeSuggestions)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_code_suggestions_patient_id");

            entity.HasOne(e => e.ExtractedEntity)
                .WithMany(ee => ee.CodeSuggestions)
                .HasForeignKey(e => e.ExtractedEntityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.BillingCodeCatalogItem)
                .WithMany(b => b.CodeSuggestions)
                .HasForeignKey(e => e.Code)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DecidedByUser)
                .WithMany(u => u.CodeDecisions)
                .HasForeignKey(e => e.DecidedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureAuditLogEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEvent>(entity =>
        {
            entity.ToTable("audit_log_events");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_audit_log_events_user_id");

            entity.HasIndex(e => e.SessionId)
                .HasDatabaseName("ix_audit_log_events_session_id");

            entity.HasIndex(e => e.ActionType)
                .HasDatabaseName("ix_audit_log_events_action_type");

            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("ix_audit_log_events_timestamp");

            entity.HasIndex(e => new { e.ResourceType, e.ResourceId })
                .HasDatabaseName("ix_audit_log_events_resource");

            entity.Property(e => e.ActionType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.ResourceType).HasMaxLength(50);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.Property(e => e.IntegrityHash).HasMaxLength(128);

            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogEvents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Session)
                .WithMany(s => s.AuditLogEvents)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    // TEMPORARY: Commented out for vector DB installation
    /*
    private static void ConfigureVectorQueryLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VectorQueryLog>(entity =>
        {
            entity.ToTable("vector_query_logs");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_vector_query_logs_user_id");

            entity.HasIndex(e => e.PatientId)
                .HasDatabaseName("ix_vector_query_logs_patient_id");

            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("ix_vector_query_logs_timestamp");

            entity.HasIndex(e => e.QueryHash)
                .HasDatabaseName("ix_vector_query_logs_query_hash");

            entity.Property(e => e.QueryText).IsRequired();
            entity.Property(e => e.QueryHash).HasMaxLength(64);

            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.VectorQueryLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Patient)
                .WithMany(p => p.VectorQueryLogs)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    */

    #endregion
}
