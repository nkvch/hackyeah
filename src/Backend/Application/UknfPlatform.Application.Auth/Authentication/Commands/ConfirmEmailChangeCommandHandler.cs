using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for ConfirmEmailChangeCommand - confirms and applies email change
/// </summary>
public class ConfirmEmailChangeCommandHandler : IRequestHandler<ConfirmEmailChangeCommand, ConfirmEmailChangeResponse>
{
    private readonly IEmailChangeTokenRepository _emailChangeTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ConfirmEmailChangeCommandHandler> _logger;

    public ConfirmEmailChangeCommandHandler(
        IEmailChangeTokenRepository emailChangeTokenRepository,
        IUserRepository userRepository,
        ILogger<ConfirmEmailChangeCommandHandler> logger)
    {
        _emailChangeTokenRepository = emailChangeTokenRepository ?? throw new ArgumentNullException(nameof(emailChangeTokenRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConfirmEmailChangeResponse> Handle(ConfirmEmailChangeCommand command, CancellationToken cancellationToken)
    {
        // Validate token exists
        var emailChangeToken = await _emailChangeTokenRepository.GetByTokenAsync(command.Token, cancellationToken)
            ?? throw new InvalidTokenException("Email change confirmation token is invalid");

        // Validate token not expired
        if (emailChangeToken.IsExpired())
        {
            throw new TokenExpiredException("Email change confirmation link has expired. Please request a new email change.");
        }

        // Validate token not already used
        if (emailChangeToken.IsUsed)
        {
            throw new TokenAlreadyUsedException("Email change confirmation link has already been used");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(emailChangeToken.UserId, cancellationToken)
            ?? throw new NotFoundException("User", emailChangeToken.UserId);

        // Verify the new email is still the one pending
        if (user.PendingEmail != emailChangeToken.NewEmail)
        {
            throw new InvalidOperationException("Email change request has been cancelled or superseded");
        }

        // Verify new email still not in use by another user
        var existingUser = await _userRepository.GetByEmailAsync(emailChangeToken.NewEmail, cancellationToken);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            throw new ConflictException($"Email {emailChangeToken.NewEmail} is now in use by another account. Please choose a different email.");
        }

        // Apply the email change
        user.ConfirmEmailChange();

        // Mark token as used
        emailChangeToken.MarkAsUsed();

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _emailChangeTokenRepository.UpdateAsync(emailChangeToken, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        await _emailChangeTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Email changed successfully for user {UserId}. New email: {NewEmail}",
            user.Id,
            user.Email);

        return new ConfirmEmailChangeResponse(
            Message: "Email changed successfully! You can now log in with your new email address.",
            NewEmail: user.Email
        );
    }
}

