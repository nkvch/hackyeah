using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Communication.Entities;

/// <summary>
/// File attachment for a message
/// Story 5.1: Messages can have multiple file attachments
/// </summary>
public class MessageAttachment : BaseEntity
{
    public Guid MessageId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileStorageKey { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public DateTime UploadedDate { get; private set; }
    
    // Navigation property
    public Message Message { get; private set; } = null!;
    
    private MessageAttachment() { } // EF Core constructor
    
    /// <summary>
    /// Creates a new message attachment
    /// </summary>
    public static MessageAttachment Create(
        Guid messageId,
        string fileName,
        string fileStorageKey,
        long fileSize,
        string contentType)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("MessageId is required", nameof(messageId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName cannot be empty", nameof(fileName));
        if (string.IsNullOrWhiteSpace(fileStorageKey))
            throw new ArgumentException("FileStorageKey cannot be empty", nameof(fileStorageKey));
        if (fileSize <= 0)
            throw new ArgumentException("FileSize must be positive", nameof(fileSize));
        
        return new MessageAttachment
        {
            MessageId = messageId,
            FileName = fileName,
            FileStorageKey = fileStorageKey,
            FileSize = fileSize,
            ContentType = contentType ?? "application/octet-stream",
            UploadedDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }
}

