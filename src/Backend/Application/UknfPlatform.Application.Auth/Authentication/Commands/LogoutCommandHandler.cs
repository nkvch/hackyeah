using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Domain.Auth.Interfaces;

namespace UknfPlatform.Application.Auth.Authentication.Commands;

/// <summary>
/// Handler for LogoutCommand - revokes refresh token
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthenticationAuditLogRepository _auditLogRepository;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IAuthenticationAuditLogRepository auditLogRepository,
        ILogger<LogoutCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("Logout attempted with empty refresh token");
            return Unit.Value; // Idempotent - don't fail
        }

        // Find and revoke refresh token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null)
        {
            _logger.LogWarning("Logout attempted with non-existent refresh token");
            return Unit.Value; // Idempotent - don't fail
        }

        // Revoke token
        refreshToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User logged out successfully: {UserId}", refreshToken.UserId);

        // Optional: Log logout event in audit log
        // (Implementation left as future enhancement)

        return Unit.Value;
    }
}

