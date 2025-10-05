using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UknfPlatform.Domain.Communication.Entities;

namespace UknfPlatform.Infrastructure.Persistence.Configurations;

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.ToTable("MessageAttachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.MessageId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.FileStorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.FileSize)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasMaxLength(100);

        builder.Property(a => a.UploadedDate)
            .IsRequired();

        builder.Property(a => a.CreatedDate)
            .IsRequired();

        builder.Property(a => a.UpdatedDate)
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.MessageId);
    }
}

