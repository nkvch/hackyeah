using MediatR;
using Microsoft.AspNetCore.Http;

namespace UknfPlatform.Application.Communication.Messages.Commands;

/// <summary>
/// Command to send a message with attachments
/// Story 5.1: Send Message with Attachments
/// </summary>
public record SendMessageCommand : IRequest<SendMessageResponse>
{
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public List<Guid> RecipientUserIds { get; init; } = new();
    public string ContextType { get; init; } = "Standalone"; // Standalone, AccessRequest, Report, Case
    public Guid? ContextId { get; init; }
    public Guid? ParentMessageId { get; init; }
    public List<IFormFile> Attachments { get; init; } = new();
}

/// <summary>
/// Response after sending a message
/// </summary>
public record SendMessageResponse(
    Guid MessageId,
    DateTime SentDate,
    int RecipientCount,
    int AttachmentCount,
    string Message
);

