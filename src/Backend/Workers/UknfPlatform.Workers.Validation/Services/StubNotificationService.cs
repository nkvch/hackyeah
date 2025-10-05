using Microsoft.Extensions.Logging;
using UknfPlatform.Application.Shared.Interfaces;

namespace UknfPlatform.Workers.Validation.Services;

/// <summary>
/// Stub implementation of INotificationService for worker.
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
            "STUB NOTIFICATION: User {UserId} - Report {ReportId} status: {Status}. {Message}",
            userId, reportId, status, message);

        return Task.CompletedTask;
    }

    public Task SendValidationTimeoutNotificationAsync(
        Guid userId,
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "STUB NOTIFICATION: User {UserId} - Report {ReportId} validation timed out (>24 hours)",
            userId, reportId);

        return Task.CompletedTask;
    }
}


