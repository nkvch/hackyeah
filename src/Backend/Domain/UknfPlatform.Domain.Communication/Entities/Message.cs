using UknfPlatform.Domain.Communication.Enums;
using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Communication.Entities;

/// <summary>
/// Message entity for communication between UKNF employees and external users
/// Story 5.1: Send Message with Attachments
/// </summary>
public class Message : BaseEntity
{
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public Guid SenderId { get; private set; }
    public MessageStatus Status { get; private set; }
    public MessageContextType ContextType { get; private set; }
    public Guid? ContextId { get; private set; }
    public Guid? ParentMessageId { get; private set; }
    public DateTime SentDate { get; private set; }
    
    // Navigation properties
    public ICollection<MessageAttachment> Attachments { get; private set; } = new List<MessageAttachment>();
    public ICollection<MessageRecipient> Recipients { get; private set; } = new List<MessageRecipient>();
    
    private Message() { } // EF Core constructor
    
    /// <summary>
    /// Creates a new message
    /// </summary>
    public static Message Create(
        string subject,
        string body,
        Guid senderId,
        bool senderIsInternal,
        MessageContextType contextType,
        Guid? contextId = null,
        Guid? parentMessageId = null)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be empty", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be empty", nameof(body));
        if (senderId == Guid.Empty)
            throw new ArgumentException("SenderId is required", nameof(senderId));
        if (contextType != MessageContextType.Standalone && contextId == null)
            throw new ArgumentException("ContextId is required when ContextType is not Standalone", nameof(contextId));
        
        // Determine status based on sender type
        var status = senderIsInternal 
            ? MessageStatus.AwaitingUserResponse 
            : MessageStatus.AwaitingUknfResponse;
        
        return new Message
        {
            Subject = subject,
            Body = body,
            SenderId = senderId,
            Status = status,
            ContextType = contextType,
            ContextId = contextId,
            ParentMessageId = parentMessageId,
            SentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Adds an attachment to the message
    /// </summary>
    public void AddAttachment(MessageAttachment attachment)
    {
        if (attachment == null)
            throw new ArgumentNullException(nameof(attachment));
        
        Attachments.Add(attachment);
        UpdatedDate = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Adds a recipient to the message
    /// </summary>
    public void AddRecipient(MessageRecipient recipient)
    {
        if (recipient == null)
            throw new ArgumentNullException(nameof(recipient));
        
        Recipients.Add(recipient);
        UpdatedDate = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Closes the message (marks as closed after reply)
    /// </summary>
    public void Close()
    {
        Status = MessageStatus.Closed;
        UpdatedDate = DateTime.UtcNow;
    }
}

