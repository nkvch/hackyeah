using UknfPlatform.Application.Shared.Interfaces;

namespace UknfPlatform.Api.Stubs;

/// <summary>
/// Stub implementation of INotificationService for development.
/// TODO: Replace with SignalR Hub implementation in production.
/// </summary>
public class StubNotificationService : INotificationService
{
    private readonly ILogger<StubNotificationService> _logger;

    public StubNotificationService(ILogger<StubNotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task SendReportStatusUpdateAsync(
        Guid userId,
        Guid reportId,
        string status,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "STUB: Sending notification to user {UserId}: Report {ReportId} status is now {Status}. Message: {Message}",
            userId, reportId, status, message);

        // In production, this would:
        // - Use SignalR to push real-time update to connected user
        // - await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveReportStatusUpdate", reportId, status, message);

        return Task.CompletedTask;
    }

    public Task SendValidationTimeoutNotificationAsync(
        Guid userId,
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "STUB: Sending timeout notification to user {UserId}: Report {ReportId} validation exceeded 24 hours",
            userId, reportId);

        return Task.CompletedTask;
    }
}


