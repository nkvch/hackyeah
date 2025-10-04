using UknfPlatform.Domain.Auth.Entities;

namespace UknfPlatform.Domain.Auth.Interfaces;

/// <summary>
/// Repository interface for ActivationToken entity
/// </summary>
public interface IActivationTokenRepository
{
    /// <summary>
    /// Gets an activation token by its token string, including the associated User
    /// </summary>
    /// <param name="token">Token string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ActivationToken if found, null otherwise</returns>
    Task<ActivationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new activation token
    /// </summary>
    /// <param name="activationToken">Activation token to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(ActivationToken activationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing activation token
    /// </summary>
    /// <param name="activationToken">Activation token to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(ActivationToken activationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all unused tokens for a user as used (for resend activation scenarios)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}

