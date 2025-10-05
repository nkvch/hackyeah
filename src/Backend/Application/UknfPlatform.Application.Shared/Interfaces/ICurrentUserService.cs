namespace UknfPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service for retrieving current authenticated user information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Gets the current user's full name
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// Gets the current user's phone number
    /// </summary>
    string? Phone { get; }

    /// <summary>
    /// Checks if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if user has a specific permission for an entity
    /// </summary>
    Task<bool> HasPermissionAsync(string permission, long? entityId = null, CancellationToken cancellationToken = default);
}

