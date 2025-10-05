using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Application.Shared.Settings;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for LoginCommand - authenticates user and generates JWT tokens
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthenticationAuditLogRepository _auditLogRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly PasswordPolicySettings _passwordPolicySettings;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuthenticationAuditLogRepository auditLogRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IOptions<PasswordPolicySettings> passwordPolicySettings,
        IOptions<JwtSettings> jwtSettings,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _passwordPolicySettings = passwordPolicySettings?.Value ?? throw new ArgumentNullException(nameof(passwordPolicySettings));
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing login request for email: {Email}", request.Email);

        // TODO: Extract IP/UserAgent from HTTP context via middleware or custom attribute
        // For now, use placeholder values (audit logging is not critical for Story 1.4 core functionality)
        var ipAddress = "Unknown";
        string? userAgent = null;

        try
        {
            // 1. Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                // Don't reveal that user doesn't exist (security best practice)
                _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                await LogFailedAttemptAsync(null, request.Email, ipAddress, userAgent, "UserNotFound", cancellationToken);
                throw new InvalidCredentialsException();
            }

            // 2. Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: Account not activated for user {UserId}", user.Id);
                await LogFailedAttemptAsync(user.Id, request.Email, ipAddress, userAgent, "AccountNotActivated", cancellationToken);
                throw new AccountNotActivatedException();
            }

            // 3. Verify password
            if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                // Don't reveal whether email exists or password is wrong (security best practice)
                _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
                await LogFailedAttemptAsync(user.Id, request.Email, ipAddress, userAgent, "InvalidPassword", cancellationToken);
                throw new InvalidCredentialsException();
            }

            // 4. Check if password is expired
            bool requiresPasswordChange = false;
            if (user.LastPasswordChangeDate.HasValue && _passwordPolicySettings.PasswordExpirationDays > 0)
            {
                var passwordAge = DateTime.UtcNow - user.LastPasswordChangeDate.Value;
                if (passwordAge.TotalDays > _passwordPolicySettings.PasswordExpirationDays)
                {
                    _logger.LogInformation("Password expired for user {UserId}. Age: {Days} days", user.Id, passwordAge.TotalDays);
                    requiresPasswordChange = true;
                    // Don't throw - return response with RequiresPasswordChange flag
                    // Frontend will handle redirecting to password change flow
                }
            }

            // 5. Generate JWT access token
            var accessToken = _jwtTokenService.GenerateAccessToken(user);

            // 6. Generate refresh token
            var refreshTokenString = _jwtTokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            var refreshToken = RefreshToken.Create(user.Id, refreshTokenString, refreshTokenExpiry);

            // 7. Store refresh token in database
            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            // 8. Update user's last login date
            user.RecordLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // 9. Log successful authentication
            await LogSuccessfulAttemptAsync(user.Id, request.Email, ipAddress, userAgent, AuthenticationAction.Login, cancellationToken);

            _logger.LogInformation("Login successful for user {UserId}", user.Id);

            // 10. Return login response
            var userInfo = new UserInfoDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.UserType.ToString()
            );

            return new LoginResponse(
                accessToken,
                refreshTokenString,
                _jwtSettings.AccessTokenExpirationMinutes * 60, // Convert minutes to seconds
                userInfo,
                requiresPasswordChange
            );
        }
        catch (InvalidCredentialsException)
        {
            throw; // Re-throw for controller to handle
        }
        catch (AccountNotActivatedException)
        {
            throw; // Re-throw for controller to handle
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email {Email}", request.Email);
            await LogFailedAttemptAsync(null, request.Email, ipAddress, userAgent, "UnexpectedError", cancellationToken);
            throw; // Let global exception handler deal with it
        }
    }

    private async Task LogSuccessfulAttemptAsync(
        Guid userId,
        string email,
        string ipAddress,
        string? userAgent,
        string action,
        CancellationToken cancellationToken)
    {
        try
        {
            var auditLog = AuthenticationAuditLog.CreateSuccess(userId, email, ipAddress, userAgent, action);
            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            await _auditLogRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Don't fail login if audit logging fails
            _logger.LogError(ex, "Failed to log successful authentication attempt for user {UserId}", userId);
        }
    }

    private async Task LogFailedAttemptAsync(
        Guid? userId,
        string email,
        string ipAddress,
        string? userAgent,
        string failureReason,
        CancellationToken cancellationToken)
    {
        try
        {
            var auditLog = AuthenticationAuditLog.CreateFailure(
                userId,
                email,
                ipAddress,
                userAgent,
                AuthenticationAction.FailedLogin,
                failureReason);

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            await _auditLogRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Don't fail login process if audit logging fails
            _logger.LogError(ex, "Failed to log failed authentication attempt for email {Email}", email);
        }
    }

}

