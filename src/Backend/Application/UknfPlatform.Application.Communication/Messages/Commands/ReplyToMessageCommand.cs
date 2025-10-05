using MediatR;
using Microsoft.AspNetCore.Http;

namespace UknfPlatform.Application.Communication.Messages.Commands;

/// <summary>
/// Command to reply to an existing message
/// Story 5.3: Reply to Message
/// </summary>
public class ReplyToMessageCommand : IRequest<ReplyToMessageResponse>
{
    /// <summary>
    /// ID of the message being replied to
    /// </summary>
    public Guid ParentMessageId { get; set; }

    /// <summary>
    /// Reply message body (required)
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Optional file attachments (same rules as Story 5.1)
    /// </summary>
    public List<IFormFile>? Attachments { get; set; }
}

/// <summary>
/// Response after successfully sending a reply
/// </summary>
public record ReplyToMessageResponse(
    Guid ReplyMessageId,
    Guid ParentMessageId,
    DateTime SentDate,
    string RecipientName,
    int AttachmentCount,
    string Message);

