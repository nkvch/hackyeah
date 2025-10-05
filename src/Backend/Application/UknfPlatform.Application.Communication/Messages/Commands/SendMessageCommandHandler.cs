using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Auth.Enums;
using UknfPlatform.Domain.Auth.Interfaces;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Enums;
using UknfPlatform.Domain.Communication.Repositories;

namespace UknfPlatform.Application.Communication.Messages.Commands;

/// <summary>
/// Handler for SendMessageCommand
/// Story 5.1: Send Message with Attachments
/// </summary>
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResponse>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ILogger<SendMessageCommandHandler> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = _currentUserService.UserId;
        
        _logger.LogInformation("Processing send message from user {SenderId} to {RecipientCount} recipients",
            senderId, request.RecipientUserIds.Count);

        // Get sender to determine user type
        var sender = await _userRepository.GetByIdAsync(senderId, cancellationToken);
        if (sender == null)
        {
            throw new UnauthorizedAccessException("Sender user not found");
        }

        var senderIsInternal = sender.UserType == UserType.Internal;

        // Parse context type
        var contextType = ParseContextType(request.ContextType);

        // Create message entity
        var message = Message.Create(
            request.Subject,
            request.Body,
            senderId,
            senderIsInternal,
            contextType,
            request.ContextId,
            request.ParentMessageId
        );

        // Process file attachments
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
                message.Id,
                file.FileName,
                storageKey,
                file.Length,
                file.ContentType);

            message.AddAttachment(attachment);
        }

        // Add recipients
        foreach (var recipientId in request.RecipientUserIds)
        {
            var recipient = MessageRecipient.Create(message.Id, recipientId);
            message.AddRecipient(recipient);
        }

        // Save message
        await _messageRepository.AddAsync(message, cancellationToken);

        // Send email notifications to recipients
        var senderName = $"{sender.FirstName} {sender.LastName}";
        foreach (var recipientId in request.RecipientUserIds)
        {
            var recipient = await _userRepository.GetByIdAsync(recipientId, cancellationToken);
            if (recipient != null)
            {
                await _emailService.SendNewMessageNotificationAsync(
                    recipient.Email,
                    senderName,
                    request.Subject,
                    cancellationToken);
            }
        }

        _logger.LogInformation("Message {MessageId} sent successfully to {RecipientCount} recipients with {AttachmentCount} attachments",
            message.Id, request.RecipientUserIds.Count, request.Attachments.Count);

        return new SendMessageResponse(
            message.Id,
            message.SentDate,
            request.RecipientUserIds.Count,
            request.Attachments.Count,
            $"Message sent successfully to {request.RecipientUserIds.Count} recipient(s)"
        );
    }

    private static MessageContextType ParseContextType(string contextType)
    {
        return contextType switch
        {
            "Standalone" => MessageContextType.Standalone,
            "AccessRequest" => MessageContextType.AccessRequest,
            "Report" => MessageContextType.Report,
            "Case" => MessageContextType.Case,
            _ => throw new ArgumentException($"Invalid context type: {contextType}")
        };
    }
}

