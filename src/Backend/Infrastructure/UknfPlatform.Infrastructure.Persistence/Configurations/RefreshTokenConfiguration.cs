using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for RefreshToken entity
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .IsRequired()
            .ValueGeneratedNever(); // Guid generated in domain

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rt => rt.RevokedDate);

        builder.Property(rt => rt.CreatedDate)
            .IsRequired();

        builder.Property(rt => rt.UpdatedDate)
            .IsRequired();

        // Composite index for efficient queries (find valid tokens for a user)
        builder.HasIndex(rt => new { rt.UserId, rt.ExpiresAt })
            .HasDatabaseName("IX_RefreshTokens_UserId_ExpiresAt");

        // Foreign key to User
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

