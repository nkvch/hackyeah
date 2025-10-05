using UknfPlatform.Domain.Shared.Common;

namespace UknfPlatform.Domain.Auth.Entities;

/// <summary>
/// Authentication audit log for security monitoring and compliance
/// Tracks all authentication attempts (successful and failed)
/// </summary>
public class AuthenticationAuditLog : BaseEntity
{
    /// <summary>
    /// User ID if user was found (nullable for failed logins with non-existent email)
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Email address used in login attempt
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// IP address of the client making the request
    /// </summary>
    public string IpAddress { get; private set; }

    /// <summary>
    /// User-Agent header from the HTTP request
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Action type (Login, Logout, FailedLogin, TokenRefresh, etc.)
    /// </summary>
    public string Action { get; private set; }

    /// <summary>
    /// Whether the authentication attempt was successful
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Reason for failure (e.g., "InvalidPassword", "AccountNotActivated", "TokenExpired")
    /// Null if Success = true
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Timestamp of the authentication attempt
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Navigation property to User (nullable)
    /// </summary>
    public User? User { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private AuthenticationAuditLog()
    {
        Email = string.Empty;
        IpAddress = string.Empty;
        Action = string.Empty;
    }

    /// <summary>
    /// Factory method to create a successful authentication audit log
    /// </summary>
    public static AuthenticationAuditLog CreateSuccess(
        Guid userId,
        string email,
        string ipAddress,
        string? userAgent,
        string action)
    {
        ValidateParameters(email, ipAddress, action);

        return new AuthenticationAuditLog
        {
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Action = action,
            Success = true,
            FailureReason = null,
            Timestamp = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a failed authentication audit log
    /// </summary>
    public static AuthenticationAuditLog CreateFailure(
        Guid? userId,
        string email,
        string ipAddress,
        string? userAgent,
        string action,
        string failureReason)
    {
        ValidateParameters(email, ipAddress, action);

        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason cannot be null or empty", nameof(failureReason));

        return new AuthenticationAuditLog
        {
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Action = action,
            Success = false,
            FailureReason = failureReason,
            Timestamp = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }

    private static void ValidateParameters(string email, string ipAddress, string action)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP address cannot be null or empty", nameof(ipAddress));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or empty", nameof(action));
    }
}

/// <summary>
/// Constants for authentication audit log actions
/// </summary>
public static class AuthenticationAction
{
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string FailedLogin = "FailedLogin";
    public const string TokenRefresh = "TokenRefresh";
    public const string PasswordReset = "PasswordReset";
}

