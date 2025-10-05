using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Communication.Entities;

/// <summary>
/// Junction entity tracking message recipients and their read status
/// Story 5.1: Messages can have multiple recipients
/// Story 5.2: Track read status per recipient
/// </summary>
public class MessageRecipient : BaseEntity
{
    public Guid MessageId { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadDate { get; private set; }
    
    // Navigation properties
    public Message Message { get; private set; } = null!;
    
    private MessageRecipient() { } // EF Core constructor
    
    /// <summary>
    /// Creates a new message recipient
    /// </summary>
    public static MessageRecipient Create(Guid messageId, Guid recipientUserId)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("MessageId is required", nameof(messageId));
        if (recipientUserId == Guid.Empty)
            throw new ArgumentException("RecipientUserId is required", nameof(recipientUserId));
        
        return new MessageRecipient
        {
            MessageId = messageId,
            RecipientUserId = recipientUserId,
            IsRead = false,
            ReadDate = null,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Marks the message as read by this recipient
    /// </summary>
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}

