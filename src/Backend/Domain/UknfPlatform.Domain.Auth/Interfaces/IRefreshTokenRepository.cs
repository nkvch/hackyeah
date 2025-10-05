using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Domain.Auth.Interfaces;

/// <summary>
/// Repository interface for RefreshToken entity operations
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Adds a new refresh token to the database
    /// </summary>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a refresh token by its token string
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all valid (not revoked, not expired) refresh tokens for a user
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing refresh token
    /// </summary>
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user (e.g., security incident)
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

