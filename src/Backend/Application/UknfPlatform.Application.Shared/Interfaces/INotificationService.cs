namespace UknfPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service for sending real-time notifications to users.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a report status update notification to a user.
    /// </summary>
    Task SendReportStatusUpdateAsync(
        Guid userId, 
        Guid reportId, 
        string status, 
        string message, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a validation timeout notification to a user.
    /// </summary>
    Task SendValidationTimeoutNotificationAsync(
        Guid userId,
        Guid reportId,
        CancellationToken cancellationToken = default);
}


