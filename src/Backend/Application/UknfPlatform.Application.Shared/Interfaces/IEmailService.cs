namespace UknfPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an account activation email to the user
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">User's first name for personalization</param>
    /// <param name="activationUrl">Activation URL with token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAccountActivationEmailAsync(
        string email,
        string firstName,
        string activationUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email to the user
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="firstName">User's first name for personalization</param>
    /// <param name="resetUrl">Password reset URL with token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendPasswordResetEmailAsync(
        string email,
        string firstName,
        string resetUrl,
        CancellationToken cancellationToken = default);
}

