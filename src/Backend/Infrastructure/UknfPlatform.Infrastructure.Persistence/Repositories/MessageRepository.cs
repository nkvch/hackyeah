using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;
using UknfPlatform.Domain.Communication.Repositories;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Message entity
/// Story 5.1, 5.2: Message persistence
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _context;

    public MessageRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.Recipients)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<List<Message>> GetByRecipientUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.Recipients)
            .Where(m => m.Recipients.Any(r => r.RecipientUserId == userId))
            .OrderByDescending(m => m.SentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> GetBySenderIdAsync(Guid senderId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.Recipients)
            .Where(m => m.SenderId == senderId)
            .OrderByDescending(m => m.SentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> GetByContextAsync(MessageContextType contextType, Guid contextId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.Recipients)
            .Where(m => m.ContextType == contextType && m.ContextId == contextId)
            .OrderBy(m => m.SentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> GetMessageThreadAsync(Guid parentMessageId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.Recipients)
            .Where(m => m.ParentMessageId == parentMessageId)
            .OrderBy(m => m.SentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        await _context.Messages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

