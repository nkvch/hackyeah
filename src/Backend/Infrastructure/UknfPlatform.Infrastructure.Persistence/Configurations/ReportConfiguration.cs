using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Report entity
/// </summary>
public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        // Primary Key
        builder.HasKey(r => r.Id);

        // Required Properties
        builder.Property(r => r.EntityId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.FileStorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.FileSize)
            .IsRequired();

        builder.Property(r => r.ReportType)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(r => r.ReportingPeriod)
            .IsRequired()
            .HasMaxLength(100);

        // Enum stored as string with check constraint
        builder.Property(r => r.ValidationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Nullable Properties
        builder.Property(r => r.ValidationResultFileKey)
            .HasMaxLength(500);

        builder.Property(r => r.UniqueValidationId)
            .HasMaxLength(100);

        builder.Property(r => r.ErrorDescription)
            .HasColumnType("text"); // Use text for potentially long error messages

        builder.Property(r => r.ContestedDescription)
            .HasColumnType("text");

        // Boolean Properties
        builder.Property(r => r.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        // Timestamp Properties
        builder.Property(r => r.SubmittedDate)
            .IsRequired();

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.UpdatedDate)
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(r => r.EntityId)
            .HasDatabaseName("IX_Reports_EntityId");

        builder.HasIndex(r => r.ValidationStatus)
            .HasDatabaseName("IX_Reports_ValidationStatus");

        builder.HasIndex(r => r.ReportingPeriod)
            .HasDatabaseName("IX_Reports_ReportingPeriod");

        builder.HasIndex(r => r.SubmittedDate)
            .HasDatabaseName("IX_Reports_SubmittedDate");

        builder.HasIndex(r => r.IsArchived)
            .HasDatabaseName("IX_Reports_IsArchived");

        // Composite index for finding reports by entity and period
        builder.HasIndex(r => new { r.EntityId, r.ReportingPeriod, r.ReportType })
            .HasDatabaseName("IX_Reports_Entity_Period_Type");

        // Foreign key relationships will be added when Entity table is created
        // For now, just the column references
        // TODO: Add foreign key constraints in migration when Entity table exists
    }
}

