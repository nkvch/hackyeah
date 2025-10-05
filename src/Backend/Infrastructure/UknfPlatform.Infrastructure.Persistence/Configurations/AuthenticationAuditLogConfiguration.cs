using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for AuthenticationAuditLog entity
/// </summary>
public class AuthenticationAuditLogConfiguration : IEntityTypeConfiguration<AuthenticationAuditLog>
{
    public void Configure(EntityTypeBuilder<AuthenticationAuditLog> builder)
    {
        builder.ToTable("AuthenticationAuditLogs");

        builder.HasKey(aal => aal.Id);

        builder.Property(aal => aal.Id)
            .IsRequired()
            .ValueGeneratedNever(); // Guid generated in domain

        builder.Property(aal => aal.UserId);

        builder.Property(aal => aal.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(aal => aal.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // Max length for IPv6

        builder.Property(aal => aal.UserAgent)
            .HasMaxLength(500);

        builder.Property(aal => aal.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(aal => aal.Success)
            .IsRequired();

        builder.Property(aal => aal.FailureReason)
            .HasMaxLength(200);

        builder.Property(aal => aal.Timestamp)
            .IsRequired();

        builder.Property(aal => aal.CreatedDate)
            .IsRequired();

        builder.Property(aal => aal.UpdatedDate)
            .IsRequired();

        // Indexes for efficient audit queries
        builder.HasIndex(aal => aal.Timestamp)
            .HasDatabaseName("IX_AuthenticationAuditLogs_Timestamp");

        builder.HasIndex(aal => aal.Email)
            .HasDatabaseName("IX_AuthenticationAuditLogs_Email");

        builder.HasIndex(aal => aal.Success)
            .HasDatabaseName("IX_AuthenticationAuditLogs_Success");

        builder.HasIndex(aal => new { aal.Email, aal.Timestamp })
            .HasDatabaseName("IX_AuthenticationAuditLogs_Email_Timestamp");

        // Foreign key to User (nullable)
        builder.HasOne(aal => aal.User)
            .WithMany()
            .HasForeignKey(aal => aal.UserId)
            .OnDelete(DeleteBehavior.SetNull); // Keep audit log if user deleted
    }
}

