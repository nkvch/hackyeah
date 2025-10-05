using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Enums;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for AccessRequest entity.
/// Story 2.1: Automatic Access Request Creation
/// </summary>
public class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.ToTable("AccessRequests");

        // Primary Key
        builder.HasKey(ar => ar.Id);
        builder.Property(ar => ar.Id)
            .ValueGeneratedNever(); // Generated in domain

        // User relationship
        builder.HasOne(ar => ar.User)
            .WithMany()
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Reviewer relationship
        builder.HasOne(ar => ar.ReviewedByUser)
            .WithMany()
            .HasForeignKey(ar => ar.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Status with enum conversion
        builder.Property(ar => ar.Status)
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<AccessRequestStatus>(v))
            .IsRequired();

        // Add check constraint for valid status values
        builder.HasCheckConstraint(
            "CK_AccessRequests_Status",
            "\"Status\" IN ('Working', 'New', 'Accepted', 'Blocked', 'Updated')");

        // Timestamps
        builder.Property(ar => ar.SubmittedDate)
            .IsRequired(false);

        builder.Property(ar => ar.ReviewedDate)
            .IsRequired(false);

        builder.Property(ar => ar.CreatedDate)
            .IsRequired();

        builder.Property(ar => ar.UpdatedDate)
            .IsRequired();

        // Indexes
        builder.HasIndex(ar => ar.UserId)
            .HasDatabaseName("IX_AccessRequests_UserId");

        builder.HasIndex(ar => ar.Status)
            .HasDatabaseName("IX_AccessRequests_Status");

        builder.HasIndex(ar => new { ar.UserId, ar.Status })
            .HasDatabaseName("IX_AccessRequests_UserId_Status");
    }
}

