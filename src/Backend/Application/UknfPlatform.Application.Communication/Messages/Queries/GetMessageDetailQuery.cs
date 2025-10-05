using MediatR;

namespace UknfPlatform.Application.Communication.Messages.Queries;

/// <summary>
/// Query to get single message detail
/// Story 5.2: Receive and View Messages
/// </summary>
public record GetMessageDetailQuery(Guid MessageId) : IRequest<MessageDetailDto>;

/// <summary>
/// Detailed DTO for single message view
/// </summary>
public record MessageDetailDto(
    Guid MessageId,
    string Subject,
    string Body,
    string SenderName,
    string SenderEmail,
    DateTime SentDate,
    string MessageStatus,
    bool IsRead,
    string ContextType,
    Guid? ContextId,
    Guid? ParentMessageId,
    List<MessageAttachmentDto> Attachments
);

/// <summary>
/// DTO for message attachments
/// </summary>
public record MessageAttachmentDto(
    Guid AttachmentId,
    string FileName,
    long FileSize,
    string? ContentType,
    DateTime UploadedDate
);

