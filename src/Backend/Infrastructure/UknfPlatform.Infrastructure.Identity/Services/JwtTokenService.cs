using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Entities;
using UknfPlatform.Application.Shared.Settings;

namespace UknfPlatform.Infrastructure.Identity.Services;

/// <summary>
/// JWT token service implementation for generating and validating JWT tokens
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IOptions<JwtSettings> jwtSettings,
        ILogger<JwtTokenService> logger)
    {
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate JWT settings on startup
        ValidateSettings();
    }

    /// <summary>
    /// Generates a JWT access token for a user with claims
    /// </summary>
    public string GenerateAccessToken(User user, IEnumerable<string>? roles = null)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim("UserType", user.UserType.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token identifier
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Issued at
        };

        // Add roles as claims if provided
        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "Generated access token for user {UserId} with expiration {ExpirationMinutes} minutes",
            user.Id,
            _jwtSettings.AccessTokenExpirationMinutes);

        return tokenString;
    }

    /// <summary>
    /// Generates a secure random refresh token using cryptographic random number generator
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var refreshToken = Convert.ToBase64String(randomBytes);

        _logger.LogDebug("Generated new refresh token");

        return refreshToken;
    }

    /// <summary>
    /// Validates a JWT token's signature, expiration, issuer, and audience
    /// </summary>
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No grace period - enforce exact expiration
            }, out SecurityToken validatedToken);

            _logger.LogDebug("Token validation successful");
            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogDebug("Token validation failed: Token expired");
            return false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogWarning("Token validation failed: Invalid signature");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Extracts the user ID from a JWT token
    /// </summary>
    public Guid GetUserIdFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new ArgumentException("Token does not contain user ID claim");
            }

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new ArgumentException("Invalid user ID format in token");
            }

            return userId;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Failed to extract user ID from token");
            throw new ArgumentException("Invalid token format", nameof(token), ex);
        }
    }

    /// <summary>
    /// Validates JWT settings configuration on startup
    /// </summary>
    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured");
        }

        if (_jwtSettings.SecretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters (256 bits) for HMACSHA256");
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
        {
            throw new InvalidOperationException("JWT Issuer is not configured");
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
        {
            throw new InvalidOperationException("JWT Audience is not configured");
        }

        if (_jwtSettings.AccessTokenExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT AccessTokenExpirationMinutes must be greater than 0");
        }

        if (_jwtSettings.RefreshTokenExpirationDays <= 0)
        {
            throw new InvalidOperationException("JWT RefreshTokenExpirationDays must be greater than 0");
        }

        _logger.LogInformation(
            "JWT settings validated: Issuer={Issuer}, Audience={Audience}, AccessExpiration={AccessMinutes}min, RefreshExpiration={RefreshDays}days",
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            _jwtSettings.AccessTokenExpirationMinutes,
            _jwtSettings.RefreshTokenExpirationDays);
    }
}

