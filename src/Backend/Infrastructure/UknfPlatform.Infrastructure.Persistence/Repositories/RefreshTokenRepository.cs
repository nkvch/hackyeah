using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for RefreshToken entity
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        if (refreshToken == null)
            throw new ArgumentNullException(nameof(refreshToken));

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > now)
            .OrderByDescending(rt => rt.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        if (refreshToken == null)
            throw new ArgumentNullException(nameof(refreshToken));

        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in userTokens)
        {
            token.Revoke();
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

