using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;
using UknfPlatform.Application.Shared.Settings;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for RefreshTokenCommand - generates new access token using refresh token
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Find refresh token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found");
            throw new InvalidTokenException("Invalid refresh token");
        }

        // 2. Validate refresh token
        if (!refreshToken.IsValid())
        {
            if (refreshToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token revoked for user {UserId}", refreshToken.UserId);
                throw new InvalidTokenException("Refresh token has been revoked");
            }

            _logger.LogWarning("Refresh token expired for user {UserId}", refreshToken.UserId);
            throw new TokenExpiredException("Refresh token has expired");
        }

        // 3. Get user
        var user = refreshToken.User ?? await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogError("User not found for refresh token: {UserId}", refreshToken.UserId);
            throw new InvalidTokenException("Invalid refresh token");
        }

        // 4. Check if user is still active
        if (!user.IsActive)
        {
            _logger.LogWarning("User account inactive: {UserId}", user.Id);
            throw new AccountNotActivatedException();
        }

        // 5. Generate new access token
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);

        // 6. Optional: Rotate refresh token (security best practice)
        // For simplicity, we'll reuse the existing refresh token
        // In production, consider rotating: revoke old token and create new one

        _logger.LogInformation("Refresh token used successfully for user {UserId}", user.Id);

        var userInfo = new UserInfoDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.UserType.ToString()
        );

        return new LoginResponse(
            newAccessToken,
            request.RefreshToken, // Reuse same refresh token
            _jwtSettings.AccessTokenExpirationMinutes * 60,
            userInfo,
            false // Not checking password expiration on refresh
        );
    }
}

