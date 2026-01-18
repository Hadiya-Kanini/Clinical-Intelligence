using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.ExtractedEntities;
using ClinicalIntelligence.Api.Services.ProcessingJobs;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Test-specific ApplicationDbContext that bypasses pgvector configuration
/// for in-memory database compatibility.
/// </summary>
public sealed class TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : DbContext(options), IProcessingJobFailureDbContext, IExtractedEntityDbContext
{
    // Core entities needed for our tests
    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();
    public DbSet<ExtractedEntity> ExtractedEntities => Set<ExtractedEntity>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ErdPatient> ErdPatients => Set<ErdPatient>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Only configure the entities we need for tests
        ConfigureUser(modelBuilder);
        ConfigureErdPatient(modelBuilder);
        ConfigureDocument(modelBuilder);
        ConfigureProcessingJob(modelBuilder);
        ConfigureExtractedEntity(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });
    }

    private static void ConfigureErdPatient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErdPatient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mrn).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        });
    }

    private static void ConfigureDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
        });
    }

    private static void ConfigureProcessingJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessingJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.ErrorDetails).HasMaxLength(2000);
        });
    }

    private static void ConfigureExtractedEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExtractedEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).HasMaxLength(500);
            entity.Property(e => e.Units).HasMaxLength(50);
        });
    }
}
