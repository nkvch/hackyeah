using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for RegisterUserCommand
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IActivationTokenRepository _activationTokenRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IEmailService _emailService;
    private readonly ApplicationSettings _applicationSettings;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IActivationTokenRepository activationTokenRepository,
        IEncryptionService encryptionService,
        IEmailService emailService,
        IOptions<ApplicationSettings> applicationSettings,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _activationTokenRepository = activationTokenRepository ?? throw new ArgumentNullException(nameof(activationTokenRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _applicationSettings = applicationSettings?.Value ?? throw new ArgumentNullException(nameof(applicationSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing user registration for email");

        // Check if email already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("Registration attempt with existing email");
            throw new InvalidOperationException("A user with this email already exists");
        }

        // Encrypt PESEL and extract last 4 digits
        var peselEncrypted = _encryptionService.Encrypt(request.Pesel);
        var peselLast4 = request.Pesel.Substring(request.Pesel.Length - 4);

        // Check if PESEL already exists
        if (await _userRepository.ExistsByPeselAsync(peselEncrypted, cancellationToken))
        {
            _logger.LogWarning("Registration attempt with existing PESEL");
            throw new InvalidOperationException("A user with this PESEL already exists");
        }

        // Create user entity
        var user = User.CreateExternal(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            peselEncrypted,
            peselLast4
        );

        // Save user to repository
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully with ID {UserId}", user.Id);

        // Generate activation token
        var activationToken = ActivationToken.Create(user.Id, expirationHours: 24);
        await _activationTokenRepository.AddAsync(activationToken, cancellationToken);
        await _activationTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activation token generated for user {UserId}", user.Id);

        // Generate activation URL
        var activationUrl = $"{_applicationSettings.FrontendUrl}/set-password?token={activationToken.Token}";

        // Send activation email
        try
        {
            await _emailService.SendAccountActivationEmailAsync(
                user.Email,
                user.FirstName,
                activationUrl,
                cancellationToken);

            _logger.LogInformation("Activation email sent to {Email} for user {UserId}", user.Email, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send activation email to {Email} for user {UserId}", user.Email, user.Id);
            // Don't fail registration if email fails - user can request resend
        }

        return new RegisterUserResponse(
            user.Id,
            "Registration successful. Please check your email to activate your account."
        );
    }
}

