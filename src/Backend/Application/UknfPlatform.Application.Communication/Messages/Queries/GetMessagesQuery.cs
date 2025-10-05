using MediatR;

namespace UknfPlatform.Application.Communication.Messages.Queries;

/// <summary>
/// Query to get messages for current user
/// Story 5.2: Receive and View Messages (MVP)
/// </summary>
public record GetMessagesQuery : IRequest<GetMessagesResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Response with list of messages
/// </summary>
public record GetMessagesResponse(
    List<MessageSummaryDto> Messages,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Summary DTO for message list
/// </summary>
public record MessageSummaryDto(
    Guid MessageId,
    string Subject,
    string BodyPreview,
    string SenderName,
    string SenderEmail,
    DateTime SentDate,
    string MessageStatus,
    bool IsRead,
    int AttachmentCount,
    string? ContextType,
    Guid? ContextId
);

