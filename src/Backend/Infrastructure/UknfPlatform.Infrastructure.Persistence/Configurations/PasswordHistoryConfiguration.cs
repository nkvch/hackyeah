using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PasswordHistory entity
/// </summary>
public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.ToTable("PasswordHistories");

        builder.HasKey(ph => ph.Id);

        builder.Property(ph => ph.Id)
            .IsRequired();

        builder.Property(ph => ph.UserId)
            .IsRequired();

        builder.Property(ph => ph.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ph => ph.CreatedDate)
            .IsRequired();

        // Foreign key to User
        builder.HasOne(ph => ph.User)
            .WithMany() // User doesn't need navigation to password history
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for efficient lookups by user and date
        builder.HasIndex(ph => new { ph.UserId, ph.CreatedDate })
            .HasDatabaseName("IX_PasswordHistory_UserId_CreatedDate");
    }
}

