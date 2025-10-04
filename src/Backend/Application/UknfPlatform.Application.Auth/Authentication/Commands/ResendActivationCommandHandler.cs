using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for ResendActivationCommand
/// </summary>
public class ResendActivationCommandHandler : IRequestHandler<ResendActivationCommand, ResendActivationResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IActivationTokenRepository _activationTokenRepository;
    private readonly IEmailService _emailService;
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<ResendActivationCommandHandler> _logger;

    public ResendActivationCommandHandler(
        IUserRepository userRepository,
        IActivationTokenRepository activationTokenRepository,
        IEmailService emailService,
        IOptions<ApplicationSettings> applicationSettings,
        ILogger<ResendActivationCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _activationTokenRepository = activationTokenRepository ?? throw new ArgumentNullException(nameof(activationTokenRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _applicationSettings = applicationSettings?.Value ?? throw new ArgumentNullException(nameof(applicationSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ResendActivationResponse> Handle(ResendActivationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing resend activation request for email");

        // Find user by email (case-insensitive)
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Return generic success message even if user not found (security: don't reveal user existence)
        if (user == null)
        {
            _logger.LogInformation("Resend activation requested for non-existent email");
            return new ResendActivationResponse(
                "If that email is registered and not yet activated, we've sent a new activation link.");
        }

        // If user is already active, return success (don't reveal account status)
        if (user.IsActive)
        {
            _logger.LogInformation("Resend activation requested for already active user {UserId}", user.Id);
            return new ResendActivationResponse(
                "If that email is registered and not yet activated, we've sent a new activation link.");
        }

        // Invalidate all previous unused tokens for this user
        await _activationTokenRepository.InvalidateUserTokensAsync(user.Id, cancellationToken);
        await _activationTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Previous activation tokens invalidated for user {UserId}", user.Id);

        // Generate new activation token
        var activationToken = ActivationToken.Create(user.Id, expirationHours: 24);
        await _activationTokenRepository.AddAsync(activationToken, cancellationToken);
        await _activationTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New activation token generated for user {UserId}", user.Id);

        // Generate activation URL
        var activationUrl = $"{_applicationSettings.FrontendUrl}/auth/activate?token={activationToken.Token}";

        // Send activation email
        try
        {
            await _emailService.SendAccountActivationEmailAsync(
                user.Email,
                user.FirstName,
                activationUrl,
                cancellationToken);

            _logger.LogInformation("New activation email sent to {Email} for user {UserId}", user.Email, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new activation email to {Email} for user {UserId}", user.Email, user.Id);
            // Don't fail the request if email fails - return success anyway
        }

        return new ResendActivationResponse(
            "If that email is registered and not yet activated, we've sent a new activation link.");
    }
}

