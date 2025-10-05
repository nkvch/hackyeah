using MediatR;

namespace UknfPlatform.Application.Communication.Messages.Queries;

/// <summary>
/// Query to get available recipients for messaging
/// Story 5.1/5.2: Get list of users that can receive messages
/// </summary>
public record GetAvailableRecipientsQuery : IRequest<GetAvailableRecipientsResponse>;

/// <summary>
/// Response with list of available recipients
/// </summary>
public record GetAvailableRecipientsResponse(
    List<RecipientDto> Recipients
);

/// <summary>
/// DTO for recipient user information
/// </summary>
public record RecipientDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    bool IsInternal
);

