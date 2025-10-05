using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;

namespace UknfPlatform.Domain.Communication.Repositories;

/// <summary>
/// Repository for managing messages
/// Story 5.1, 5.2: Message persistence
/// </summary>
public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<List<Message>> GetByRecipientUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<List<Message>> GetBySenderIdAsync(Guid senderId, CancellationToken cancellationToken = default);
    
    Task<List<Message>> GetByContextAsync(MessageContextType contextType, Guid contextId, CancellationToken cancellationToken = default);
    
    Task<List<Message>> GetMessageThreadAsync(Guid parentMessageId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets messages for a recipient with pagination
    /// Story 5.2: Receive and View Messages
    /// </summary>
    Task<(List<Message> Messages, int TotalCount)> GetMessagesForRecipientAsync(
        Guid recipientUserId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets message detail with verification that user is a recipient
    /// Story 5.2: Receive and View Messages
    /// </summary>
    Task<Message?> GetMessageDetailForRecipientAsync(
        Guid messageId, 
        Guid recipientUserId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets recipient record for marking as read
    /// Story 5.2: Receive and View Messages
    /// </summary>
    Task<MessageRecipient?> GetRecipientAsync(
        Guid messageId, 
        Guid recipientUserId, 
        CancellationToken cancellationToken = default);
    
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Message message, CancellationToken cancellationToken = default);
}

