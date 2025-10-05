using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Body)
            .IsRequired()
            .HasMaxLength(10000);

        builder.Property(m => m.SenderId)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.ContextType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.ContextId)
            .IsRequired(false);

        builder.Property(m => m.ParentMessageId)
            .IsRequired(false);

        builder.Property(m => m.SentDate)
            .IsRequired();

        builder.Property(m => m.CreatedDate)
            .IsRequired();

        builder.Property(m => m.UpdatedDate)
            .IsRequired();

        // Relationships
        builder.HasMany(m => m.Attachments)
            .WithOne(a => a.Message)
            .HasForeignKey(a => a.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Recipients)
            .WithOne(r => r.Message)
            .HasForeignKey(r => r.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => new { m.ContextType, m.ContextId });
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.SentDate);
    }
}

