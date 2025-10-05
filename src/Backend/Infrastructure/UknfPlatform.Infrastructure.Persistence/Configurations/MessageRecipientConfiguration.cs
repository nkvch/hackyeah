using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Communication.Entities;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

public class MessageRecipientConfiguration : IEntityTypeConfiguration<MessageRecipient>
{
    public void Configure(EntityTypeBuilder<MessageRecipient> builder)
    {
        builder.ToTable("MessageRecipients");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.MessageId)
            .IsRequired();

        builder.Property(r => r.RecipientUserId)
            .IsRequired();

        builder.Property(r => r.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.ReadDate)
            .IsRequired(false);

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.UpdatedDate)
            .IsRequired();

        // Indexes
        builder.HasIndex(r => r.MessageId);
        builder.HasIndex(r => new { r.RecipientUserId, r.IsRead });
    }
}

