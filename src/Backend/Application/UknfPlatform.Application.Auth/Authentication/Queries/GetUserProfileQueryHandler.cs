using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Auth.Authentication.DTOs;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.Application.Auth.Authentication.Queries;

/// <summary>
/// Handler for GetUserProfileQuery - retrieves authenticated user's profile
/// </summary>
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        // Get authenticated user ID from context
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("User not authenticated");

        var userId = _currentUserService.UserId;

        // Retrieve user from repository
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        _logger.LogInformation("Retrieved profile for user {UserId}", userId);

        // Map to DTO (exclude sensitive data)
        return new UserProfileDto(
            UserId: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email,
            PendingEmail: user.PendingEmail,
            PhoneNumber: user.Phone,
            PeselLast4: user.PeselLast4, // Only last 4 digits
            UserType: user.UserType.ToString(),
            CreatedDate: user.CreatedDate,
            LastLoginDate: user.LastLoginDate
        );
    }
}

