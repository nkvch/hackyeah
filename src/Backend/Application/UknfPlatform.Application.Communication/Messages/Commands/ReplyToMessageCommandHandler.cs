using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;
using UknfPlatform.Domain.Communication.Repositories;
using UknfPlatform.Domain.Auth.Enums;

namespace UknfPlatform.Application.Communication.Messages.Commands;

/// <summary>
/// Handler for ReplyToMessageCommand
/// Story 5.3: Reply to Message
/// </summary>
public class ReplyToMessageCommandHandler : IRequestHandler<ReplyToMessageCommand, ReplyToMessageResponse>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ReplyToMessageCommandHandler> _logger;

    public ReplyToMessageCommandHandler(
        IMessageRepository messageRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        ILogger<ReplyToMessageCommandHandler> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReplyToMessageResponse> Handle(ReplyToMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        _logger.LogInformation("User {UserId} replying to message {ParentMessageId}",
            currentUserId, request.ParentMessageId);

        // 1. Fetch parent message
        var parentMessage = await _messageRepository.GetMessageDetailForRecipientAsync(
            request.ParentMessageId,
            currentUserId,
            cancellationToken);

        if (parentMessage == null)
        {
            _logger.LogWarning("Parent message {ParentMessageId} not found or user {UserId} not authorized",
                request.ParentMessageId, currentUserId);
            throw new UnauthorizedAccessException("You are not authorized to reply to this message.");
        }

        // 2. Determine reply subject (add "Re:" prefix if not already present)
        var replySubject = parentMessage.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase)
            ? parentMessage.Subject
            : $"Re: {parentMessage.Subject}";

        // 3. Determine sender type by checking if current user was the original sender (External)
        //    or is replying as UKNF employee (Internal)
        //    Simplified: If replying to own message = External, else = Internal
        var senderIsInternal = currentUserId != parentMessage.SenderId;

        // 4. Create reply message (inherit context from parent)
        //    The Create method automatically sets the correct status based on senderIsInternal
        var replyMessage = Message.Create(
            replySubject,
            request.Body,
            currentUserId,
            senderIsInternal,
            parentMessage.ContextType,
            parentMessage.ContextId,
            request.ParentMessageId);

        // 6. Process file attachments
        if (request.Attachments != null && request.Attachments.Any())
        {
            foreach (var file in request.Attachments)
            {
                _logger.LogInformation("Uploading attachment: {FileName} ({FileSize} bytes)",
                    file.FileName, file.Length);

                using var stream = file.OpenReadStream();
                var storageKey = await _fileStorageService.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    cancellationToken);

                var attachment = MessageAttachment.Create(
                    replyMessage.Id,
                    file.FileName,
                    storageKey,
                    file.Length,
                    file.ContentType);

                replyMessage.AddAttachment(attachment);
            }
        }

        // 7. Add recipient (sender of parent message)
        var recipientUserId = parentMessage.SenderId;
        var recipient = MessageRecipient.Create(replyMessage.Id, recipientUserId);
        replyMessage.AddRecipient(recipient);

        // 8. Save reply message
        await _messageRepository.AddAsync(replyMessage, cancellationToken);

        // 9. Update parent message status to Closed
        parentMessage.Close();
        await _messageRepository.UpdateAsync(parentMessage, cancellationToken);

        _logger.LogInformation("Reply message {ReplyMessageId} created successfully for parent {ParentMessageId}",
            replyMessage.Id, request.ParentMessageId);

        // 11. Return response
        return new ReplyToMessageResponse(
            replyMessage.Id,
            request.ParentMessageId,
            replyMessage.SentDate,
            "Recipient Name", // TODO: Load recipient name in future
            replyMessage.Attachments.Count,
            "Reply sent successfully");
    }
}

