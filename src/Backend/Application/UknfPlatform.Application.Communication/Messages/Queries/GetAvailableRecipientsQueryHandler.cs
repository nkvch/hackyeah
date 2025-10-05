using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Auth.Enums;

namespace UknfPlatform.Application.Communication.Messages.Queries;

/// <summary>
/// Handler for GetAvailableRecipientsQuery
/// Returns list of users that can receive messages
/// </summary>
public class GetAvailableRecipientsQueryHandler : IRequestHandler<GetAvailableRecipientsQuery, GetAvailableRecipientsResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAvailableRecipientsQueryHandler> _logger;

    public GetAvailableRecipientsQueryHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ILogger<GetAvailableRecipientsQueryHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetAvailableRecipientsResponse> Handle(
        GetAvailableRecipientsQuery request, 
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        
        _logger.LogInformation("Getting available recipients for user {UserId}", currentUserId);

        // Get all active users except current user
        var users = await _userRepository.GetAvailableRecipientsAsync(currentUserId, cancellationToken);

        var recipients = users
            .Select(u => new RecipientDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.UserType == UserType.Internal
            ))
            .ToList();

        _logger.LogInformation("Found {Count} available recipients", recipients.Count);

        return new GetAvailableRecipientsResponse(recipients);
    }
}
