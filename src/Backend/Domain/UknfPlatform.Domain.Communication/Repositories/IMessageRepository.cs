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
    
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Message message, CancellationToken cancellationToken = default);
}

