using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;

namespace UknfPlatform.Infrastructure.Identity.Services;

/// <summary>
/// Email service implementation using MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private const int MaxRetries = 3;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendAccountActivationEmailAsync(
        string email,
        string firstName,
        string activationUrl,
        CancellationToken cancellationToken = default)
    {
        var subject = "Activate Your UKNF Account";
        var htmlBody = GetActivationEmailTemplate(firstName, activationUrl);
        var textBody = $"Hello {firstName},\n\nPlease activate your account by clicking this link: {activationUrl}\n\nThis link will expire in 24 hours.\n\nBest regards,\nUKNF Communication Platform";

        await SendEmailWithRetryAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(
        string email,
        string firstName,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your UKNF Account Password";
        var htmlBody = GetPasswordResetEmailTemplate(firstName, resetUrl);
        var textBody = $"Hello {firstName},\n\nYou requested to reset your password. Click this link: {resetUrl}\n\nThis link will expire in 1 hour.\n\nIf you didn't request this, please ignore this email.\n\nBest regards,\nUKNF Communication Platform";

        await SendEmailWithRetryAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendEmailChangeConfirmationAsync(
        string email,
        string firstName,
        string confirmationUrl,
        CancellationToken cancellationToken = default)
    {
        var subject = "Confirm Your New Email Address";
        var htmlBody = GetEmailChangeConfirmationTemplate(firstName, email, confirmationUrl);
        var textBody = $"Hello {firstName},\n\nYou requested to change your email address to {email}.\n\nPlease confirm this change by clicking the link: {confirmationUrl}\n\nThis link will expire in 24 hours.\n\nIf you didn't request this change, please ignore this email and contact our support.\n\nBest regards,\nUKNF Communication Platform";

        await SendEmailWithRetryAsync(email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task SendNewMessageNotificationAsync(
        string email,
        string senderName,
        string subject,
        CancellationToken cancellationToken = default)
    {
        var emailSubject = $"New Message from {senderName}";
        var htmlBody = $@"
            <h2>New Message Received</h2>
            <p>You have received a new message from <strong>{senderName}</strong></p>
            <p><strong>Subject:</strong> {subject}</p>
            <p>Please log in to the UKNF Communication Platform to view and respond to this message.</p>
            <p><a href='http://localhost:4200/messages'>View Message</a></p>
        ";
        var textBody = $"You have received a new message from {senderName}.\n\nSubject: {subject}\n\nPlease log in to view and respond.";

        await SendEmailWithRetryAsync(email, emailSubject, htmlBody, textBody, cancellationToken);
    }

    private async Task SendEmailWithRetryAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string textBody,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < MaxRetries)
        {
            attempt++;
            try
            {
                _logger.LogInformation("Sending email to {Email}, attempt {Attempt}/{MaxRetries}", toEmail, attempt, MaxRetries);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = htmlBody,
                    TextBody = textBody
                };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Connect to SMTP server
                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    _emailSettings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None,
                    cancellationToken);

                // Authenticate if credentials are provided
                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password, cancellationToken);
                }

                // Send the message
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return; // Success!
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Failed to send email to {Email} on attempt {Attempt}/{MaxRetries}", toEmail, attempt, MaxRetries);
                
                // Exponential backoff: 1s, 2s, 4s
                var delaySeconds = Math.Pow(2, attempt - 1);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(ex, "Failed to send email to {Email} after {MaxRetries} attempts", toEmail, MaxRetries);
            }
        }

        // If we get here, all retries failed
        throw new InvalidOperationException(
            $"Failed to send email to {toEmail} after {MaxRetries} attempts. See inner exception for details.",
            lastException);
    }

    private string GetActivationEmailTemplate(string firstName, string activationUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""pl"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Activate Your Account</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td align=""center"" style=""padding: 40px 40px 20px 40px;"">
                            <h1 style=""color: #1e40af; margin: 0; font-size: 28px;"">UKNF Communication Platform</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 20px 40px;"">
                            <h2 style=""color: #333333; margin: 0 0 20px 0; font-size: 24px;"">Welcome, {firstName}!</h2>
                            <p style=""color: #666666; line-height: 1.6; margin: 0 0 20px 0;"">
                                Thank you for registering for the UKNF Communication Platform. 
                                To complete your registration and access your account, please activate your account by clicking the button below.
                            </p>
                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"" style=""padding: 30px 0;"">
                                        <a href=""{activationUrl}"" style=""background-color: #1e40af; color: #ffffff; padding: 15px 40px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;"">Activate Account</a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #666666; line-height: 1.6; margin: 0 0 10px 0; font-size: 14px;"">
                                Or copy and paste this link into your browser:
                            </p>
                            <p style=""color: #1e40af; line-height: 1.6; margin: 0 0 20px 0; font-size: 12px; word-break: break-all;"">
                                {activationUrl}
                            </p>
                            <div style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0;"">
                                <p style=""color: #92400e; margin: 0; font-size: 14px;"">
                                    <strong>⚠️ Important:</strong> This activation link will expire in 24 hours.
                                </p>
                            </div>
                            <div style=""background-color: #fee2e2; border-left: 4px solid #ef4444; padding: 15px; margin: 20px 0;"">
                                <p style=""color: #991b1b; margin: 0; font-size: 14px;"">
                                    <strong>🔒 Security:</strong> Do not share this activation link with anyone. 
                                    It is unique to your account.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 20px 40px; background-color: #f9fafb; border-top: 1px solid #e5e7eb;"">
                            <p style=""color: #6b7280; margin: 0; font-size: 12px; line-height: 1.5;"">
                                If you didn't create an account, please ignore this email or contact our support team.
                            </p>
                            <p style=""color: #6b7280; margin: 10px 0 0 0; font-size: 12px; line-height: 1.5;"">
                                <strong>Support:</strong> <a href=""mailto:support@uknf.gov.pl"" style=""color: #1e40af;"">support@uknf.gov.pl</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private string GetPasswordResetEmailTemplate(string firstName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""pl"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Your Password</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td align=""center"" style=""padding: 40px 40px 20px 40px;"">
                            <h1 style=""color: #1e40af; margin: 0; font-size: 28px;"">UKNF Communication Platform</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 20px 40px;"">
                            <h2 style=""color: #333333; margin: 0 0 20px 0; font-size: 24px;"">Password Reset Request</h2>
                            <p style=""color: #666666; line-height: 1.6; margin: 0 0 20px 0;"">
                                Hello {firstName},
                            </p>
                            <p style=""color: #666666; line-height: 1.6; margin: 0 0 20px 0;"">
                                We received a request to reset your password. Click the button below to create a new password.
                            </p>
                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"" style=""padding: 30px 0;"">
                                        <a href=""{resetUrl}"" style=""background-color: #1e40af; color: #ffffff; padding: 15px 40px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;"">Reset Password</a>
                                    </td>
                                </tr>
                            </table>
                            <div style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0;"">
                                <p style=""color: #92400e; margin: 0; font-size: 14px;"">
                                    <strong>⚠️ Important:</strong> This reset link will expire in 1 hour.
                                </p>
                            </div>
                            <div style=""background-color: #fee2e2; border-left: 4px solid #ef4444; padding: 15px; margin: 20px 0;"">
                                <p style=""color: #991b1b; margin: 0; font-size: 14px;"">
                                    <strong>🔒 Security:</strong> If you didn't request a password reset, please ignore this email. Your password will remain unchanged.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 20px 40px; background-color: #f9fafb; border-top: 1px solid #e5e7eb;"">
                            <p style=""color: #6b7280; margin: 0; font-size: 12px; line-height: 1.5;"">
                                <strong>Support:</strong> <a href=""mailto:support@uknf.gov.pl"" style=""color: #1e40af;"">support@uknf.gov.pl</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private string GetEmailChangeConfirmationTemplate(string firstName, string newEmail, string confirmationUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""pl"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Confirm Email Change</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td align=""center"" style=""padding: 40px 40px 20px 40px;"">
                            <h1 style=""color: #1e40af; margin: 0; font-size: 28px;"">UKNF Communication Platform</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 20px 40px;"">
                            <h2 style=""color: #333333; margin: 0 0 20px 0; font-size: 24px;"">Confirm Your New Email Address</h2>
                            <p style=""color: #666666; line-height: 1.6; margin: 0 0 20px 0;"">
                                Hello {firstName},
                            </p>
                            <p style=""color: #666666; line-height: 1.6; margin: 0 0 20px 0;"">
                                You requested to change your email address to <strong>{newEmail}</strong>.
                            </p>
                            <p style=""color: #666666; line-height: 1.6; margin: 0 0 20px 0;"">
                                To complete this change, please confirm your new email address by clicking the button below.
                            </p>
                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"" style=""padding: 30px 0;"">
                                        <a href=""{confirmationUrl}"" style=""background-color: #1e40af; color: #ffffff; padding: 15px 40px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;"">Confirm Email Change</a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #666666; line-height: 1.6; margin: 0 0 10px 0; font-size: 14px;"">
                                Or copy and paste this link into your browser:
                            </p>
                            <p style=""color: #1e40af; line-height: 1.6; margin: 0 0 20px 0; font-size: 12px; word-break: break-all;"">
                                {confirmationUrl}
                            </p>
                            <div style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0;"">
                                <p style=""color: #92400e; margin: 0; font-size: 14px;"">
                                    <strong>⚠️ Important:</strong> This confirmation link will expire in 24 hours.
                                </p>
                            </div>
                            <div style=""background-color: #fee2e2; border-left: 4px solid #ef4444; padding: 15px; margin: 20px 0;"">
                                <p style=""color: #991b1b; margin: 0; font-size: 14px;"">
                                    <strong>🔒 Security:</strong> If you didn't request this email change, please ignore this email and contact our support immediately. Your current email address will remain unchanged.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 20px 40px; background-color: #f9fafb; border-top: 1px solid #e5e7eb;"">
                            <p style=""color: #6b7280; margin: 0; font-size: 12px; line-height: 1.5;"">
                                <strong>Support:</strong> <a href=""mailto:support@uknf.gov.pl"" style=""color: #1e40af;"">support@uknf.gov.pl</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}

