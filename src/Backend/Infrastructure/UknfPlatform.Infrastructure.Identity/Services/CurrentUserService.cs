using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;

namespace UknfPlatform.Infrastructure.Identity.Services;

/// <summary>
/// Service for retrieving current authenticated user information from HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current user's ID from JWT claims
    /// </summary>
    public Guid UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            _logger.LogInformation("CurrentUserService.UserId called. HttpContext is null: {IsNull}", httpContext == null);
            
            if (httpContext != null)
            {
                _logger.LogInformation("User.Identity.IsAuthenticated: {IsAuth}", httpContext.User?.Identity?.IsAuthenticated);
                var claims = httpContext.User?.Claims?.ToList();
                _logger.LogInformation("Number of claims: {ClaimCount}", claims?.Count ?? 0);
                if (claims != null)
                {
                    foreach (var claim in claims)
                    {
                        _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }
                }
            }

            var userIdClaim = httpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("UserIdClaim value: {UserIdClaim}", userIdClaim ?? "NULL");

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("No user ID claim found, returning Guid.Empty");
                return Guid.Empty;
            }

            var parsed = Guid.TryParse(userIdClaim, out var userId);
            _logger.LogInformation("Parsed user ID: {UserId}, Success: {Success}", userId, parsed);
            return parsed ? userId : Guid.Empty;
        }
    }

    /// <summary>
    /// Gets the current user's email from JWT claims
    /// </summary>
    public string Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the current user's full name from JWT claims
    /// </summary>
    public string FullName
    {
        get
        {
            var firstName = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
            var lastName = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;

            return $"{firstName} {lastName}".Trim();
        }
    }

    /// <summary>
    /// Gets the current user's phone number from JWT claims
    /// Note: Phone is not currently stored in JWT claims, returns null
    /// </summary>
    public string? Phone => null;

    /// <summary>
    /// Checks if user is authenticated
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    /// <summary>
    /// Checks if user has a specific permission for an entity
    /// </summary>
    public Task<bool> HasPermissionAsync(string permission, long? entityId = null, CancellationToken cancellationToken = default)
    {
        // For now, grant all permissions to authenticated users
        // TODO: Implement proper permission checking when Epic 2 (Authorization) is complete
        return Task.FromResult(IsAuthenticated);
    }
}

