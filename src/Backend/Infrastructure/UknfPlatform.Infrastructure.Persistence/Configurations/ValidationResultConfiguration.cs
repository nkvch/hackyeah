using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

public class ValidationResultConfiguration : IEntityTypeConfiguration<ValidationResult>
{
    public void Configure(EntityTypeBuilder<ValidationResult> builder)
    {
        builder.ToTable("ValidationResults");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ReportId).IsRequired();
        builder.Property(v => v.UniqueValidationId).IsRequired().HasMaxLength(100);
        
        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(v => v.ValidationStartedDate).IsRequired();
        builder.Property(v => v.ValidationCompletedDate);
        builder.Property(v => v.ResultFileStorageKey).HasMaxLength(500);
        builder.Property(v => v.IsValid).IsRequired().HasDefaultValue(false);
        
        // JSON columns for errors, warnings, and metadata
        builder.Property(v => v.ErrorsJson).HasColumnType("nvarchar(max)");
        builder.Property(v => v.WarningsJson).HasColumnType("nvarchar(max)");
        builder.Property(v => v.ExtractedMetadataJson).HasColumnType("nvarchar(max)");
        builder.Property(v => v.TechnicalErrorMessage).HasMaxLength(4000);

        builder.Property(v => v.CreatedDate).IsRequired();
        builder.Property(v => v.UpdatedDate).IsRequired();

        // Indexes
        builder.HasIndex(v => v.ReportId).HasDatabaseName("IX_ValidationResults_ReportId");
        builder.HasIndex(v => v.UniqueValidationId).IsUnique().HasDatabaseName("IX_ValidationResults_UniqueValidationId");
        builder.HasIndex(v => v.Status).HasDatabaseName("IX_ValidationResults_Status");

        // Foreign Key
        builder.HasOne(v => v.Report)
            .WithMany()
            .HasForeignKey(v => v.ReportId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


