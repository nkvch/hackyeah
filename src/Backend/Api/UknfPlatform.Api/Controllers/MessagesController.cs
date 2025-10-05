using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UknfPlatform.Application.Communication.Messages.Commands;

namespace UknfPlatform.Api.Controllers;

/// <summary>
/// API for managing messages between UKNF employees and external users
/// Story 5.1: Send Message with Attachments
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
    /// Gets a message by ID
    /// </summary>
    /// <param name="id">Message ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Message details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMessage(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement in Story 5.2
        return NotFound(new { error = "Message retrieval not yet implemented" });
    }
}

