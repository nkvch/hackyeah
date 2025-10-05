using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UknfPlatform.Application.Communication.Messages.Commands;
using UknfPlatform.Application.Communication.Messages.Queries;

namespace UknfPlatform.Api.Controllers;

/// <summary>
/// API for managing messages between UKNF employees and external users
/// Story 5.1: Send Message with Attachments
/// Story 5.2: Receive and View Messages
/// </summary>
[Authorize]
[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMediator mediator, ILogger<MessagesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets list of available recipients for messaging
    /// Story 5.1: Compose message - get recipients
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available recipients</returns>
    [HttpGet("recipients")]
    [ProducesResponseType(typeof(GetAvailableRecipientsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetAvailableRecipientsResponse>> GetAvailableRecipients(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableRecipientsQuery();
        var response = await _mediator.Send(query, cancellationToken);
        
        _logger.LogInformation("Retrieved {Count} available recipients", response.Recipients.Count);
        
        return Ok(response);
    }

    /// <summary>
    /// Sends a message with optional file attachments
    /// </summary>
    /// <param name="command">Message details and attachments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Message send confirmation</returns>
    /// <response code="201">Message sent successfully</response>
    /// <response code="400">Validation failed (invalid file format, size exceeded, etc.)</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(SendMessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(104_857_600)] // 100 MB max request size
    public async Task<ActionResult<SendMessageResponse>> SendMessage(
        [FromForm] SendMessageCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Message {MessageId} sent by user via API", response.MessageId);
            
            return CreatedAtAction(
                nameof(GetMessage),
                new { id = response.MessageId },
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets list of messages for current user
    /// Story 5.2: Receive and View Messages
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of messages</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetMessagesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetMessagesResponse>> GetMessages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMessagesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var response = await _mediator.Send(query, cancellationToken);
        
        _logger.LogInformation("Retrieved {Count} messages for user", response.Messages.Count);
        
        return Ok(response);
    }

    /// <summary>
    /// Gets a message by ID
    /// Story 5.2: Receive and View Messages
    /// </summary>
    /// <param name="id">Message ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Message details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MessageDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MessageDetailDto>> GetMessage(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var query = new GetMessageDetailQuery(id);
        var message = await _mediator.Send(query, cancellationToken);

        if (message == null)
        {
            _logger.LogWarning("Message {MessageId} not found or not authorized", id);
            return NotFound(new { error = "Message not found" });
        }

        _logger.LogInformation("Retrieved message {MessageId}", id);
        
        return Ok(message);
    }

    /// <summary>
    /// Downloads an attachment from a message
    /// Story 5.2: Download message attachments
    /// </summary>
    /// <param name="id">Message ID</param>
    /// <param name="attachmentId">Attachment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Attachment file</returns>
    [HttpGet("{id}/attachments/{attachmentId}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DownloadAttachment(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var query = new DownloadAttachmentQuery(id, attachmentId);
        var response = await _mediator.Send(query, cancellationToken);

        if (response == null)
        {
            _logger.LogWarning("Attachment {AttachmentId} not found or not authorized", attachmentId);
            return NotFound(new { error = "Attachment not found" });
        }

        _logger.LogInformation("Downloading attachment {AttachmentId} from message {MessageId}", attachmentId, id);

        return File(response.FileStream, response.ContentType, response.FileName);
    }

    /// <summary>
    /// Replies to an existing message
    /// Story 5.3: Reply to Message
    /// </summary>
    /// <param name="id">Parent message ID</param>
    /// <param name="command">Reply details and attachments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reply confirmation</returns>
    [HttpPost("{id}/reply")]
    [ProducesResponseType(typeof(ReplyToMessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(104_857_600)] // 100 MB max request size
    public async Task<ActionResult<ReplyToMessageResponse>> ReplyToMessage(
        Guid id,
        [FromForm] ReplyToMessageCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Set parent message ID from route parameter
            command.ParentMessageId = id;

            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Reply {ReplyMessageId} sent to message {ParentMessageId}", 
                response.ReplyMessageId, id);
            
            return CreatedAtAction(
                nameof(GetMessage),
                new { id = response.ReplyMessageId },
                response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User not authorized to reply to message {MessageId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying to message {MessageId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }
}

