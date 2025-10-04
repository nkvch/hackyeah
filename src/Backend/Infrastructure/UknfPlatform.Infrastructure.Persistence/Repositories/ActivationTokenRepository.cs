using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ActivationToken entity
/// </summary>
public class ActivationTokenRepository : IActivationTokenRepository
{
    private readonly ApplicationDbContext _context;

    public ActivationTokenRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ActivationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.ActivationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task AddAsync(ActivationToken activationToken, CancellationToken cancellationToken = default)
    {
        await _context.ActivationTokens.AddAsync(activationToken, cancellationToken);
    }

    public Task UpdateAsync(ActivationToken activationToken, CancellationToken cancellationToken = default)
    {
        _context.ActivationTokens.Update(activationToken);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task InvalidateUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.ActivationTokens
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.MarkAsUsed();
        }
    }
}

