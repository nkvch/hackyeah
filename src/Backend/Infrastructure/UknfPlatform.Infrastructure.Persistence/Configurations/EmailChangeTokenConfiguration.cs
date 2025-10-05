using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for EmailChangeToken entity
/// </summary>
public class EmailChangeTokenConfiguration : IEntityTypeConfiguration<EmailChangeToken>
{
    public void Configure(EntityTypeBuilder<EmailChangeToken> builder)
    {
        builder.ToTable("EmailChangeTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.NewEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.IsUsed)
            .IsRequired();

        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.UpdatedDate)
            .IsRequired();

        // Foreign key relationship
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(t => new { t.UserId, t.ExpiresAt });
        builder.HasIndex(t => t.IsUsed);
    }
}

