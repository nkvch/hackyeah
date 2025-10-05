using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Auth.Repositories;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for SetPasswordCommand
/// Sets initial password for newly activated user account
/// </summary>
public class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand, SetPasswordResponse>
{
    private readonly IActivationTokenRepository _activationTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly PasswordPolicySettings _passwordPolicy;
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<SetPasswordCommandHandler> _logger;

    public SetPasswordCommandHandler(
        IActivationTokenRepository activationTokenRepository,
        IUserRepository userRepository,
        IPasswordHistoryRepository passwordHistoryRepository,
        IAccessRequestRepository accessRequestRepository,
        IPasswordHasher passwordHasher,
        IOptions<PasswordPolicySettings> passwordPolicy,
        IOptions<ApplicationSettings> applicationSettings,
        ILogger<SetPasswordCommandHandler> logger)
    {
        _activationTokenRepository = activationTokenRepository ?? throw new ArgumentNullException(nameof(activationTokenRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHistoryRepository = passwordHistoryRepository ?? throw new ArgumentNullException(nameof(passwordHistoryRepository));
        _accessRequestRepository = accessRequestRepository ?? throw new ArgumentNullException(nameof(accessRequestRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _passwordPolicy = passwordPolicy?.Value ?? throw new ArgumentNullException(nameof(passwordPolicy));
        _applicationSettings = applicationSettings?.Value ?? throw new ArgumentNullException(nameof(applicationSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SetPasswordResponse> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing set password request");

        // 1. Validate token (reuse activation token from Story 1.2)
        var activationToken = await _activationTokenRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (activationToken == null)
        {
            _logger.LogWarning("Set password attempt with invalid token");
            throw new InvalidTokenException("Activation link is invalid");
        }

        if (!activationToken.IsValid())
        {
            _logger.LogWarning("Set password attempt with expired token for user {UserId}", activationToken.UserId);
            throw new TokenExpiredException("Activation link has expired. Please request a new one.");
        }

        if (activationToken.IsUsed)
        {
            _logger.LogWarning("Set password attempt with already used token for user {UserId}", activationToken.UserId);
            // If user already has password set, this is not an error - just inform them
            if (!string.IsNullOrEmpty(activationToken.User.PasswordHash))
            {
                _logger.LogInformation("User {UserId} already has password set", activationToken.User.Id);
                return new SetPasswordResponse(
                    activationToken.User.Id,
                    "Password has already been set for this account. Please proceed to login.",
                    $"{_applicationSettings.FrontendUrl}/auth/login");
            }
            throw new TokenAlreadyUsedException("This activation link has already been used");
        }

        var user = activationToken.User;

        // 2. Check if password was already set (idempotent check)
        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            _logger.LogInformation("Password already set for user {UserId}, returning success", user.Id);
            return new SetPasswordResponse(
                user.Id,
                "Password has already been set. You can now log in.",
                $"{_applicationSettings.FrontendUrl}/auth/login");
        }

        // 3. Check password history (prevent reuse)
        if (_passwordPolicy.PasswordHistoryCount > 0)
        {
            var recentPasswords = await _passwordHistoryRepository.GetRecentPasswordsAsync(
                user.Id, 
                _passwordPolicy.PasswordHistoryCount, 
                cancellationToken);

            foreach (var historicalPassword in recentPasswords)
            {
                if (_passwordHasher.VerifyPassword(request.Password, historicalPassword.PasswordHash))
                {
                    _logger.LogWarning("User {UserId} attempted to reuse a recent password", user.Id);
                    throw new InvalidOperationException(
                        $"Password was used recently. Please choose a different password. You cannot reuse your last {_passwordPolicy.PasswordHistoryCount} passwords.");
                }
            }
        }

        // 4. Hash the password
        var passwordHash = _passwordHasher.HashPassword(request.Password);
        _logger.LogDebug("Password hashed successfully for user {UserId}", user.Id);

        // 5. Update user with password
        user.SetPassword(passwordHash);
        user.Activate(); // Ensure user is active
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Password set successfully for user {UserId}", user.Id);

        // 5a. Story 2.1: Automatically create access request after activation
        var existingAccessRequest = await _accessRequestRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (existingAccessRequest == null)
        {
            var accessRequest = AccessRequest.CreateForUser(user.Id);
            await _accessRequestRepository.AddAsync(accessRequest, cancellationToken);
            
            _logger.LogInformation("Access request created automatically for user {UserId}, RequestId: {AccessRequestId}", 
                user.Id, accessRequest.Id);
        }
        else
        {
            _logger.LogInformation("Access request already exists for user {UserId}, skipping creation", user.Id);
        }

        // 6. Mark activation token as used
        activationToken.MarkAsUsed();
        await _activationTokenRepository.UpdateAsync(activationToken, cancellationToken);
        await _activationTokenRepository.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Activation token marked as used for user {UserId}", user.Id);

        // 7. Add to password history
        var passwordHistory = PasswordHistory.Create(user.Id, passwordHash);
        await _passwordHistoryRepository.AddAsync(passwordHistory, cancellationToken);
        await _passwordHistoryRepository.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Password added to history for user {UserId}", user.Id);

        return new SetPasswordResponse(
            user.Id,
            "Password set successfully. You can now log in.",
            $"{_applicationSettings.FrontendUrl}/auth/login");
    }
}

