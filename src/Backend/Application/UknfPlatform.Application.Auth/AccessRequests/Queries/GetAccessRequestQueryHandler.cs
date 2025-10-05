using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Auth.AccessRequests.DTOs;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Repositories;
using UknfPlatform.Domain.Shared.Exceptions;

namespace UknfPlatform.Application.Auth.AccessRequests.Queries;

/// <summary>
/// Handler for GetAccessRequestQuery.
/// Returns the current user's access request with pre-populated user data.
/// Story 2.1: Automatic Access Request Creation
/// </summary>
public class GetAccessRequestQueryHandler : IRequestHandler<GetAccessRequestQuery, AccessRequestDto>
{
    private readonly IAccessRequestRepository _accessRequestRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAccessRequestQueryHandler> _logger;

    public GetAccessRequestQueryHandler(
        IAccessRequestRepository accessRequestRepository,
        ICurrentUserService currentUserService,
        ILogger<GetAccessRequestQueryHandler> logger)
    {
        _accessRequestRepository = accessRequestRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<AccessRequestDto> Handle(GetAccessRequestQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to get access request without authenticated user");
            throw new UnauthorizedException("You must be logged in to view your access request");
        }

        var accessRequest = await _accessRequestRepository.GetActiveRequestByUserIdAsync(userId, cancellationToken);

        if (accessRequest == null)
        {
            _logger.LogWarning("No access request found for user {UserId}", userId);
            throw new NotFoundException($"No access request found. Please contact support.");
        }

        // Map to DTO with user data
        return new AccessRequestDto
        {
            Id = accessRequest.Id,
            UserId = accessRequest.UserId,
            Status = accessRequest.Status.ToString(),
            SubmittedDate = accessRequest.SubmittedDate,
            CreatedDate = accessRequest.CreatedDate,
            
            // User data from navigation property
            FirstName = accessRequest.User?.FirstName ?? string.Empty,
            LastName = accessRequest.User?.LastName ?? string.Empty,
            Email = accessRequest.User?.Email ?? string.Empty,
            PhoneNumber = accessRequest.User?.Phone ?? string.Empty,
            PeselMasked = MaskPesel(accessRequest.User?.PeselLast4),
            
            IsEditable = accessRequest.IsEditable(),
            IsVisibleToReviewers = accessRequest.IsVisibleToReviewers()
        };
    }

    private static string MaskPesel(string? peselLast4)
    {
        if (string.IsNullOrEmpty(peselLast4) || peselLast4.Length != 4)
            return "*******";

        // Show only last 4 digits: *******1234
        return "*******" + peselLast4;
    }
}

