using MediatR;
using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;
using UknfPlatform.Domain.Communication.Repositories;

namespace UknfPlatform.Application.Communication.Messages.Queries;

/// <summary>
/// Handler for GetMessagesQuery
/// Story 5.2: Receive and View Messages (MVP)
/// </summary>
public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, GetMessagesResponse>
{
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMessagesQueryHandler> _logger;

    public GetMessagesQueryHandler(
        IMessageRepository messageRepository,
        ICurrentUserService currentUserService,
        ILogger<GetMessagesQueryHandler> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetMessagesResponse> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        
        _logger.LogInformation("Getting messages for user {UserId}, page {PageNumber}", 
            currentUserId, request.PageNumber);

        // Get messages with pagination from repository
        var (messages, totalCount) = await _messageRepository.GetMessagesForRecipientAsync(
            currentUserId, 
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Map to DTOs
        var messageSummaries = messages.Select(m =>
        {
            var recipient = m.Recipients.FirstOrDefault(r => r.RecipientUserId == currentUserId);
            
            return new MessageSummaryDto(
                m.Id,
                m.Subject,
                m.Body.Length > 200 ? m.Body.Substring(0, 200) + "..." : m.Body,
                "Unknown Sender", // TODO: Load sender info in Story 5.2.1
                "unknown@example.com",
                m.SentDate,
                m.Status.ToString(),
                recipient?.IsRead ?? false,
                m.Attachments.Count,
                m.ContextType.ToString(),
                m.ContextId
            );
        }).ToList();

        _logger.LogInformation("Found {Count} messages for user {UserId}", messageSummaries.Count, currentUserId);

        return new GetMessagesResponse(
            messageSummaries,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages
        );
    }
}
