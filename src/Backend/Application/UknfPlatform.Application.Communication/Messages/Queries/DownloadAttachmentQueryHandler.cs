using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Communication.Repositories;

namespace UknfPlatform.Application.Communication.Messages.Queries;

/// <summary>
/// Handler for DownloadAttachmentQuery
/// Story 5.2: Download message attachments
/// </summary>
public class DownloadAttachmentQueryHandler : IRequestHandler<DownloadAttachmentQuery, DownloadAttachmentResponse?>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DownloadAttachmentQueryHandler> _logger;

    public DownloadAttachmentQueryHandler(
        IMessageRepository messageRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        ILogger<DownloadAttachmentQueryHandler> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DownloadAttachmentResponse?> Handle(DownloadAttachmentQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        
        _logger.LogInformation("User {UserId} downloading attachment {AttachmentId} from message {MessageId}", 
            currentUserId, request.AttachmentId, request.MessageId);

        // Verify user is a recipient of this message
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

        // Find the attachment
        var attachment = message.Attachments.FirstOrDefault(a => a.Id == request.AttachmentId);
        if (attachment == null)
        {
            _logger.LogWarning("Attachment {AttachmentId} not found in message {MessageId}", 
                request.AttachmentId, request.MessageId);
            return null;
        }

        // Download file from storage
        var fileStream = await _fileStorageService.DownloadFileAsync(attachment.FileStorageKey, cancellationToken);
        if (fileStream == null)
        {
            _logger.LogError("File not found in storage: {FileStorageKey}", attachment.FileStorageKey);
            return null;
        }

        _logger.LogInformation("Successfully retrieved attachment {AttachmentId} for user {UserId}", 
            request.AttachmentId, currentUserId);

        return new DownloadAttachmentResponse(
            attachment.FileName,
            attachment.ContentType ?? "application/octet-stream",
            fileStream
        );
    }
}

