using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Communication.Repositories;
using UknfPlatform.Domain.Auth.Interfaces;

namespace UknfPlatform.Application.Communication.Messages.Queries;

/// <summary>
/// Handler for GetMessageDetailQuery
/// Story 5.2: Receive and View Messages
/// </summary>
public class GetMessageDetailQueryHandler : IRequestHandler<GetMessageDetailQuery, MessageDetailDto?>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMessageDetailQueryHandler> _logger;

    public GetMessageDetailQueryHandler(
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ILogger<GetMessageDetailQueryHandler> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MessageDetailDto?> Handle(GetMessageDetailQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        
        _logger.LogInformation("Getting message {MessageId} for user {UserId}", 
            request.MessageId, currentUserId);

        // Get message with verification that current user is a recipient
        var message = await _messageRepository.GetMessageDetailForRecipientAsync(
            request.MessageId, 
            currentUserId, 
            cancellationToken);

        if (message == null)
        {
            _logger.LogWarning("Message {MessageId} not found or user {UserId} not authorized", 
                request.MessageId, currentUserId);
            return null;
        }

        // Get recipient record for read status
        var recipient = await _messageRepository.GetRecipientAsync(
            request.MessageId, 
            currentUserId, 
            cancellationToken);

        // Mark message as read if not already
        if (recipient != null && !recipient.IsRead)
        {
            recipient.MarkAsRead();
            await _messageRepository.UpdateAsync(message, cancellationToken);
            _logger.LogInformation("Marked message {MessageId} as read for user {UserId}", 
                request.MessageId, currentUserId);
        }

        // Load sender information
        var sender = await _userRepository.GetByIdAsync(message.SenderId, cancellationToken);
        var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "Unknown Sender";
        var senderEmail = sender?.Email ?? "unknown@example.com";

        // Map attachments to DTOs
        var attachmentDtos = message.Attachments
            .Select(a => new MessageAttachmentDto(
                a.Id,
                a.FileName,
                a.FileSize,
                a.ContentType,
                a.UploadedDate
            ))
            .ToList();

        return new MessageDetailDto(
            message.Id,
            message.Subject,
            message.Body,
            senderName,
            senderEmail,
            message.SentDate,
            message.Status.ToString(),
            true, // Now marked as read
            message.ContextType.ToString(),
            message.ContextId,
            message.ParentMessageId,
            attachmentDtos
        );
    }
}
