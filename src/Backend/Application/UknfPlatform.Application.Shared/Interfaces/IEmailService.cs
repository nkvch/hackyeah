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

    /// <summary>
    /// Sends an email change confirmation email to the new email address
    /// </summary>
    /// <param name="email">New email address to confirm</param>
    /// <param name="firstName">User's first name for personalization</param>
    /// <param name="confirmationUrl">Email change confirmation URL with token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailChangeConfirmationAsync(
        string email,
        string firstName,
        string confirmationUrl,
        CancellationToken cancellationToken = default);
}

