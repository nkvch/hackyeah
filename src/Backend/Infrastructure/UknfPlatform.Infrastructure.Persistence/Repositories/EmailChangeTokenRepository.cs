using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for EmailChangeToken entity
/// </summary>
public class EmailChangeTokenRepository : IEmailChangeTokenRepository
{
    private readonly ApplicationDbContext _context;

    public EmailChangeTokenRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<EmailChangeToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.EmailChangeTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<EmailChangeToken>> GetValidTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await _context.EmailChangeTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > now)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EmailChangeToken token, CancellationToken cancellationToken = default)
    {
        await _context.EmailChangeTokens.AddAsync(token, cancellationToken);
    }

    public Task UpdateAsync(EmailChangeToken token, CancellationToken cancellationToken = default)
    {
        _context.EmailChangeTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task InvalidateAllTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.EmailChangeTokens
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.MarkAsUsed();
        }
    }
}

